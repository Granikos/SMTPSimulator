using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace Granikos.Hydra.Service.Models
{
    [DataContract]
    public class TLSSettings
    {
        public TLSSettings()
        {
            Mode = TLSMode.Enabled;
            SslProtocols = SslProtocols.Default;
            EncryptionPolicy = EncryptionPolicy.RequireEncryption;
            AuthLevel = TLSAuthLevel.EncryptionOnly;
        }

        [DataMember]
        public TLSMode Mode { get; set; }

        [DataMember]
        public SslProtocols SslProtocols { get; set; }

        [DataMember]
        public EncryptionPolicy EncryptionPolicy { get; set; }

        [DataMember]
        public TLSAuthLevel AuthLevel { get; set; }

        [DataMember]
        public bool ValidateCertificateRevocation { get; set; }

        [DataMember]
        public string CertificateName { get; set; }

        [DataMember]
        public string CertificatePassword { get; set; }

        [DataMember]
        public string CertificateDomain { get; set; }

        [DataMember]
        public bool IsFilesystemCertificate { get; set; }
    }
}