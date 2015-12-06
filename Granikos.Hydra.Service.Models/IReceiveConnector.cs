using System;
using System.Net;
using Granikos.Hydra.Core;

namespace Granikos.Hydra.Service.Models
{
    public interface IReceiveConnector : IEntity<int>
    {
        bool Enabled { get; set; }
        string Name { get; set; }
        IPAddress Address { get; set; }
        int Port { get; set; }
        IIpRange[] RemoteIPRanges { get; set; }
        string Banner { get; set; }
        bool RequireAuth { get; set; }
        string AuthUsername { get; set; }
        string AuthPassword { get; set; }
        TLSSettings TLSSettings { get; set; }

        // TODO: [Range(TimeSpan.Zero, TimeSpan.MaxValue)]
        TimeSpan? GreylistingTime { get; set; }
    }
}