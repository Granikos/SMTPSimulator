using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Authentication;
using Granikos.Hydra.SmtpClient;

namespace Granikos.Hydra.Service.Models
{
    [DataContract]
    public class SendConnector : IEntity<int>, ISendSettings
    {
        private TLSSettings _tlsSettings;

        public SendConnector(IPAddress remoteIP = null)
        {
            RemoteAddress = remoteIP;
            UseSmarthost = RemoteAddress != null;
            LocalAddress = IPAddress.Any;
            RemotePort = 25;
            TLSSettings = new TLSSettings();
        }

        [Required]
        [DataMember]
        public string Name { get; set; }

        public IPAddress LocalAddress { get; set; }

        [Required]
        [DataMember]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
        public string LocalAddressString
        {
            get { return LocalAddress.ToString(); }
            set { LocalAddress = IPAddress.Parse(value); }
        }

        public IPAddress RemoteAddress { get; set; }

        [DataMember]
        [RegularExpression(
            @"^((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))?$")]
        public string RemoteAddressString
        {
            get { return RemoteAddress != null ? RemoteAddress.ToString() : null; }
            set { RemoteAddress = value != null ? IPAddress.Parse(value) : null; }
        }

        [Required]
        [DataMember]
        [Range(0, 65535)]
        public int RemotePort { get; set; }

        [DataMember]
        public bool UseSmarthost { get; set; }

        [Required]
        [DataMember]
        public TLSSettings TLSSettings
        {
            get { return _tlsSettings; }
            set { _tlsSettings = value ?? new TLSSettings(); }
        }

        [DataMember]
        public TimeSpan RetryTime { get; set; }

        [DataMember]
        [Range(0, 10)]
        public int RetryCount { get; set; }

        [Required]
        [DataMember]
        [Range(0, int.MaxValue)]
        public int Id { get; set; }

        [DataMember]
        public bool UseAuth { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        public bool RequireTLS
        {
            get { return TLSSettings.Mode == TLSMode.Required; }
        }

        public bool EnableTLS
        {
            get { return TLSSettings.Mode != TLSMode.Disabled && TLSSettings.Mode != TLSMode.FullTunnel; }
        }

        public bool TLSFullTunnel
        {
            get { return TLSSettings.Mode == TLSMode.FullTunnel; }
        }

        public ICredentials Credentials
        {
            get { return Username != null ? new NetworkCredential(Username, Password) : null; }
        }

        public EncryptionPolicy TLSEncryptionPolicy
        {
            get { return TLSSettings.EncryptionPolicy; }
        }

        public SslProtocols SslProtocols
        {
            get { return TLSSettings.SslProtocols; }
        }

        public bool ValidateCertificateRevocation
        {
            get { return TLSSettings.ValidateCertificateRevocation; }
        }
    }
}