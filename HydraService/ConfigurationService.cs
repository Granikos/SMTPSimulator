using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Net;
using System.ServiceModel;
using HydraCore;
using HydraService.Models;
using HydraService.Providers;

namespace HydraService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ConfigurationService : IConfigurationService
    {
        private static int _userId = 2;
        private static int _subnetId = 1;

        [Import(typeof (IServerBindingsProvider))]
        private IServerBindingsProvider _serverBindings;

        [Import(typeof(IServerSubnetProvider))]
        private IServerSubnetProvider _serverSubnets;

        [Import(typeof(ILocalUserProvider))]
        private ILocalUserProvider _localUsers;

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

        public IEnumerable<ServerBindingConfiguration> GetServerBindings()
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

        public IEnumerable<ServerSubnetConfiguration> GetSubnets()
        {
            return _serverSubnets.All();
        }

        public ServerSubnetConfiguration GetSubnet(int id)
        {
            return _serverSubnets.Get(id);
        }

        public ServerSubnetConfiguration AddSubnet(ServerSubnetConfiguration binding)
        {
            return _serverSubnets.Add(binding);
        }

        public ServerSubnetConfiguration UpdateSubnet(ServerSubnetConfiguration binding)
        {
            return _serverSubnets.Update(binding);
        }

        public bool DeleteSubnet(int id)
        {
            return _serverSubnets.Delete(id);
        }

        public IEnumerable<LocalUser> GetLocalUsers()
        {
            return _localUsers.All();
        }

        public LocalUser GetLocalUser(int id)
        {
            return _localUsers.Get(id);
        }

        public LocalUser AddLocalUser(LocalUser binding)
        {
            return _localUsers.Add(binding);
        }

        public LocalUser UpdateLocalUser(LocalUser binding)
        {
            return _localUsers.Update(binding);
        }

        public bool DeleteLocalUser(int id)
        {
            return _localUsers.Delete(id);
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