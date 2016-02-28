using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class ReceiveConnector : IReceiveConnector
    {
        private JsonIPRange[] _remoteIPRanges;
        private TLSSettings _tlsSettings;

        public ReceiveConnector()
        {
            Address = IPAddress.Any;
            Port = 25;
            _tlsSettings = new TLSSettings();
        }

        [DataMember]
        public bool Enabled { get; set; }

        [Required]
        [DataMember]
        public string Name { get; set; }

        public IPAddress Address { get; set; }

        [Required]
        [DataMember]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
        public string AddressString
        {
            get { return Address.ToString(); }
            set { Address = IPAddress.Parse(value); }
        }

        [Required]
        [DataMember]
        [Range(0, 65535)]
        public int Port { get; set; }


        IIpRange[] IReceiveConnector.RemoteIPRanges
        {
            get { return RemoteIPRanges.Select(r => (IIpRange)r).ToArray(); }
            set { RemoteIPRanges = value.Select(JsonIPRange.FromOther).ToArray(); }
        }

        [DataMember]
        public JsonIPRange[] RemoteIPRanges
        {
            get { return _remoteIPRanges ?? new JsonIPRange[0]; }
            set { _remoteIPRanges = value; }
        }

        [Required]
        [DataMember]
        public string Banner { get; set; }

        [DataMember]
        public bool RequireAuth { get; set; }

        [DataMember]
        public string AuthUsername { get; set; }

        [DataMember]
        public string AuthPassword { get; set; }

        [DataMember]
        [Required]
        public TLSSettings TLSSettings
        {
            get { return _tlsSettings; }
            set { _tlsSettings = value ?? new TLSSettings(); }
        }

        [DataMember]
        // TODO: [Range(TimeSpan.Zero, TimeSpan.MaxValue)]
        public TimeSpan? GreylistingTime { get; set; }

        [Required]
        [DataMember]
        [Range(0, int.MaxValue)]
        public int Id { get; set; }
    }
}