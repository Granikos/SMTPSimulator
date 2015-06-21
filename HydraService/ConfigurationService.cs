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

        [Import]
        private ILocalUserProvider _localUsers;

        [Import]
        private IRecieveConnectorProvider _recieveConnectors;

        [Import]
        private ISendConnectorProvider _sendConnectors;

        [Import]
        private IDomainProvider _domains;

        public IEnumerable<string> GetDomains()
        {
            return _domains.GetDomains();
        }

        public bool DomainExists(string domain)
        {
            return _domains.Exists(domain);
        }

        public bool AddDomain(string domain)
        {
            return _domains.Add(domain);
        }

        public bool DeleteDomain(string domain)
        {
            return _domains.Delete(domain);
        }

        public ConfigurationService(SMTPCore core, ISMTPServerContainer servers)
        {
            Contract.Requires<ArgumentNullException>(core != null, "core");
            Contract.Requires<ArgumentNullException>(servers != null, "servers");

            _servers = servers;
            Core = core;
        }

        public SMTPCore Core { get; private set; }

        public IEnumerable<SendConnector> GetSendConnectors()
        {
            return _sendConnectors.All();
        }

        public SendConnector GetSendConnector(int id)
        {
            return _sendConnectors.Get(id);
        }

        public SendConnector AddSendConnector(SendConnector binding)
        {
            return _sendConnectors.Add(binding);
        }

        public SendConnector UpdateSendConnector(SendConnector binding)
        {
            return _sendConnectors.Update(binding);
        }

        public bool DeleteSendConnector(int id)
        {
            return _sendConnectors.Delete(id);
        }

        public void SetProperty(string name, string value)
        {
            throw new NotImplementedException();
        }

        public SendConnector GetDefaultSendConnector()
        {
            return new SendConnector();
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

            return Directory.GetFiles(folder, "*.pfx").Select(Path.GetFileName).ToArray();
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