using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(IRecieveConnectorProvider))]
    public class RecieveConnectorProvider : InMemoryProvider<RecieveConnector>, IRecieveConnectorProvider
    {
        public RecieveConnectorProvider()
        {
            Add(new RecieveConnector
                {
                    Address = IPAddress.Parse("127.0.0.1"),
                    Port = 25,
                    EnableSsl = false
                });

            Add(new RecieveConnector
                {
                    Address = IPAddress.Parse("127.0.0.1"),
                    Port = 465,
                    EnableSsl = true
                });
        }
    }
}