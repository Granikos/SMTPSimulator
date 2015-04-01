using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HydraCore;

namespace HydraService
{
    class SMTPServer
    {
        private readonly TcpListener _tcpListener;
        private Thread _listenThread;
        private SMTPCore _core;

        public SMTPServer(IPEndPoint endPoint, SMTPCore core)
        {
            _tcpListener = new TcpListener(endPoint);
            _core = core;

            _core.OnNewMessage += (transaction, sender, recipients, body) =>
            {
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("New message from <" + sender + ">");
                Console.WriteLine("Recipients: " + String.Join(", ", recipients));
                Console.WriteLine();
                Console.WriteLine(body);
                Console.WriteLine("--------------------------------------");
            };
        }

        public void Start()
        {
            if (_listenThread == null)
            {
                _listenThread = new Thread(ListenForClients);
                _listenThread.Start();
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
        }

        private void ListenForClients()
        {
            _tcpListener.Start();

            while (true)
            {
                TcpClient client = _tcpListener.AcceptTcpClient();

                Thread clientThread = new Thread(HandleClientConnection);
                clientThread.Start(client);
            }
        }
        private async void HandleClientConnection(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            var ip = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;

            Console.WriteLine("New client: " + ip);

            SMTPResponse response;
            var transaction = _core.StartTransaction(ip, out response);

            var writer = new StreamWriter(clientStream, Encoding.ASCII, 1000, true) { NewLine = "\r\n" };
            var reader = new StreamReader(clientStream, Encoding.ASCII, false, 1000, true);

            await writer.WriteLineAsync(response.ToString());
            writer.Flush();

            while(!reader.EndOfStream && !transaction.Closed)
            {
                var line = await reader.ReadLineAsync();
                var parts = line.Split(new [] {' '}, 2);

                var verb = parts[0].ToUpperInvariant();
                var command = parts.Length > 1? new SMTPCommand(verb, parts[1]) : new SMTPCommand(verb);

                response = transaction.ExecuteCommand(command);
                await writer.WriteLineAsync(response.ToString());
                writer.Flush();

                while (transaction.DataMode)
                {
                    var data = new StringBuilder();
                    var first = true;

                    while(true)
                    {
                        line = await reader.ReadLineAsync();

                        if (line.Equals(".")) break;
                        if (line.StartsWith(".")) line = line.Substring(1);

                        data.Append((first? "" : "\r\n") + line);
                        first = false;
                    }

                    response = transaction.HandleData(data.ToString());
                    await writer.WriteLineAsync(response.ToString());
                    writer.Flush();
                }
            }

            writer.Close();
            reader.Close();

            tcpClient.Close();
        }
    }
}