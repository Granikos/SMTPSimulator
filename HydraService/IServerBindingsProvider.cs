using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;

namespace HydraService
{
    public interface IServerBindingsProvider : IDataProvider<ServerBindingConfiguration>
    {
    }

    [Export(typeof(IServerBindingsProvider))]
    public class ServerBindingsProvider : IServerBindingsProvider
    {
        private static int _id = 2;

        public IList<ServerBindingConfiguration> All()
        {
            return new List<ServerBindingConfiguration>
            {
                new ServerBindingConfiguration
                {
                    Id = 0,
                    Address = IPAddress.Parse("127.0.0.1"),
                    Port = 25,
                    EnableSsl = false,
                    EnforceTLS = true
                },
                new ServerBindingConfiguration
                {
                    Id = 1,
                    Address = IPAddress.Parse("127.0.0.1"),
                    Port = 465,
                    EnableSsl = true,
                    EnforceTLS = true
                }
            };
        }

        public ServerBindingConfiguration Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public ServerBindingConfiguration Add(ServerBindingConfiguration binding)
        {
            binding.Id = _id++;

            return binding;
        }

        public ServerBindingConfiguration Update(ServerBindingConfiguration binding)
        {
            return binding;
        }

        public bool Delete(int id)
        {
            return true;
        }
    }
}