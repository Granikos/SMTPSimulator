using System;
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
        private readonly TcpListener _tcpListener;
        private Thread _listenThread;
        private Thread _processThread;
        private MessageSender _sender;

        public SMTPServer(RecieveConnector connector, SMTPCore core, CompositionContainer container)
        {
            _container = container;
            LocalEndpoint = new IPEndPoint(connector.Address, connector.Port);
            _tcpListener = new TcpListener(LocalEndpoint);
            Core = core;

            Connector = connector;
            Settings = new DefaultSettings(connector);

            TLSConnector = new TLSConnector(connector.TLSSettings);

            // _sender = new MessageSender(container);

            Core.OnConnect += (transaction, connect) =>
            {
                if (connector.RemoteIPRanges.Any() && !connector.RemoteIPRanges.Any(range => range.Contains(connect.IP)))
                {
                    connect.Cancel = true;
                }
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

        public SMTPCore Core { get; private set; }
        public ISettings Settings { get; private set; }
        public RecieveConnector Connector { get; private set; }
        public TLSConnector TLSConnector { get; private set; }
        public IPEndPoint LocalEndpoint { get; private set; }
        public bool UseSsl { get; set; }

        public void Start()
        {
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