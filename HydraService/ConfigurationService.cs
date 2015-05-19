using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Net;
using System.ServiceModel;
using HydraCore;

namespace HydraService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ConfigurationService : IConfigurationService
    {
        private static int _userId = 2;
        private static int _subnetId = 1;

        [Import(typeof (IServerBindingsProvider))]
        private IServerBindingsProvider _serverBindings;

        public ConfigurationService(SMTPCore core)
        {
            Contract.Requires<ArgumentNullException>(core != null);
            Core = core;
        }

        public SMTPCore Core { get; private set; }

        public void SetProperty(string name, string value)
        {
            throw new NotImplementedException();
        }

        public IList<ServerBindingConfiguration> GetServerBindings()
        {
            return _serverBindings.All();
        }

        public ServerBindingConfiguration GetServerBinding(int id)
        {
            return _serverBindings.Get(id);
        }

        public ServerBindingConfiguration AddServerBinding(ServerBindingConfiguration binding)
        {
            return _serverBindings.Add(binding);
        }

        public ServerBindingConfiguration UpdateServerBinding(ServerBindingConfiguration binding)
        {
            return _serverBindings.Update(binding);
        }

        public bool DeleteServerBinding(int id)
        {
            return _serverBindings.Delete(id);
        }

        public IList<ServerSubnetConfiguration> GetSubnets()
        {
            return new List<ServerSubnetConfiguration>
            {
                new ServerSubnetConfiguration
                {
                    Id = 0,
                    Address = IPAddress.Parse("127.0.0.1"),
                    Size = 24
                }
            };
        }

        public ServerSubnetConfiguration GetSubnet(int id)
        {
            throw new NotImplementedException();
        }

        public ServerSubnetConfiguration AddSubnet(ServerSubnetConfiguration subnet)
        {
            subnet.Id = _subnetId++;
            return subnet;
        }

        public ServerSubnetConfiguration UpdateSubnet(ServerSubnetConfiguration subnet)
        {
            return subnet;
        }

        public bool DeleteSubnet(int id)
        {
            return true;
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

        public LocalUser GetLocalUser(int id)
        {
            return null;
        }

        public LocalUser AddLocalUser(LocalUser user)
        {
            user.Id = ++_userId;

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

        public ServerConfig GetServerConfig()
        {
            return Core.Config;
        }

        public bool SetServerConfig(ServerConfig config)
        {
            Core.Config = config;

            return true;
        }
    }
}