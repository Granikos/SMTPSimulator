using HydraService.Models;
using System.ComponentModel.Composition;
using System.Net;

namespace HydraService.Providers
{
    [Export(typeof(IServerSubnetProvider))]
    public class ServerSubnetProvider : InMemoryProvider<ServerSubnetConfiguration>, IServerSubnetProvider
    {
        public ServerSubnetProvider()
        {
            Add(
                new ServerSubnetConfiguration
                {
                    Address = IPAddress.Parse("127.0.0.1"),
                    Size = 24
                });
        }
    }
}