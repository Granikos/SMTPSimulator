﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
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
        private IDomainProvider _domains;

        [Import]
        private IExternalUserProvider _externalUsers;

        [Import]
        private ILocalUserProvider _localUsers;

        [Import]
        private IRecieveConnectorProvider _recieveConnectors;

        [Import]
        private ISendConnectorProvider _sendConnectors;

        public ConfigurationService(SMTPCore core, ISMTPServerContainer servers)
        {
            Contract.Requires<ArgumentNullException>(core != null, "core");
            Contract.Requires<ArgumentNullException>(servers != null, "servers");

            _servers = servers;
            Core = core;
        }

        public SMTPCore Core { get; private set; }

        public IEnumerable<Domain> GetDomains()
        {
            return _domains.All();
        }

        public Domain GetDomain(string domain)
        {
            return
                _domains.All()
                    .FirstOrDefault(d => d.DomainName.Equals(domain, StringComparison.InvariantCultureIgnoreCase));
        }

        public Domain UpdateDomain(Domain domain)
        {
            return _domains.Update(domain);
        }

        public Domain AddDomain(string domain)
        {
            return _domains.Add(new Domain(domain));
        }

        public bool DeleteDomain(int id)
        {
            return _domains.Delete(id);
        }

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

        public SendConnector GetDefaultSendConnector()
        {
            return _sendConnectors.DefaultConnector;
        }

        public bool SetDefaultSendConnector(int id)
        {
            try
            {
                _sendConnectors.DefaultId = id;
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        public void SetProperty(string name, string value)
        {
            throw new NotImplementedException();
        }

        public SendConnector GetEmptySendConnector()
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

        public Stream ExportLocalUsers()
        {
            var stream = new MemoryStream();
            _localUsers.ExportAsCSV(stream);

            stream.Position = 0;

            return stream;
        }

        public void ImportLocalUsers(Stream stream)
        {
            _localUsers.ImportFromCSV(stream);
        }

        public bool GenerateLocalUsers(string template, string pattern, int count)
        {
            return _localUsers.Generate(template, pattern, count);
        }

        public IEnumerable<ExternalUser> GetExternalUsers()
        {
            return _externalUsers.All();
        }

        public ExternalUser GetExternalUser(int id)
        {
            return _externalUsers.Get(id);
        }

        public ExternalUser AddExternalUser(ExternalUser user)
        {
            return _externalUsers.Add(user);
        }

        public ExternalUser UpdateExternalUser(ExternalUser user)
        {
            return _externalUsers.Update(user);
        }

        public bool DeleteExternalUser(int id)
        {
            return _externalUsers.Delete(id);
        }

        public Stream ExportExternalUsers()
        {
            var stream = new MemoryStream();
            _externalUsers.ExportAsCSV(stream, DomainSource);

            stream.Position = 0;

            return stream;
        }

        public void ImportExternalUsers(Stream stream)
        {
            _externalUsers.ImportFromCSV(stream, DomainSource);
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

        private string DomainSource(int id)
        {
            return _domains.Get(id).DomainName;
        }

        private int DomainSource(string domainName)
        {
            var domain =
                _domains.All()
                    .FirstOrDefault(d => d.DomainName.Equals(domainName, StringComparison.InvariantCultureIgnoreCase));

            if (domain == null)
            {
                domain = AddDomain(domainName);
            }

            return domain.Id;
        }
    }
}