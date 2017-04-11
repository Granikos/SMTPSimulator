using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Granikos.SMTPSimulator.Core.Logging;

namespace Granikos.SMTPSimulator.ImapClient
{
    public class ImapStream
    {
        public delegate void ResponseHander(ImapResponse response);


        public delegate void StreamError(Exception e);

        private readonly Dictionary<int, WriteAction> _actions = new Dictionary<int, WriteAction>();

        private TcpClient _client;
        private StreamReader _reader;
        private Stream _stream;
        private StreamWriter _writer;
        private readonly int curAction = 1;

        public ImapStream(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string ClientName { get; set; }

        public IPEndPoint LocalEndPoint
        {
            get { return _client != null && _client.Client != null ? (IPEndPoint) _client.Client.LocalEndPoint : null; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return _client != null && _client.Client != null ? (IPEndPoint) _client.Client.RemoteEndPoint : null;
            }
        }

        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool ValidateCertificateRevocation { get; set; }
        public SslProtocols SslProtocols { get; set; }
        public X509CertificateCollection Certificates { get; set; }
        public EncryptionPolicy TLSEncryptionPolicy { get; set; }

        public bool CanWrite
        {
            get { return _stream != null && _stream.CanWrite && _writer != null; }
        }

        public bool CanRead
        {
            get { return _stream != null && _stream.CanRead && _reader != null; }
        }

        public bool Connected
        {
            get { return _client.Connected; }
        }

        public void Log(LogEventType type, string data = null)
        {
            Console.WriteLine("{0} {1}", type, data);
        }

        public WriteAction Write(string command, params object[] args)
        {
            var action = new WriteAction();
            _writer.WriteLine(curAction + " " + command, args);
            _writer.Flush();

            _actions.Add(curAction, action);

            Log(LogEventType.Outgoing, string.Format(command, args));

            return action;
        }

        private void RefreshWriter()
        {
            if (_writer != null) _writer.Close();
            _writer = new StreamWriter(_stream, Encoding.ASCII, 1000, true) {NewLine = "\r\n"};
        }

        private void RefreshReader()
        {
            if (_reader != null) _reader.Close();
            _reader = new StreamReader(_stream, Encoding.ASCII, false, 1000, true);
        }

        public void Close()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }

            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }

            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }

            Log(LogEventType.Disconnect);
        }

        public event StreamError OnError;

        public bool CreateConnection()
        {
            try
            {
                _client = new TcpClient(Host, Port);
                _stream = _client.GetStream();
            }
            catch (InvalidOperationException e)
            {
                TriggerError(e);
                return false; // TODO: Cleanup
            }
            catch (SocketException e)
            {
                // https://msdn.microsoft.com/de-de/library/system.net.sockets.socketerror%28v=vs.110%29.aspx
                TriggerError(e);
                return false; // TODO: Cleanup
            }

            Log(LogEventType.Connect);

            RefreshWriter();
            RefreshReader();

            return true;
        }

        public bool CreateTlsLayer()
        {
            try
            {
                var sslStream = new SslStream(_stream, false, UserCertificateValidationCallback,
                    UserCertificateSelectionCallback, TLSEncryptionPolicy);

                sslStream.AuthenticateAsClient(Host, Certificates, SslProtocols, ValidateCertificateRevocation);

                _stream = sslStream;
            }
            catch (AuthenticationException e)
            {
                TriggerError(e);
                return false;
            }

            RefreshWriter();
            RefreshReader();

            return true;
        }

        protected virtual void TriggerError(Exception e)
        {
            var handler = OnError;
            if (handler != null) handler(e);
        }

        public event ResponseHander OnUntaggedReponse;

        public async void ReadResponse()
        {
            string line;
            try
            {
                line = await _reader.ReadLineAsync();
            }
            catch (IOException e)
            {
                TriggerError(e);
                return;
            }

            Log(LogEventType.Incoming, line);

            var parts = line.Split(new[] {' '}, 2);

            if (parts.Length != 2)
            {
                throw new Exception("Unexpected line in IMAP response: " + line);
            }

            int actionId;

            if (parts[0].Equals("*"))
            {
                if (OnUntaggedReponse != null)
                {
                    OnUntaggedReponse(new ImapResponse(parts[1]));
                }

                return;
            }

            if (!int.TryParse(parts[0], out actionId))
            {
                throw new Exception("Unexpected action id in IMAP response: " + line);
            }

            WriteAction action;

            if (!_actions.TryGetValue(actionId, out action))
            {
                throw new Exception("Unknown action id in IMAP response: " + line);
            }

            parts = parts[1].Split(new[] {' '}, 2);
            var text = parts.Length > 1 ? parts[1] : null;

            switch (parts[0].ToUpper())
            {
                case "OK":
                    action.TriggerOkay(text);
                    break;
                case "NO":
                    action.TriggerNo(text);
                    break;
                case "BAD":
                    action.TriggerBad(text);
                    break;
                default:
                    throw new Exception("Syntax error in IMAP response line, expected OK, NO or BAD: " + line);
            }

            ReadResponse();
        }

        private X509Certificate UserCertificateSelectionCallback(object sender, string targetHost,
            X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            Debug.WriteLine("Client is selecting a local certificate.");
            if (acceptableIssuers != null &&
                acceptableIssuers.Length > 0 &&
                localCertificates != null &&
                localCertificates.Count > 0)
            {
                // Use the first certificate that is from an acceptable issuer. 
                foreach (var certificate in localCertificates)
                {
                    var issuer = certificate.Issuer;
                    if (Array.IndexOf(acceptableIssuers, issuer) != -1)
                        return certificate;
                }
            }
            if (localCertificates != null &&
                localCertificates.Count > 0)
                return localCertificates[0];

            return null;
        }

        private bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            //Return true if the server certificate is ok
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            var acceptCertificate = true;
            var msg = "The server could not be validated for the following reason(s):\r\n";

            //The server did not present a certificate
            if ((sslPolicyErrors &
                 SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                msg = msg + "\r\n    -The server did not present a certificate.\r\n";
                acceptCertificate = false;
            }
            else
            {
                //The certificate does not match the server name
                if ((sslPolicyErrors &
                     SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
                {
                    msg = msg + "\r\n    -The certificate name does not match the authenticated name.\r\n";
                    acceptCertificate = false;
                }

                //There is some other problem with the certificate
                if ((sslPolicyErrors &
                     SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    foreach (var item in chain.ChainStatus)
                    {
                        if (item.Status == X509ChainStatusFlags.RevocationStatusUnknown &&
                            item.Status == X509ChainStatusFlags.OfflineRevocation)
                            break;

                        if (item.Status != X509ChainStatusFlags.NoError)
                        {
                            msg = msg + "\r\n    -" + item.StatusInformation;
                            acceptCertificate = false;
                        }
                    }
                }
            }

            Log(LogEventType.Certificate, msg);

            return acceptCertificate;
        }

        public class WriteAction
        {
            private Action<ImapResponse> _onNo;
            private Action<ImapResponse> _onOkay;
            private Action<ImapResponse> _onBad;

            public WriteAction OnOkay(Action<ImapResponse> action)
            {
                _onOkay = action;

                return this;
            }

            public WriteAction OnNo(Action<ImapResponse> action)
            {
                _onNo = action;
                return this;
            }

            public WriteAction OnBad(Action<ImapResponse> action)
            {
                _onBad = action;
                return this;
            }

            internal void TriggerOkay(string text)
            {
                if (_onOkay != null)
                {
                    _onOkay(new ImapResponse(text));
                }
            }

            internal void TriggerNo(string text)
            {
                if (_onNo != null)
                {
                    _onNo(new ImapResponse(text));
                }
            }

            internal void TriggerBad(string text)
            {
                if (_onBad != null)
                {
                    _onBad(new ImapResponse(text));
                }
            }
        }
    }
}