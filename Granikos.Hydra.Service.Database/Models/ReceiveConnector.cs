using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Database.Models
{
    public class ReceiveConnector : IReceiveConnector
    {
        private DbIPRange[] _remoteIPRanges;
        private TLSSettings _tlsSettings;

        public ReceiveConnector()
        {
            Address = IPAddress.Any;
            Port = 25;
            _tlsSettings = new TLSSettings();
        }

        public bool Enabled { get; set; }

        [Required]
        public string Name { get; set; }

        [NotMapped]
        public IPAddress Address { get; set; }

        [Required]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
        public string AddressString
        {
            get { return Address.ToString(); }
            set { Address = IPAddress.Parse(value); }
        }

        [Required]
        [Range(0, 65535)]
        public int Port { get; set; }

        [NotMapped]
        IIpRange[] IReceiveConnector.RemoteIPRanges
        {
            get { return RemoteIPRanges.Select(r => (IIpRange)r).ToArray(); }
            set { RemoteIPRanges = value.Select(DbIPRange.FromOther).ToArray(); }
        }

        public DbIPRange[] RemoteIPRanges
        {
            get { return _remoteIPRanges ?? new DbIPRange[0]; }
            set { _remoteIPRanges = value; }
        }

        [Required]
        public string Banner { get; set; }

        public bool RequireAuth { get; set; }

        public string AuthUsername { get; set; }

        public string AuthPassword { get; set; }

        [Required]
        public TLSSettings TLSSettings
        {
            get { return _tlsSettings; }
            set { _tlsSettings = value ?? new TLSSettings(); }
        }

        [Range(0, Int32.MaxValue)]
        public int? GreylistingTimeInternal {get; set; }

        [NotMapped]
        // TODO: [Range(TimeSpan.Zero, TimeSpan.MaxValue)]
        public TimeSpan? GreylistingTime
        {
            get { return GreylistingTimeInternal != null? TimeSpan.FromSeconds(GreylistingTimeInternal.Value) : (TimeSpan?) null; }
            set { GreylistingTimeInternal = value != null? (int)value.Value.TotalSeconds : (int?) null; }
        }

        [Required]
        [Range(0, int.MaxValue)]
        public int Id { get; set; }

        public static ReceiveConnector FromOther(IReceiveConnector source)
        {
            var target = new ReceiveConnector();

            source.CopyTo(target);

            return target;
        }
    }
}