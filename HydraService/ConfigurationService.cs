using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
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
        private readonly ISMTPServerContainer _servers;
        private static int _userId = 2;
        private static int _subnetId = 1;

        [Import(typeof (IRecieveConnectorProvider))]
        private IRecieveConnectorProvider _recieveConnectors;

        [Import(typeof(IServerSubnetProvider))]
        private IServerSubnetProvider _serverSubnets;

        [Import(typeof(ILocalUserProvider))]
        private ILocalUserProvider _localUsers;

        public ConfigurationService(SMTPCore core, ISMTPServerContainer servers)
        {
            _servers = servers;
            Contract.Requires<ArgumentNullException>(core != null);
            Core = core;
        }

        public SMTPCore Core { get; private set; }

        public void SetProperty(string name, string value)
        {
            throw new NotImplementedException();
        }

        public RecieveConnector GetDefaultRecieveConnector()
        {
            return new RecieveConnector();
        }

        public IEnumerable<RecieveConnector> GetRecieveConnectors()
        {
            return _recieveConnectors.All();
        }

        public RecieveConnector GetRecieveConnector(int id)
        {
            return _recieveConnectors.Get(id);
        }

        public RecieveConnector AddRecieveConnector(RecieveConnector binding)
        {
            return _recieveConnectors.Add(binding);
        }

        public RecieveConnector UpdateRecieveConnector(RecieveConnector binding)
        {
            return _recieveConnectors.Update(binding);
        }

        public bool DeleteRecieveConnector(int id)
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

        public string[] GetCertificateFiles()
        {
            var folder = ConfigurationManager.AppSettings["CertificateFolder"];

            return Directory.GetFiles(folder, "*.pfx").Select(System.IO.Path.GetFileName).ToArray();
        }

        public void Start()
        {
            _servers.StartSMTPServers();
        }

        public void Stop()
        {
            _servers.StopSMTPServers();
        }

        public bool IsRunning()
        {
            return _servers.Running;
        }
    }
}