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
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.Core.Logging;

namespace Granikos.NikosTwo.SmtpClient
{
    public class SmtpStream
    {
        public delegate void StreamError(Exception e);

        private TcpClient _client;
        private Logger _log;
        private StreamReader _reader;
        private Stream _stream;
        private StreamWriter _writer;

        public void SetupLogging(IEnumerable<ISMTPLogger> loggers)
        {
            _log = new Logger(loggers);

            if (_client != null && _client.Client != null)
            {
                _log.LocalEndpoint = LocalEndPoint;
                _log.RemoteEndpoint = RemoteEndPoint;
            }
        }

        public void Log(LogEventType type, string data = null)
        {
            if (_log != null) _log.Log(type, data);
        }

        public SmtpStream(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string ClientName
        {
            get { return _log != null? _log.ClientName : null; }
            set { if (_log != null) _log.ClientName = value; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return _client != null && _client.Client != null? (IPEndPoint)_client.Client.LocalEndPoint : null; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return _client != null && _client.Client != null? (IPEndPoint)_client.Client.RemoteEndPoint : null; }
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

        public void Write(string command, params object[] args)
        {
            _writer.WriteLine(command, args);
            _writer.Flush();

            Log(LogEventType.Outgoing, string.Format(command, args));
        }

        private void RefreshWriter()
        {
            if (_writer != null) _writer.Close();
            _writer = new StreamWriter(_stream, Encoding.ASCII, 1000, true) { NewLine = "\r\n" };
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
            _log.EndSession();
        }

        public event StreamError OnError;

        public bool CreateConnection()
        {
            try
            {
                _client = new TcpClient(Host, Port);
                if (_log != null)
                {
                    _log.LocalEndpoint = LocalEndPoint;
                    _log.RemoteEndpoint = RemoteEndPoint;
                }
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

        public void WriteDataBlock(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                _writer.WriteLine(line);
            }
            _writer.Flush();
        }

        protected virtual void TriggerError(Exception e)
        {
            var handler = OnError;
            if (handler != null) handler(e);
        }

        public SMTPResponse ReadResponse()
        {
            string line;
            try
            {
                line = _reader.ReadLine();
            }
            catch (IOException e)
            {
                TriggerError(e);
                return null;
            }

            SMTPStatusCode? code = null;
            var args = new List<string>();

            while (line != null)
            {
                Log(LogEventType.Incoming, line);

                if (line.Length < 4) throw new Exception("Unexpected reply by server");
                var intCode = int.Parse(line.Substring(0, 3));

                if (!Enum.IsDefined(typeof(SMTPStatusCode), intCode))
                {
                    throw new Exception("Unexpected reply by server: Unknown status code");
                }

                var newCode = (SMTPStatusCode)intCode;
                var message = line.Substring(4);

                if (code != null && newCode != code)
                {
                    throw new Exception("Unexpected reply by server: Mixed status codes");
                }

                code = newCode;
                args.Add(message);

                if (line[3] == '-')
                {
                    line = _reader.ReadLine();
                }
                else if (line[3] == ' ')
                {
                    break;
                }
                else
                {
                    throw new Exception("Unexpected reply by server");
                }
            }

            return code == null ? null : new SMTPResponse(code.Value, args.ToArray());
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
    }
}