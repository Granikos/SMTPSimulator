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

        [Import(typeof (IRecieveConnectorProvider))]
        private IRecieveConnectorProvider _recieveConnectors;

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

        public IEnumerable<RecieveConnector> GetServerBindings()
        {
            return _recieveConnectors.All();
        }

        public RecieveConnector GetServerBinding(int id)
        {
            return _recieveConnectors.Get(id);
        }

        public RecieveConnector AddServerBinding(RecieveConnector binding)
        {
            return _recieveConnectors.Add(binding);
        }

        public RecieveConnector UpdateServerBinding(RecieveConnector binding)
        {
            return _recieveConnectors.Update(binding);
        }

        public bool DeleteServerBinding(int id)
        {
            return _recieveConnectors.Delete(id);
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
    }
}