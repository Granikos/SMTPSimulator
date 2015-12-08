using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Authentication;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.ConfigurationService.Models
{
    [DataContract]
    public class SendConnector : ISendConnector
    {
        [Required]
        [DataMember]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
        public string LocalAddressString
        {
            get { return LocalAddress.ToString(); }
            set { LocalAddress = IPAddress.Parse(value); }
        }

        [DataMember]
        [RegularExpression(
            @"^((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))?$")]
        public string RemoteAddressString
        {
            get { return RemoteAddress != null ? RemoteAddress.ToString() : null; }
            set { RemoteAddress = value != null ? IPAddress.Parse(value) : null; }
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public IPAddress LocalAddress { get; set; }
        public IPAddress RemoteAddress { get; set; }

        [Required]
        [DataMember]
        [Range(0, 65535)]
        public int RemotePort { get; set; }

        [DataMember]
        public bool UseSmarthost { get; set; }

        [DataMember]
        public TLSSettings TLSSettings { get; set; }

        [DataMember]
        public TimeSpan RetryTime { get; set; }

        [DataMember]
        public int RetryCount { get; set; }

        [DataMember]
        public string[] Domains { get; set; }

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