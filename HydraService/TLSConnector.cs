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
        public TLSConnector(TLSSettings settings)
        {
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
            return new X509Certificate2Collection(new[] {GetCertificate()});
        }

        public SslStream GetSslStream(Stream stream)
        {
            return new SslStream(stream, false, UserCertificateValidationCallback, UserCertificateSelectionCallback,
                Settings.EncryptionPolicy);
        }

        public async Task AuthenticateAsServerAsync(SslStream sslStream)
        {
            await
                sslStream.AuthenticateAsServerAsync(GetCertificate(), Settings.AuthLevel != TLSAuthLevel.EncryptionOnly,
                    Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public void AuthenticateAsServer(SslStream sslStream)
        {
            sslStream.AuthenticateAsServer(GetCertificate(), Settings.AuthLevel != TLSAuthLevel.EncryptionOnly,
                Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public async Task AuthenticateAsClientAsync(SslStream sslStream)
        {
            await
                sslStream.AuthenticateAsClientAsync(Settings.CertificateDomain, GetCertificateCollection(),
                    Settings.SslProtocols, Settings.ValidateCertificateRevocation);
        }

        public void AuthenticateAsClient(SslStream sslStream)
        {
            sslStream.AuthenticateAsClient(Settings.CertificateDomain, GetCertificateCollection(), Settings.SslProtocols,
                Settings.ValidateCertificateRevocation);
        }

        public X509Certificate UserCertificateSelectionCallback(object sender, string targetHost,
            X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            Debug.WriteLine("Client is selecting a local certificate.");
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

        public bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
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

            Debug.WriteLine(msg);

            return acceptCertificate;
        }
    }
}