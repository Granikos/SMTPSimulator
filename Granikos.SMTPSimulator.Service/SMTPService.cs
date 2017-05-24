// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.SmtpServer;
using log4net;

namespace Granikos.SMTPSimulator.Service
{
    internal class SMTPService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SMTPService));
        private readonly CompositionContainer _container;

        private Thread _listenThread;
        private TcpListener _tcpListener;
        private bool _running;

        public SMTPService(IReceiveConnector connector, SMTPServer smtpServer, CompositionContainer container)
        {
            _container = container;
            LocalEndpoint = new IPEndPoint(connector.Address, connector.Port);
            SMTPServer = smtpServer;

            Connector = connector;
            Settings = new DefaultReceiveSettings(connector);

            _greylistingManager = new GreylistingManager(connector.GreylistingTime ?? TimeSpan.Zero);
            SMTPServer.OnConnect += SMTPServerOnOnConnect;

            _container.SatisfyImportsOnce(this);
        }

        private void SMTPServerOnOnConnect(SMTPTransaction transaction, SMTPServer.ConnectEventArgs connect)
        {
            if (Connector.RemoteIPRanges.Any() && !Connector.RemoteIPRanges.Any(range => range.Contains(connect.IP)))
            {
                connect.Cancel = true;
            }

            if (_greylistingManager.IsGreylisted(connect.IP))
            {
                connect.Cancel = true;
                connect.ResponseCode = SMTPStatusCode.NotAvailiable;
            }
        }

        public SMTPServer SMTPServer { get; private set; }
        public IReceiveSettings Settings { get; private set; }
        public IReceiveConnector Connector { get; private set; }
        public IPEndPoint LocalEndpoint { get; private set; }

        private readonly GreylistingManager _greylistingManager;

        public void Start()
        {
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
            }

            _running = true;

            _tcpListener = new TcpListener(LocalEndpoint);

            if (_listenThread == null)
            {
                _listenThread = new Thread(ListenForClients);
                _listenThread.Start();
            }
        }

        public void Stop()
        {
            _running = false;
            _listenThread = null;

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

                while (_running)
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
                var handler = new ClientHandler((TcpClient)obj, this, _container);
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