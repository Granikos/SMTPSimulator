using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Granikos.Hydra.Core;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.SmtpServer;
using log4net;

namespace Granikos.Hydra.Service
{
    internal class SMTPService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SMTPService));
        private readonly CompositionContainer _container;
        private readonly Dictionary<IPAddress, DateTime> _greyList = new Dictionary<IPAddress, DateTime>();
        private Thread _listenThread;
        private TcpListener _tcpListener;

        public SMTPService(ReceiveConnector connector, SMTPServer smtpServer, CompositionContainer container)
        {
            _container = container;
            LocalEndpoint = new IPEndPoint(connector.Address, connector.Port);
            SMTPServer = smtpServer;

            Connector = connector;
            Settings = new DefaultReceiveSettings(connector);

            SMTPServer.OnConnect += (transaction, connect) =>
            {
                if (connector.RemoteIPRanges.Any() && !connector.RemoteIPRanges.Any(range => range.Contains(connect.IP)))
                {
                    connect.Cancel = true;
                }

                CheckGreylisting(connect);
            };

            SMTPServer.OnNewMessage += (transaction, mail) =>
            {
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("New message from " + mail.From);
                Console.WriteLine("Recipients: " + string.Join(", ", mail.Recipients.Select(r => r.ToString())));
                // Console.WriteLine();
                // Console.WriteLine(body);
                Console.WriteLine("--------------------------------------");
            };

            _container.SatisfyImportsOnce(this);
        }

        public SMTPServer SMTPServer { get; private set; }
        public IReceiveSettings Settings { get; private set; }
        public ReceiveConnector Connector { get; private set; }
        public IPEndPoint LocalEndpoint { get; private set; }

        private void CheckGreylisting(SMTPServer.ConnectEventArgs connect)
        {
            if (Connector.GreylistingTime != null && Connector.GreylistingTime > TimeSpan.Zero)
            {
                DateTime time;

                if (_greyList.TryGetValue(connect.IP, out time))
                {
                    if (time > DateTime.Now)
                    {
                        Console.WriteLine("Greylisting activate, time left: " + (time - DateTime.Now));
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
                    Console.WriteLine("Greylisting started, time left: " + Connector.GreylistingTime);
                    _greyList.Add(connect.IP, (DateTime) (DateTime.Now + Connector.GreylistingTime));
                    connect.Cancel = true;
                    connect.ResponseCode = SMTPStatusCode.NotAvailiable;
                }
            }
        }

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
        }

        public void Stop()
        {
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
                _tcpListener = null;
            }
        }

        private void ListenForClients()
        {
            try
            {
                _tcpListener.Start();

                while (true)
                {
                    try
                    {
                        var client = _tcpListener.AcceptTcpClient();

                        var clientThread = new Thread(HandleClientConnection);
                        clientThread.Start(client);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("An error while listening for clients.", e);
                    }
                }
            }
            catch (SocketException e)
            {
                if ((e.SocketErrorCode != SocketError.Interrupted)) throw;
            }
        }

        private void HandleClientConnection(object obj)
        {
            try
            {
                var handler = new ClientHandler((TcpClient) obj, this);
                _container.SatisfyImportsOnce(handler);
                handler.Process().Wait();
            }
            catch (Exception e)
            {
                Logger.Error("An error while handling a client connection.", e);
            }
        }
    }
}