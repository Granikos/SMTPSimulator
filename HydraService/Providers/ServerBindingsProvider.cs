using System.ComponentModel.Composition;
using System.Net;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(IServerBindingsProvider))]
    public class ServerBindingsProvider : InMemoryProvider<ServerBindingConfiguration>, IServerBindingsProvider
    {
        public ServerBindingsProvider()
        {
            Add(new ServerBindingConfiguration
                {
                    Address = IPAddress.Parse("127.0.0.1"),
                    Port = 25,
                    EnableSsl = false,
                    EnforceTLS = true
                });

            Add(new ServerBindingConfiguration
                {
                    Address = IPAddress.Parse("127.0.0.1"),
                    Port = 465,
                    EnableSsl = true,
                    EnforceTLS = true
                });
        }
    }
}