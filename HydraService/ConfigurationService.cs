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

        public IList<LocalUser> GetLocalUsers()
        {
            return new List<LocalUser>
            {
                new LocalUser
                {
                    Id = 1,
                    FirstName = "Bernd",
                    LastName = "Müller",
                    Mailbox = "bernd.mueller@test.de"
                },
                new LocalUser
                {
                    Id = 2,
                    FirstName = "Eva",
                    LastName = "Schmidt",
                    Mailbox = "eva.schmidt@test.de"
                }
            };
        }

        private static int _id = 2;

        public LocalUser GetLocalUser(int id)
        {
            return null;
        }

        public LocalUser AddLocalUser(LocalUser user)
        {
            user.Id = ++_id;

            return user;
        }

        public LocalUser UpdateLocalUser(LocalUser user)
        {
            return user;
        }

        public bool DeleteLocalUser(int id)
        {
            return true;
        }
    }
}
