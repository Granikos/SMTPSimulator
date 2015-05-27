using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using HydraCore;

namespace HydraService
{
    internal class SMTPServer
    {
        private readonly ICollection<IPSubnet> _allowedSubnets = new HashSet<IPSubnet>();
        private readonly SMTPCore _core;
        private readonly TcpListener _tcpListener;
        private Thread _listenThread;
        private Thread _processThread;
        private X509Certificate2 _cert;

        MessageSender _sender = new MessageSender();

        public SMTPServer(IPEndPoint endPoint, SMTPCore core, CompositionContainer container)
        {
            _tcpListener = new TcpListener(endPoint);
            _core = core;

            var store = new X509Store("Personal", StoreLocation.LocalMachine);
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", true);

            _cert = new X509Certificate2("cert.pfx", "tester"); // TODO: make configurable

            _core.OnConnect += (transaction, connect) =>
            {
                if (_allowedSubnets.Any() && !IsAllowedIP(connect.IP))
                {
                    connect.Cancel = true;
                }
            };

            _core.OnNewMessage += (transaction, mail) =>
            {
                _sender.Enqueue(mail);

                Console.WriteLine("--------------------------------------");
                Console.WriteLine("New message from " + mail.From);
                Console.WriteLine("Recipients: " + String.Join(", ", mail.Recipients.Select(r => r.ToString())));
                // Console.WriteLine();
                // Console.WriteLine(body);
                Console.WriteLine("--------------------------------------");
            };

            container.SatisfyImportsOnce(_sender);
        }

        public bool UseSsl { get; set; }

        public void AddSubnet(IPSubnet subnet)
        {
            _allowedSubnets.Add(subnet);
        }

        public void RemoveSubnet(IPSubnet subnet)
        {
            _allowedSubnets.Remove(subnet);
        }

        public bool IsAllowedIP(IPAddress address)
        {
            return _allowedSubnets.Any(s => s.Contains(address));
        }

        public void Start()
        {
            if (_listenThread == null)
            {
                _listenThread = new Thread(ListenForClients);
                _listenThread.Start();
            }

            if (_processThread == null)
            {
                _processThread = new Thread(ProcessMail);
                _processThread.Start();
            }
        }

        public void Stop()
        {
            // TODO: Cleaner solution
            if (_listenThread != null)
            {
                _listenThread.Abort();
                _listenThread = null;
            }

            // TODO: Cleaner solution
            if (_processThread != null)
            {
                _processThread.Abort();
                _processThread = null;
            }
        }

        private void ListenForClients()
        {
            _tcpListener.Start();

            while (true)
            {
                var client = _tcpListener.AcceptTcpClient();

                var clientThread = new Thread(HandleClientConnection);
                clientThread.Start(client);
            }
        }

        private void ProcessMail()
        {
            while (true)
            {
                _sender.ProcessMail();
            }
        }

        private async void HandleClientConnection(object client)
        {
            var tcpClient = (TcpClient) client;
            SslStream sslStream = null;
            Stream clientStream = tcpClient.GetStream();

            var starttls = false;

            var ip = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address;

            Console.WriteLine("New client: " + ip);

            SMTPResponse response;
            var transaction = _core.StartTransaction(ip, out response);

            if (UseSsl)
            {
                sslStream = new SslStream(clientStream);

                await sslStream.AuthenticateAsServerAsync(_cert, false, SslProtocols.Ssl2, false);

                transaction.TLSEnabled = true;
            }

            SMTPTransaction.CloseAction onCloseHandler = smtpTransaction => {
                Console.WriteLine("Client " + ip + " disconnected");
            };

            var writer = new StreamWriter(sslStream ?? clientStream, Encoding.ASCII, 1000, true) { NewLine = "\r\n" };
            var reader = new StreamReader(sslStream ?? clientStream, Encoding.ASCII, false, 1000, true);

            transaction.OnStartTLS += (sender, args) =>
            {
                starttls = true;
            };
            transaction.OnClose += onCloseHandler;

            await writer.WriteLineAsync(response.ToString());
            writer.Flush();

            try
            {
                while (!reader.EndOfStream && !transaction.Closed)
                {
                    var line = await reader.ReadLineAsync();
                    Console.WriteLine("[{0}] > {1}", ip, line);
                    var parts = line.Split(new[] { ' ' }, 2);

                    var verb = parts[0].ToUpperInvariant();
                    var command = parts.Length > 1 ? new SMTPCommand(verb, parts[1]) : new SMTPCommand(verb);

                    response = transaction.ExecuteCommand(command);
                    foreach (var l in response.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None))
                    {
                        Console.WriteLine("[{0}] < {1}", ip, l);
                    }
                    await writer.WriteLineAsync(response.ToString());
                    writer.Flush();


                    while (transaction.InDataMode)
                    {
                        var data = new StringBuilder();

                        do
                        {
                            line = await reader.ReadLineAsync();
                            // Console.WriteLine("[{0}] > {1}", ip, line);
                        } while (transaction.HandleDataLine(line, data));

                        response = transaction.HandleData(data.ToString());
                        await writer.WriteLineAsync(response.ToString());
                        writer.Flush();
                    }

                    if (starttls)
                    {
                        sslStream = new SslStream(clientStream);

                        sslStream.AuthenticateAsServer(_cert, false, SslProtocols.Tls, false);

                        Console.WriteLine("[{0}] Established TLS Layer", ip);

                        writer.Close();
                        reader.Close();

                        writer = new StreamWriter(sslStream, Encoding.ASCII, 1000, true) { NewLine = "\r\n" };
                        reader = new StreamReader(sslStream, Encoding.ASCII, false, 1000, true);

                        transaction.OnClose -= onCloseHandler;
                        transaction.Close();
                        transaction = _core.StartTransaction(ip, out response);
                        transaction.TLSEnabled = true;
                        transaction.OnClose += onCloseHandler;

                        starttls = false;
                    }
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                transaction.Close();
                writer.Close();
                reader.Close();

                clientStream.Close();
                if (sslStream != null) sslStream.Close();

                tcpClient.Close();
            }
        }
    }
}