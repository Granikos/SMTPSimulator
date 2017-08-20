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
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.Core.Logging;

namespace Granikos.SMTPSimulator.SmtpClient
{
    public class SMTPClient
    {
        private readonly SmtpStream _stream;
        private bool _authenticated;
        private string[] _authMethods;
        private string _clientName;
        private bool _tlsEnabled;

        protected SMTPClient(ISendSettings settings, string host, int port = 25)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException("host");
            if (!(port >= 0)) throw new ArgumentOutOfRangeException("port");

            Settings = settings;
            _stream = new SmtpStream(host, port)
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

        public static SMTPClient Create(CompositionContainer container, ISendSettings settings, string host,
            int port = 25)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (settings == null) throw new ArgumentNullException("settings");
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException("host");
            if (!(port >= 0)) throw new ArgumentOutOfRangeException("port");

            var client = new SMTPClient(settings, host, port);

            client._stream.SetupLogging(container.GetExportedValues<ISMTPLogger>());

            return client;
        }

        private SMTPResponse ReadResponse()
        {
            var response = _stream.ReadResponse();

            if (response != null)
            {
                LastStatus = response.Code;
            }

            return response;
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

            if (!DoandParseEHLO())
            {
                return false;
            }

            if (!_tlsEnabled && Settings.RequireTLS)
            {
                _stream.Log(LogEventType.Connect, "TLS is required, but the server does not support it.");
                return false;
            }

            if (!Settings.EnableTLS || !_tlsEnabled) return true;
            if (!TryStartTLS())
            {
                return false;
            }

            return true;
        }

        private bool TryStartTLS()
        {
            var success = StartTLS();

            if (Settings.RequireTLS && !success)
            {
                _stream.Log(LogEventType.Connect, "TLS is required, but establishing the TLS layer failed.");
                return false;
            }

            if (success || LastStatus != SMTPStatusCode.Ready) return DoandParseEHLO();

            return DoConnectionSequence() && DoandParseEHLO();
        }

        private bool DoandParseEHLO()
        {
            SMTPResponse reponse;
            if (!DoEHLO(out reponse))
            {
                return false;
            }

            _tlsEnabled = reponse.Args.Contains("STARTTLS", StringComparer.InvariantCultureIgnoreCase);
            var authLine =
                reponse.Args.FirstOrDefault(a => a.StartsWith("AUTH ", StringComparison.InvariantCultureIgnoreCase));
            _authMethods = (authLine ?? "").Split(' ').Skip(1).ToArray();
            return true;
        }

        private bool DoConnectionSequence()
        {
            if (!_stream.CreateConnection()) return false;

            if (Settings.TLSFullTunnel)
            {
                if (!_stream.CreateTlsLayer()) return false;
            }


            var reponse = ReadResponse();

            if (reponse.Code != SMTPStatusCode.Ready)
            {
                return false;
            }

            return true;
        }

        private bool DoEHLO(out SMTPResponse reponse)
        {
            _stream.Write("EHLO {0}", ClientName);

            reponse = ReadResponse();

            return reponse.Code == SMTPStatusCode.Okay;
        }

        public bool SendMail(string from, string[] recipients, string body)
        {
            _stream.Write("MAIL FROM:<{0}>", from);

            var reponse = ReadResponse();

            if (reponse.Code == SMTPStatusCode.AuthRequired)
            {
                if (!Auth()) return false;

                _stream.Write("MAIL FROM:<{0}>", from);
                reponse = ReadResponse();
            }

            if (reponse.Code != SMTPStatusCode.Okay)
            {
                return false;
            }

            foreach (var recipient in recipients)
            {
                _stream.Write("RCPT TO:<{0}>", recipient);

                reponse = ReadResponse();

                if (reponse.Code != SMTPStatusCode.Okay)
                {
                    return false;
                }
            }

            _stream.Write("DATA");

            reponse = ReadResponse();

            if (reponse.Code != SMTPStatusCode.StartMailInput)
            {
                return false;
            }

            var dataLines = body
                .Split(new[] {"\r\n"}, StringSplitOptions.None)
                .Select(l => l.StartsWith(".") ? "." + l : l);
            var dataLinesWithEndDot = dataLines.Concat(new[] {"."});
            _stream.WriteDataBlock(dataLinesWithEndDot);

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

            if (Settings.Credentials != null)
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

        private bool StartTLS()
        {
            _stream.Write("STARTTLS");

            var reponse = ReadResponse();

            if (reponse.Code != SMTPStatusCode.Ready)
            {
                return false;
            }

            return _stream.CreateTlsLayer();
        }

        protected static string Base64Encode(string str)
        {
            if (str == null) throw new ArgumentNullException();
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }

        private bool AuthLogin()
        {
            var nc = Settings.Credentials as NetworkCredential;

            if (nc != null)
            {
                var username = Base64Encode(nc.UserName);
                var password = Base64Encode(nc.Password);

                _stream.Write("AUTH LOGIN {0}", username);

                var reponse = ReadResponse();

                if (reponse.Code != SMTPStatusCode.AuthContinue)
                {
                    return false;
                }

                _stream.Write(password);

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
            var nc = Settings.Credentials as NetworkCredential;

            if (nc != null)
            {
                // var auth = Base64Encode(String.Format("{0} {1}", nc.UserName, nc.Password));
                var auth = Base64Encode(string.Format("\0{0}\0{1}", Settings.Username, Settings.Password));

                _stream.Write("AUTH PLAIN {0}", auth);

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

        public void Close()
        {
            if (_stream.CanWrite)
            {
                _stream.Write("QUIT");

                if (_stream.Connected)
                {
                    var reponse = ReadResponse();

                    if (reponse != null && reponse.Code != SMTPStatusCode.Closing)
                    {
                        _stream.Close();
                        throw new Exception("Server did not respond correctly to QUIT command.");
                    }
                }
            }

            _stream.Close();
        }
    }
}