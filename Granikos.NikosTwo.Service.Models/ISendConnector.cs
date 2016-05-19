using System;
using System.Net;
using Granikos.NikosTwo.Core;

namespace Granikos.NikosTwo.Service.Models
{
    public interface ISendConnector : IEntity<int>, ISendSettings
    {
        string Name { get; set; }
        IPAddress LocalAddress { get; set; }
        IPAddress RemoteAddress { get; set; }
        int RemotePort { get; set; }
        bool UseSmarthost { get; set; }
        TLSSettings TLSSettings { get; set; }
        TimeSpan RetryTime { get; set; }
        int RetryCount { get; set; }
        string[] Domains { get; set; }
        new bool UseAuth { get; set; }
        new string Username { get; set; }
        new string Password { get; set; }
    }
}