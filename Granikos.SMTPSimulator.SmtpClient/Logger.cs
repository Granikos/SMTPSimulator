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