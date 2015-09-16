using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using HydraCore;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof (IReceiveConnectorProvider))]
    public class ReceiveConnectorProvider : DefaultProvider<ReceiveConnector>, IReceiveConnectorProvider
    {
        public ReceiveConnectorProvider()
            : base("ReceiveConnectors")
        {
        }

        protected override IEnumerable<ReceiveConnector> Initializer()
        {
            yield return new ReceiveConnector
            {
                Name = "Default",
                Enabled = true,
                Address = IPAddress.Parse("0.0.0.0"),
                Port = 25,
                Banner = "nikos two ready DEFAULT",
                TLSSettings = new TLSSettings
                {
                    CertificateName = "cert.pfx",
                    CertificatePassword = "tester",
                    IsFilesystemCertificate = true
                },
                GreylistingTime = TimeSpan.FromSeconds(30),
                RemoteIPRanges = new[]
                {
                    new IPRange(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.255")),
                    new IPRange(IPAddress.Parse("192.168.178.1"), IPAddress.Parse("192.168.178.255"))
                }
            };

            yield return new ReceiveConnector
            {
                Name = "Default SSL",
                Enabled = true,
                Address = IPAddress.Parse("0.0.0.0"),
                Port = 465,
                Banner = "nikos two ready DEFAULT SSL",
                GreylistingTime = TimeSpan.FromSeconds(30),
                TLSSettings = new TLSSettings
                {
                    Mode = TLSMode.FullTunnel,
                    CertificateName = "cert.pfx",
                    CertificatePassword = "tester",
                    IsFilesystemCertificate = true
                }
            };
        }
    }
}