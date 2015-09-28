using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Security;
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
                    EncryptionPolicy = EncryptionPolicy.NoEncryption
                },
                GreylistingTime = TimeSpan.FromMinutes(15)
            };
        }
    }
}