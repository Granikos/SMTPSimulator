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
using System.Net;
using Granikos.SMTPSimulator.Core.Logging;

namespace Granikos.SMTPSimulator.SmtpClient
{
    public class Logger
    {
        private string _clientName;
        private IPEndPoint _localEndpoint;

        [ImportMany]
        private readonly IEnumerable<ISMTPLogger> _loggers;

        private IPEndPoint _remoteEndpoint;
        private string _session;

        public Logger(IEnumerable<ISMTPLogger> loggers)
        {
            _loggers = loggers;
        }

        public IPEndPoint RemoteEndpoint
        {
            get { return _remoteEndpoint; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _remoteEndpoint = value;
            }
        }

        public IPEndPoint LocalEndpoint
        {
            get { return _localEndpoint; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _localEndpoint = value;
            }
        }

        public string ClientName
        {
            get { return _clientName ?? Dns.GetHostName(); }
            set { _clientName = value; }
        }

        public string StartSession()
        {
            if (_session != null) EndSession();

            _session = Guid.NewGuid().ToString("N").Substring(0, 10);

            foreach (var logger in _loggers)
            {
                logger.StartSession(_session);
            }

            return _session;
        }

        public void Log(LogEventType type, string data = null)
        {
            if (_session == null) StartSession();

            foreach (var logger in _loggers)
            {
                logger.Log(ClientName, _session, LocalEndpoint, RemoteEndpoint, LogPartType.Client, type, data);
            }
        }

        public void EndSession()
        {
            if (_session != null)
            {
                foreach (var logger in _loggers)
                {
                    logger.EndSession(_session);
                }
            }
            _session = null;
        }
    }
}