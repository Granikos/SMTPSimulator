using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using HydraCore.Logging;

namespace HydraCore
{
    public class SMTPClient
    {
        private bool _authenticated;
        private string[] _authMethods;
        private TcpClient _client;
        private string _clientName;
        private IPEndPoint _localEndpoint;

        [ImportMany]
        private IEnumerable<ISMTPLogger> _loggers;

        private int _port;
        private StreamReader _reader;
        private IPEndPoint _remoteEndpoint;
        private string _session;
        private Stream _stream;
        private StreamWriter _writer;

        public SMTPStatusCode? LastStatus { get; private set; }

        protected SMTPClient(ISendSettings settings, string host, int port = 25)
        {
            Contract.Requires<ArgumentNullException>(settings != null, "settings");
            Contract.Requires<ArgumentNullException>(host != null, "host");
            Contract.Requires<ArgumentOutOfRangeException>(port >= 0, "port");

            Settings = settings;
            Host = host;
            Port = port;

            EncryptionPolicy = EncryptionPolicy.RequireEncryption;
            SslProtocols = SslProtocols.Default;
            ValidateCertificateRevocation = true;
        }

        public ISendSettings Settings { get; private set; }
        public string Host { get; set; }

        public string ClientName
        {
            get { return _clientName ?? Dns.GetHostName(); }
            set { _clientName = value; }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value >= 0, "value");
                _port = value;
            }
        }

        public bool TLSFullTunnel { get; set; }
        public ICredentials Credentials { get; set; }
        public X509Certificate2Collection Certificates { get; set; }

        public EncryptionPolicy EncryptionPolicy { get; set; }
        public SslProtocols SslProtocols { get; set; }
        public bool ValidateCertificateRevocation { get; set; }

        public static SMTPClient Create(CompositionContainer container, ISendSettings settings, string host,
            int port = 25)
        {
            var client = new SMTPClient(settings, host, port);

            container.SatisfyImportsOnce(client);

            return client;
        }

        private void Log(LogEventType type, string data = null)
        {
            if (_session == null)
            {
                _session = Guid.NewGuid().ToString("N").Substring(0, 10);
                _localEndpoint = (IPEndPoint) _client.Client.LocalEndPoint;
                _remoteEndpoint = (IPEndPoint) _client.Client.RemoteEndPoint;
            }
            foreach (var logger in _loggers)
            {
                logger.Log(ClientName, _session, _localEndpoint, _remoteEndpoint, LogPartType.Client, type, data);
            }
        }

        public bool Connect()
        {
            if (!DoConnectionSequence()) return false;

            SMTPResponse reponse;
            if (!DoEHLO(out reponse)) return false;

            var tlsEnabled = reponse.Args.Contains("STARTTLS", StringComparer.InvariantCultureIgnoreCase);
            var authLine =
                reponse.Args.FirstOrDefault(a => a.StartsWith("AUTH ", StringComparison.InvariantCultureIgnoreCase));
            _authMethods = (authLine ?? "").Split(' ').Skip(1).ToArray();

            if (!tlsEnabled && Settings.RequireTLS)
            {
                Log(LogEventType.Connect, "TLS is required, but the server does not support it.");
                return false;
            }

            if (Settings.EnableTLS && tlsEnabled)
            {
                var success = StartTLS();

                if (Settings.RequireTLS && !success)
                {
                    Log(LogEventType.Connect, "TLS is required, but establishing the TLS layer failed.");
                    return false;
                }

                // Restore Connection
                if (!success && LastStatus == SMTPStatusCode.Ready)
                {
                    if (!DoConnectionSequence()) return false;
                    if (!DoEHLO(out reponse)) return false;

                }
            }

            // Auth();

            return true;
        }

        private bool DoConnectionSequence()
        {
            if (!CreateConnection()) return false;

            if (TLSFullTunnel)
            {
                if (!CreateTlsLayer()) return false;
            }

            RefreshWriter();
            RefreshReader();

            var reponse = ReadResponse();

            if (reponse.Code != SMTPStatusCode.Ready)
            {
                return false;
            }

            return true;
        }

        private bool DoEHLO(out SMTPResponse reponse)
        {
            Write("EHLO {0}", ClientName);

            reponse = ReadResponse();

            if (reponse.Code != SMTPStatusCode.Okay)
            {
                return false;
            }
            return true;
        }

        private bool CreateTlsLayer()
        {
            var sslStream = new SslStream(_stream, false, UserCertificateValidationCallback,
                UserCertificateSelectionCallback, EncryptionPolicy);

            try
            {
                sslStream.AuthenticateAsClient(Host, Certificates, SslProtocols, ValidateCertificateRevocation);
            }
            catch (AuthenticationException e)
            {
                return false;
            }

            _stream = sslStream;
            return true;
        }

        private bool CreateConnection()
        {
            try
            {
                _client = new TcpClient(Host, Port);
                _stream = _client.GetStream();
            }
            catch (InvalidOperationException)
            {
                return false; // TODO: Cleanup
            }
            catch (SocketException)
            {
                return false; // TODO: Cleanup
            }

            Log(LogEventType.Connect);

            return true;
        }

        public bool SendMail(string from, string[] recipients, string body)
        {
            Write("MAIL FROM:<{0}>", from);

            var reponse = ReadResponse();

            if (reponse.Code == SMTPStatusCode.AuthRequired)
            {
                if (!Auth()) return false;

                Write("MAIL FROM:<{0}>", from);
                reponse = ReadResponse();
            }

            if (reponse.Code != SMTPStatusCode.Okay)
            {
                return false;
            }

            foreach (var recipient in recipients)
            {
                Write("RCPT TO:<{0}>", recipient);

                reponse = ReadResponse();

                if (reponse.Code != SMTPStatusCode.Okay)
                {
                    return false;
                }
            }

            Write("DATA");

            reponse = ReadResponse();

            if (reponse.Code != SMTPStatusCode.StartMailInput)
            {
                return false;
            }

            foreach (var line in body.Split(new[] {"\r\n"}, StringSplitOptions.None))
            {
                if (line.StartsWith("."))
                {
                    _writer.WriteLine("." + line);
                }
                else
                {
                    _writer.WriteLine(line);
                }
            }
            _writer.WriteLine(".");
            _writer.Flush();

            reponse = ReadResponse();

            if (reponse.Code != SMTPStatusCode.Okay)
            {
                return false;
            }

            return true;
        }

        private bool Auth()
        {
            if (_authenticated) return true;

            if (Credentials != null)
            {
                if (_authMethods.Contains("PLAIN", StringComparer.InvariantCultureIgnoreCase))
                {
                    if (AuthPlain())
                    {
                        _authenticated = true;
                        return true;
                    }
                }

                if (_authMethods.Contains("LOGIN", StringComparer.InvariantCultureIgnoreCase))
                {
                    if (AuthLogin())
                    {
                        _authenticated = true;
                        return true;
                    }
                }
            }

            return false;
        }

        private void Write(string command, params object[] args)
        {
            _writer.WriteLine(command, args);
            _writer.Flush();

            Log(LogEventType.Outgoing, string.Format(command, args));
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

        private bool StartTLS()
        {
            Write("STARTTLS");

            var reponse = ReadResponse();

            if (reponse.Code != SMTPStatusCode.Ready)
            {
                return false;
            }

            try
            {

                var sslStream = new SslStream(_stream, false, UserCertificateValidationCallback,
                    UserCertificateSelectionCallback, EncryptionPolicy);

                sslStream.AuthenticateAsClient(Host, Certificates, SslProtocols, ValidateCertificateRevocation);

                _stream = sslStream;
            }
            catch (AuthenticationException e)
            {
                return false;
            }

            RefreshWriter();
            RefreshReader();

            return true;
        }

        protected static string Base64Encode(string str)
        {
            Contract.Requires<ArgumentNullException>(str != null);
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }

        private bool AuthLogin()
        {
            var nc = Credentials as NetworkCredential;

            if (nc != null)
            {
                var username = Base64Encode(nc.UserName);
                var password = Base64Encode(nc.Password);

                Write("AUTH LOGIN {0}", username);

                var reponse = ReadResponse();

                if (reponse.Code != SMTPStatusCode.AuthContinue)
                {
                    return false;
                }

                Write(password);

                reponse = ReadResponse();

                if (reponse.Code == SMTPStatusCode.AuthFailed)
                {
                    throw new Exception("Auth failed: " + string.Join("\n", reponse.Args));
                }

                if (reponse.Code == SMTPStatusCode.AuthSuccess)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        private bool AuthPlain()
        {
            var nc = Credentials as NetworkCredential;

            if (nc != null)
            {
                // var auth = Base64Encode(String.Format("{0} {1}", nc.UserName, nc.Password));
                var auth = Base64Encode(string.Format("\0{0}\0{1}", Settings.Username, Settings.Password));

                Write("AUTH PLAIN {0}", auth);

                var reponse = ReadResponse();

                if (reponse.Code == SMTPStatusCode.AuthFailed)
                {
                    throw new Exception("Auth failed: " + string.Join("\n", reponse.Args));
                }

                if (reponse.Code == SMTPStatusCode.AuthSuccess)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        private SMTPResponse ReadResponse()
        {
            string line;
            try
            {
                line = _reader.ReadLine();
            }
            catch (IOException)
            {
                return null;
            }

            SMTPStatusCode? code = null;
            var args = new List<string>();

            while (line != null)
            {
                Log(LogEventType.Incoming, line);

                if (line.Length < 4) throw new Exception("Unexpected reply by server");
                var intCode = int.Parse(line.Substring(0, 3));

                if (!Enum.IsDefined(typeof (SMTPStatusCode), intCode))
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

            LastStatus = code;

            return code == null ? null : new SMTPResponse(code.Value, args.ToArray());
        }

        public void Close()
        {
            if (_stream != null && _stream.CanWrite && _writer != null)
            {
                Write("QUIT");

                if (_client.Connected)
                {
                    var reponse = ReadResponse();

                    if (reponse != null && reponse.Code != SMTPStatusCode.Closing)
                    {
                        throw new Exception("Server did not respond correctly to QUIT command.");
                    }
                }
            }

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