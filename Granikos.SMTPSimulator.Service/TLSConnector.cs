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
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service
{
    public class TLSConnector
    {
        private readonly Action<string> _logger;
        private readonly ICertificateProvider _certProvider;

        public TLSConnector(TLSSettings settings, Action<string> logger, ICertificateProvider certProvider)
        {
            _logger = logger;
            _certProvider = certProvider;
            if (settings == null) throw new ArgumentNullException();
            Settings = settings;
        }

        public TLSSettings Settings { get; private set; }

        public X509Certificate2 GetCertificate()
        {
            return _certProvider != null? _certProvider.GetCertificate(Settings.CertificateName, Settings.CertificatePassword) : null;
        }

        public X509Certificate2Collection GetCertificateCollection()
        {
            return _certProvider != null? new X509Certificate2Collection(new[] {GetCertificate()}) : new X509Certificate2Collection();
        }

        public SslStream GetSslStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException();
            return new SslStream(stream, false, UserCertificateValidationCallback, UserCertificateSelectionCallback,
                Settings.EncryptionPolicy);
        }

        public async Task AuthenticateAsServerAsync(SslStream sslStream)
        {
            if (sslStream == null) throw new ArgumentNullException();
            await
                sslStream.AuthenticateAsServerAsync(GetCertificate(), Settings.AuthLevel != TLSAuthLevel.EncryptionOnly,
                    Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public void AuthenticateAsServer(SslStream sslStream)
        {
            if (sslStream == null) throw new ArgumentNullException();
            sslStream.AuthenticateAsServer(GetCertificate(), Settings.AuthLevel != TLSAuthLevel.EncryptionOnly,
                Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public async Task AuthenticateAsClientAsync(SslStream sslStream)
        {
            if (sslStream == null) throw new ArgumentNullException();
            await
                sslStream.AuthenticateAsClientAsync(Settings.CertificateDomain, GetCertificateCollection(),
                    Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public void AuthenticateAsClient(SslStream sslStream)
        {
            if (sslStream == null) throw new ArgumentNullException();
            sslStream.AuthenticateAsClient(Settings.CertificateDomain, GetCertificateCollection(), Settings.SslProtocols,
                Settings.ValidateCertificateRevocation);
        }

        private X509Certificate UserCertificateSelectionCallback(object sender, string targetHost,
            X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            // Log("Client is selecting a local certificate.");
            if (acceptableIssuers != null &&
                acceptableIssuers.Length > 0 &&
                localCertificates != null &&
                localCertificates.Count > 1)
            {
                // Use the first certificate that is from an acceptable issuer. 
                foreach (var certificate in localCertificates)
                {
                    if (Array.IndexOf(acceptableIssuers, certificate.Issuer) != -1)
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
            if (sslPolicyErrors == SslPolicyErrors.None || Settings.AuthLevel == TLSAuthLevel.EncryptionOnly)
                return true;

            var acceptCertificate = true;
            var msg = "The client could not be validated for the following reason(s):\r\n";

            //The server did not present a certificate
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) ==
                SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                msg = msg + "\r\n    -The client did not present a certificate.\r\n";
                acceptCertificate = false;
            }
            else
            {
                //The certificate does not match the server name
                if (Settings.AuthLevel == TLSAuthLevel.DomainValidation &&
                    (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) ==
                    SslPolicyErrors.RemoteCertificateNameMismatch)
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
                        if (item.Status == X509ChainStatusFlags.RevocationStatusUnknown ||
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

            Log(msg);

            return acceptCertificate;
        }

        private void Log(string message, params object[] args)
        {
            _logger(string.Format(message, args));
        }

        public void DisplaySecurityLevel(SslStream stream)
        {
            if (stream == null) throw new ArgumentNullException();
            Log("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Log("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Log("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Log("Protocol: {0}", stream.SslProtocol);
        }

        public void DisplaySecurityServices(SslStream stream)
        {
            if (stream == null) throw new ArgumentNullException();
            Log("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Log("IsSigned: {0}", stream.IsSigned);
            Log("Is Encrypted: {0}", stream.IsEncrypted);
        }

        public void DisplayCertificateInformation(SslStream stream)
        {
            if (stream == null) throw new ArgumentNullException();
            Log("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            var localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Log("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Log("Local certificate is null.");
            }

            var remoteCertificate = stream.RemoteCertificate;
            if (remoteCertificate != null)
            {
                Log("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Log("Remote certificate is null.");
            }
        }
    }
}