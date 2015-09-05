using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using HydraCore;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof (IRecieveConnectorProvider))]
    public class ReceiveConnectorProvider : DefaultProvider<RecieveConnector>, IRecieveConnectorProvider
    {
        public ReceiveConnectorProvider()
            : base("ReceiveConnectors")
        {
        }

        protected override IEnumerable<RecieveConnector> Initializer()
        {
            yield return new RecieveConnector
            {
                Name = "Default",
                Enabled = true,
                Address = IPAddress.Parse("0.0.0.0"),
                Port = 25,
                Banner = "This is the banner text!",
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

            yield return new RecieveConnector
            {
                Name = "Default SSL",
                Enabled = true,
                Address = IPAddress.Parse("0.0.0.0"),
                Port = 465,
                Banner = "This is the banner text!",
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