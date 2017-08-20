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
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.Core.Logging;

namespace Granikos.SMTPSimulator.ImapClient
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
            if (settings == null) throw new ArgumentNullException("settings");
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException("host");
            if (!(port >= 0)) throw new ArgumentOutOfRangeException("port");

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
            get { return _clientName ?? Dns.GetHostEntry("LocalHost").HostName; }
            set { _clientName = value; }
        }

        public X509Certificate2Collection Certificates { get; set; }
        public bool Connected { get; private set; }

        public static ImapClient Create(ISendSettings settings, string host,
            int port = 143)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException("host");
            if (!(port >= 0)) throw new ArgumentOutOfRangeException("port");

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