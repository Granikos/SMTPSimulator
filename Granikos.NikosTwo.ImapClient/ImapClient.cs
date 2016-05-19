using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.Core.Logging;

namespace Granikos.NikosTwo.ImapClient
{
    public class ImapClient
    {
        private readonly ImapStream _stream;
        private bool _authenticated;
        private string[] _authMethods;
        private string _clientName;
        private bool _tlsEnabled;

        protected ImapClient(ISendSettings settings, string host, int port = 143)
        {
            Contract.Requires<ArgumentNullException>(settings != null, "settings");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(host), "host");
            Contract.Requires<ArgumentOutOfRangeException>(port >= 0, "port");

            Settings = settings;
            _stream = new ImapStream(host, port)
            {
                SslProtocols = settings.SslProtocols,
                TLSEncryptionPolicy = settings.TLSEncryptionPolicy,
                ValidateCertificateRevocation = settings.ValidateCertificateRevocation
            };

            _stream.OnError += exception =>
            {
                LastException = exception;
            };
        }

        public SMTPStatusCode? LastStatus { get; private set; }
        public Exception LastException { get; private set; }
        public ISendSettings Settings { get; private set; }

        public string ClientName
        {
            get { return _clientName ?? Dns.GetHostByName("LocalHost").HostName; }
            set { _clientName = value; }
        }

        public X509Certificate2Collection Certificates { get; set; }
        public bool Connected { get; private set; }

        public static ImapClient Create(ISendSettings settings, string host,
            int port = 143)
        {
            Contract.Requires<ArgumentNullException>(settings != null, "settings");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(host), "host");
            Contract.Requires<ArgumentOutOfRangeException>(port >= 0, "port");

            var client = new ImapClient(settings, host, port);

            return client;
        }

        public bool Connect()
        {
            if (!Connected)
            {
                Connected = DoConnect();

                if (!Connected) _stream.Close();
            }

            return Connected;
        }

        private bool DoConnect()
        {
            if (!DoConnectionSequence())
            {
                return false;
            }

            // TODO: Capabilities

            if (!_tlsEnabled && Settings.RequireTLS)
            {
                _stream.Log(LogEventType.Connect, "TLS is required, but the server does not support it.");
                return false;
            }

            if (!Settings.EnableTLS || !_tlsEnabled) return true;

            // TODO: Try STARTTLS

            return true;
        }

        private bool DoConnectionSequence()
        {
            if (!_stream.CreateConnection()) return false;

            if (Settings.TLSFullTunnel)
            {
                if (!_stream.CreateTlsLayer()) return false;
            }

            _stream.ReadResponse();

            return true;
        }

        public ImapStream Stream { get {  return _stream; } }
    }
}