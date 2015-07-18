using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using HydraCore;
using HydraService.Models;

namespace HydraService
{
    internal class SMTPServer
    {
        private readonly CompositionContainer _container;
        private TcpListener _tcpListener;
        private Thread _listenThread;
        private Thread _processThread;
        private MessageSender _sender;

        private Dictionary<IPAddress,DateTime> _greyList = new Dictionary<IPAddress, DateTime>(); 

        public SMTPServer(RecieveConnector connector, SMTPCore core, CompositionContainer container)
        {
            _container = container;
            LocalEndpoint = new IPEndPoint(connector.Address, connector.Port);
            Core = core;

            Connector = connector;
            Settings = new DefaultRecieveSettings(connector);

            // _sender = new MessageSender(container);

            Core.OnConnect += (transaction, connect) =>
            {
                if (connector.RemoteIPRanges.Any() && !connector.RemoteIPRanges.Any(range => range.Contains(connect.IP)))
                {
                    connect.Cancel = true;
                }

                CheckGreylisting(connect);
            };

            Core.OnNewMessage += (transaction, mail) =>
            {
                // _sender.Enqueue(mail);

                Console.WriteLine("--------------------------------------");
                Console.WriteLine("New message from " + mail.From);
                Console.WriteLine("Recipients: " + string.Join(", ", mail.Recipients.Select(r => r.ToString())));
                // Console.WriteLine();
                // Console.WriteLine(body);
                Console.WriteLine("--------------------------------------");
            };

            _container.SatisfyImportsOnce(this);

            // container.SatisfyImportsOnce(_sender);
        }

        private void CheckGreylisting(SMTPCore.ConnectEventArgs connect)
        {
            if (Connector.GreylistingTime != null && Connector.GreylistingTime > TimeSpan.Zero)
            {
                DateTime time;

                if (_greyList.TryGetValue(connect.IP, out time))
                {
                    if (time > DateTime.Now)
                    {
                        connect.Cancel = true;
                        connect.ResponseCode = SMTPStatusCode.NotAvailiable;
                    }
                    else
                    {
                        _greyList.Remove(connect.IP);
                    }
                }
                else
                {
                    _greyList.Add(connect.IP, (DateTime)(DateTime.Now + Connector.GreylistingTime));
                    connect.Cancel = true;
                    connect.ResponseCode = SMTPStatusCode.NotAvailiable;
                }
            }
        }

        public SMTPCore Core { get; private set; }
        public IReceiveSettings Settings { get; private set; }
        public RecieveConnector Connector { get; private set; }
        public IPEndPoint LocalEndpoint { get; private set; }

        public void Start()
        {
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
            }

            _tcpListener = new TcpListener(LocalEndpoint);

            if (_listenThread == null)
            {
                _listenThread = new Thread(ListenForClients);
                _listenThread.Start();
            }

            if (_processThread == null)
            {
                // _processThread = new Thread(ProcessMail);
                // _processThread.Start();
            }
        }

        public void Stop()
        {
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
                _tcpListener = null;
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
            try
            {
                _tcpListener.Start();

                while (true)
                {
                    var client = _tcpListener.AcceptTcpClient();

                    var clientThread = new Thread(HandleClientConnection);
                    clientThread.Start(client);
                }
            }
            catch (SocketException e)
            {
                if ((e.SocketErrorCode != SocketError.Interrupted)) throw;
            }
        }

        private void HandleClientConnection(object obj)
        {
            var handler = new ClientHandler((TcpClient) obj, this);
            _container.SatisfyImportsOnce(handler);
            handler.Process().Wait();
        }

        private void ProcessMail()
        {
            while (true)
            {
                // _sender.ProcessMail();
            }
        }
    }
}