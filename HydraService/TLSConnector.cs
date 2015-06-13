using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HydraService.Models;

namespace HydraService
{
    public class TLSConnector
    {
        private readonly Action<string> _logger;

        public TLSConnector(TLSSettings settings, Action<string> logger)
        {
            _logger = logger;
            Contract.Requires<ArgumentNullException>(settings != null);
            Settings = settings;
        }

        public TLSSettings Settings { get; private set; }

        public X509Certificate2 GetCertificate()
        {
            if (Settings.IsFilesystemCertificate)
            {
                return new X509Certificate2(Settings.CertificateName, Settings.CertificatePassword);
            }

            var store = new X509Store("Personal", StoreLocation.LocalMachine);
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, Settings.CertificateName, true);

            return certs[0];
        }

        public X509Certificate2Collection GetCertificateCollection()
        {
            return new X509Certificate2Collection(new[] { GetCertificate() });
        }

        public SslStream GetSslStream(Stream stream)
        {
            Contract.Requires<ArgumentNullException>(stream != null);
            return new SslStream(stream, false, UserCertificateValidationCallback, UserCertificateSelectionCallback,
                Settings.EncryptionPolicy);
        }

        public async Task AuthenticateAsServerAsync(SslStream sslStream)
        {
            Contract.Requires<ArgumentNullException>(sslStream != null);
            await
                sslStream.AuthenticateAsServerAsync(GetCertificate(), Settings.AuthLevel != TLSAuthLevel.EncryptionOnly,
                    Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public void AuthenticateAsServer(SslStream sslStream)
        {
            Contract.Requires<ArgumentNullException>(sslStream != null);
            sslStream.AuthenticateAsServer(GetCertificate(), Settings.AuthLevel != TLSAuthLevel.EncryptionOnly,
                Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public async Task AuthenticateAsClientAsync(SslStream sslStream)
        {
            Contract.Requires<ArgumentNullException>(sslStream != null);
            await
                sslStream.AuthenticateAsClientAsync(Settings.CertificateDomain, GetCertificateCollection(),
                    Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public void AuthenticateAsClient(SslStream sslStream)
        {
            Contract.Requires<ArgumentNullException>(sslStream != null);
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
                    (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
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
            _logger(String.Format(message, args));
        }

        public void DisplaySecurityLevel(SslStream stream)
        {
            Contract.Requires<ArgumentNullException>(stream != null);
            Log("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Log("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Log("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Log("Protocol: {0}", stream.SslProtocol);
        }

        public void DisplaySecurityServices(SslStream stream)
        {
            Contract.Requires<ArgumentNullException>(stream != null);
            Log("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Log("IsSigned: {0}", stream.IsSigned);
            Log("Is Encrypted: {0}", stream.IsEncrypted);
        }

        public void DisplayCertificateInformation(SslStream stream)
        {
            Contract.Requires<ArgumentNullException>(stream != null);
            Log("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
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

            X509Certificate remoteCertificate = stream.RemoteCertificate;
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