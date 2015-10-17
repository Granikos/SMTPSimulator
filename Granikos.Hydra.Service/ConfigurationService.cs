using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Providers;
using Granikos.Hydra.SmtpServer;

namespace Granikos.Hydra.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ConfigurationService : IConfigurationService
    {
        private readonly IMailQueueProvider _mailQueue;
        private readonly ISMTPServerContainer _servers;

        [Import]
        private ILocalMailboxGroupProvider _localGroups;

        [Import]
        private IExternalMailboxGroupProvider _externalGroups;

        [Import]
        private IExternalUserProvider _externalUsers;

        [Import]
        private ILocalUserProvider _localUsers;

        [Import]
        private ILogProvider _logs;

        [Import]
        private IReceiveConnectorProvider _receiveConnectors;

        [Import]
        private ISendConnectorProvider _sendConnectors;

        [Import]
        private ITimeTableProvider _timeTables;

        public ConfigurationService(SMTPServer server, ISMTPServerContainer servers, IMailQueueProvider mailQueue)
        {
            Contract.Requires<ArgumentNullException>(server != null, "server");
            Contract.Requires<ArgumentNullException>(servers != null, "servers");
            Contract.Requires<ArgumentNullException>(mailQueue != null, "mailQueue");

            _servers = servers;
            _mailQueue = mailQueue;
            SmtpServer = server;
        }

        public SMTPServer SmtpServer { get; private set; }

        public IEnumerable<MailboxGroup> GetLocalGroups()
        {
            return _localGroups.All();
        }

        public MailboxGroup GetLocalGroup(int id)
        {
            return _localGroups.Get(id);
        }

        public MailboxGroup UpdateLocalGroup(MailboxGroup mailboxGroup)
        {
            return _localGroups.Update(mailboxGroup);
        }

        public MailboxGroup AddLocalGroup(string name)
        {
            return _localGroups.Add(new MailboxGroup(name));
        }

        public bool DeleteLocalGroup(int id)
        {
            return _localGroups.Delete(id);
        }

        public IEnumerable<MailboxGroup> GetExternalGroups()
        {
            return _externalGroups.All();
        }

        public MailboxGroup GetExternalGroup(int id)
        {
            return _externalGroups.Get(id);
        }

        public MailboxGroup UpdateExternalGroup(MailboxGroup mailboxGroup)
        {
            return _externalGroups.Update(mailboxGroup);
        }

        public MailboxGroup AddExternalGroup(string name)
        {
            return _externalGroups.Add(new MailboxGroup(name));
        }

        public bool DeleteExternalGroup(int id)
        {
            return _externalGroups.Delete(id);
        }

        public IEnumerable<SendConnector> GetSendConnectors()
        {
            return _sendConnectors.All();
        }

        public SendConnector GetSendConnector(int id)
        {
            return _sendConnectors.Get(id);
        }

        public SendConnector AddSendConnector(SendConnector connector)
        {
            return _sendConnectors.Add(connector);
        }

        public SendConnector UpdateSendConnector(SendConnector connector)
        {
            return _sendConnectors.Update(connector);
        }

        public bool DeleteSendConnector(int id)
        {
            return _sendConnectors.Delete(id);
        }

        public IEnumerable<User> GetLocalUsersByDomain(string domain)
        {
            return _localUsers.GetByDomain(domain);
        }

        public IEnumerable<ValueWithCount<string>> SearchExternalUserDomains(string domain)
        {
            return _externalUsers.SearchDomains(domain);
        }

        public IEnumerable<User> GetExternalUsersByDomain(string domain)
        {
            return _externalUsers.GetByDomain(domain);
        }

        public int GetLocalUserCount()
        {
            return _localUsers.Total;
        }

        public TimeTable GetEmptyTimeTable()
        {
            return new TimeTable();
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

        public VersionInfo GetVersionInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var date = assembly.GetBuildDate();

            return new VersionInfo
            {
                BuildDate = date,
                Version = version
            };
        }

        public string[] GetLogNames()
        {
            return _logs.FileNames;
        }

        public Stream GetLogFile(string name)
        {
            var stream = new MemoryStream();
            _logs.GetFile(stream, name);

            stream.Position = 0;

            return stream;
        }

        public void SetProperty(string name, string value)
        {
            throw new NotImplementedException();
        }

        public SendConnector GetEmptySendConnector()
        {
            return new SendConnector();
        }

        public ReceiveConnector GetDefaultReceiveConnector()
        {
            return new ReceiveConnector();
        }

        public IEnumerable<ReceiveConnector> GetReceiveConnectors()
        {
            return _receiveConnectors.All();
        }

        public ReceiveConnector GetReceiveConnector(int id)
        {
            return _receiveConnectors.Get(id);
        }

        public ReceiveConnector AddReceiveConnector(ReceiveConnector connector)
        {
            return _receiveConnectors.Add(connector);
        }

        public ReceiveConnector UpdateReceiveConnector(ReceiveConnector connector)
        {
            return _receiveConnectors.Update(connector);
        }

        public bool DeleteReceiveConnector(int id)
        {
            return _receiveConnectors.Delete(id);
        }

        public EntitiesWithTotal<User> GetLocalUsers(int page, int perPage)
        {
            var users = _localUsers.Paged(page, perPage);
            var total = _localUsers.Total;
            var result = new EntitiesWithTotal<User>(users, total);
            return result;
        }

        public IEnumerable<string> SearchLocalUsers(string search)
        {
            return _localUsers.SearchMailboxes(search, 20);
        }

        public User GetLocalUser(int id)
        {
            return _localUsers.Get(id);
        }

        public User AddLocalUser(User connector)
        {
            return _localUsers.Add(connector);
        }

        public User UpdateLocalUser(User connector)
        {
            return _localUsers.Update(connector);
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

        public ImportResult ImportLocalUsers(Stream stream)
        {
            var count = _localUsers.ImportFromCSV(stream, false);

            return new ImportResult(count, 0);
        }

        public ImportResult ImportLocalUsersWithOverwrite(Stream stream)
        {
            var before = _localUsers.Total;
            var count = _localUsers.ImportFromCSV(stream, true);

            return new ImportResult(count, before);
        }

        public bool GenerateLocalUsers(string template, string pattern, string domain, int count)
        {
            return _localUsers.Generate(template, pattern, domain, count);
        }

        public IEnumerable<UserTemplate> GetLocalUserTemplates()
        {
            return _localUsers.GetTemplates();
        }

        public EntitiesWithTotal<User> GetExternalUsers(int page, int perPage)
        {
            return new EntitiesWithTotal<User>(_externalUsers.Paged(page, perPage), _externalUsers.Total);
        }

        public int GetExternalUserCount()
        {
            return _externalUsers.Total;
        }

        public IEnumerable<string> SearchExternalUsers(string search)
        {
            return _externalUsers.SearchMailboxes(search, 20);
        }

        public User GetExternalUser(int id)
        {
            return _externalUsers.Get(id);
        }

        public User AddExternalUser(User user)
        {
            return _externalUsers.Add(user);
        }

        public User UpdateExternalUser(User user)
        {
            return _externalUsers.Update(user);
        }

        public bool DeleteExternalUser(int id)
        {
            return _externalUsers.Delete(id);
        }

        public IEnumerable<TimeTable> GetTimeTables()
        {
            return _timeTables.All();
        }

        public IEnumerable<TimeTableTypeInfo> GetTimeTableTypes()
        {
            return _timeTables.GetTimeTableTypes();
        }

        public TimeTable GetTimeTable(int id)
        {
            return _timeTables.Get(id);
        }

        public TimeTable AddTimeTable(TimeTable timeTable)
        {
            return _timeTables.Add(timeTable);
        }

        public TimeTable UpdateTimeTable(TimeTable timeTable)
        {
            return _timeTables.Update(timeTable);
        }

        public bool DeleteTimeTable(int id)
        {
            return _timeTables.Delete(id);
        }

        public Stream ExportExternalUsers()
        {
            var stream = new MemoryStream();
            _externalUsers.ExportAsCSV(stream);

            stream.Position = 0;

            return stream;
        }

        public ImportResult ImportExternalUsers(Stream stream)
        {
            var count = _externalUsers.ImportFromCSV(stream, false);

            return new ImportResult(count, 0);
        }

        public ImportResult ImportExternalUsersWithOverwrite(Stream stream)
        {
            var before = _externalUsers.Total;
            var count = _externalUsers.ImportFromCSV(stream, true);

            return new ImportResult(count, before);
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

        public void SendMail(MailMessage msg)
        {
            _mailQueue.Enqueue(msg);
        }
    }
}