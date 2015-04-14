using System.Collections.Generic;
using System.Net;
using System.ServiceModel;

namespace HydraService
{
    public class ConfigurationService : IConfigurationService
    {
        public void SetProperty(string name, string value)
        {
            throw new System.NotImplementedException();
        }

        public IList<ServerBindingConfiguration> GetServerBindings()
        {
            return new List<ServerBindingConfiguration>()
            {
                new ServerBindingConfiguration
                {
                    Address = IPAddress.Parse("127.0.0.1"),
                    Port = 25,
                    EnableSsl = false,
                    EnforceTLS = true
                },
                new ServerBindingConfiguration
                {
                    Address = IPAddress.Parse("127.0.0.1"),
                    Port = 465,
                    EnableSsl = true,
                    EnforceTLS = true
                }
            };
        }
    }
}
