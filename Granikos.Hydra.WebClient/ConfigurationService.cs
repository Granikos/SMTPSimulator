using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using Granikos.Hydra.Service.ConfigurationService;
using Granikos.Hydra.Service.ConfigurationService.Models;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.WebClient
{
    public class ConfigurationServiceClient : ClientBase<IConfigurationService>, IConfigurationService
    {
        public ConfigurationServiceClient()
        {
        }

        public ConfigurationServiceClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public ConfigurationServiceClient(
            string endpointConfigurationName,
            string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public ConfigurationServiceClient(string endpointConfigurationName,
            EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public ConfigurationServiceClient(Binding binding,
            EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public bool DeleteMailTemplate(int id)
        {
            return Channel.DeleteMailTemplate(id);
        }

        public MailTemplate UpdateMailTemplate(MailTemplate template)
        {
            return Channel.UpdateMailTemplate(template);
        }

        public MailTemplate GetMailTemplate(int id)
        {
            return Channel.GetMailTemplate(id);
        }

        public Stream ExportMailTemplate(int id)
        {
            return Channel.ExportMailTemplate(id);
        }

        public MailTemplate ImportMailTemplate(Stream stream)
        {
            return Channel.ImportMailTemplate(stream);
        }

        public MailTemplate AddMailTemplate(MailTemplate template)
        {
            return Channel.AddMailTemplate(template);
        }

        public VersionInfo GetVersionInfo()
        {
            return Channel.GetVersionInfo();
        }

        public string[] GetLogNames()
        {
            return Channel.GetLogNames();
        }

        public Stream GetLogFile(string name)
        {
            return Channel.GetLogFile(name);
        }

        public IEnumerable<UserGroup> GetLocalGroups()
        {
            return Channel.GetLocalGroups();
        }

        public UserGroup GetLocalGroup(int id)
        {
            return Channel.GetLocalGroup(id);
        }

        public UserGroup UpdateLocalGroup(UserGroup @group)
        {
            return Channel.UpdateLocalGroup(@group);
        }

        public UserGroup AddLocalGroup(string name)
        {
            return Channel.AddLocalGroup(name);
        }

        public bool DeleteLocalGroup(int id)
        {
            return Channel.DeleteLocalGroup(id);
        }

        public IEnumerable<UserGroup> GetExternalGroups()
        {
            return Channel.GetExternalGroups();
        }

        public UserGroup GetExternalGroup(int id)
        {
            return Channel.GetExternalGroup(id);
        }

        public UserGroup UpdateExternalGroup(UserGroup @group)
        {
            return Channel.UpdateExternalGroup(@group);
        }

        public UserGroup AddExternalGroup(string name)
        {
            return Channel.AddExternalGroup(name);
        }

        public bool DeleteExternalGroup(int id)
        {
            return Channel.DeleteExternalGroup(id);
        }

        public ReceiveConnector GetDefaultReceiveConnector()
        {
            return Channel.GetDefaultReceiveConnector();
        }

        public IEnumerable<ReceiveConnector> GetReceiveConnectors()
        {
            return Channel.GetReceiveConnectors();
        }

        public ReceiveConnector GetReceiveConnector(int id)
        {
            return Channel.GetReceiveConnector(id);
        }

        public ReceiveConnector AddReceiveConnector(ReceiveConnector connector)
        {
            return Channel.AddReceiveConnector(connector);
        }

        public ReceiveConnector UpdateReceiveConnector(ReceiveConnector connector)
        {
            return Channel.UpdateReceiveConnector(connector);
        }

        public bool DeleteReceiveConnector(int id)
        {
            return Channel.DeleteReceiveConnector(id);
        }

        public SendConnector GetEmptySendConnector()
        {
            return Channel.GetEmptySendConnector();
        }

        public TimeTable GetEmptyTimeTable()
        {
            return Channel.GetEmptyTimeTable();
        }

        public SendConnector GetDefaultSendConnector()
        {
            return Channel.GetDefaultSendConnector();
        }

        public bool SetDefaultSendConnector(int id)
        {
            return Channel.SetDefaultSendConnector(id);
        }

        public IEnumerable<SendConnector> GetSendConnectors()
        {
            return Channel.GetSendConnectors();
        }

        public SendConnector GetSendConnector(int id)
        {
            return Channel.GetSendConnector(id);
        }

        public SendConnector AddSendConnector(SendConnector connector)
        {
            return Channel.AddSendConnector(connector);
        }

        public SendConnector UpdateSendConnector(SendConnector connector)
        {
            return Channel.UpdateSendConnector(connector);
        }

        public bool DeleteSendConnector(int id)
        {
            return Channel.DeleteSendConnector(id);
        }

        public IEnumerable<User> GetLocalUsersByDomain(string domain)
        {
            return Channel.GetLocalUsersByDomain(domain);
        }

        public IEnumerable<ValueWithCount<string>> SearchExternalUserDomains(string domain)
        {
            return Channel.SearchExternalUserDomains(domain);
        }

        public IEnumerable<User> GetExternalUsersByDomain(string domain)
        {
            return Channel.GetExternalUsersByDomain(domain);
        }

        public int GetLocalUserCount()
        {
            return Channel.GetLocalUserCount();
        }

        public EntitiesWithTotal<User> GetLocalUsers(int page, int perPage)
        {
            return Channel.GetLocalUsers(page, perPage);
        }

        public IEnumerable<string> SearchLocalUsers(string search)
        {
            return Channel.SearchLocalUsers(search);
        }

        public User GetLocalUser(int id)
        {
            return Channel.GetLocalUser(id);
        }

        public User AddLocalUser(User user)
        {
            return Channel.AddLocalUser(user);
        }

        public User UpdateLocalUser(User user)
        {
            return Channel.UpdateLocalUser(user);
        }

        public bool DeleteLocalUser(int id)
        {
            return Channel.DeleteLocalUser(id);
        }

        public Stream ExportLocalUsers()
        {
            return Channel.ExportLocalUsers();
        }

        public ImportResult ImportLocalUsers(Stream stream)
        {
            return Channel.ImportLocalUsers(stream);
        }

        public ImportResult ImportLocalUsersWithOverwrite(Stream stream)
        {
            return Channel.ImportLocalUsersWithOverwrite(stream);
        }

        public bool GenerateLocalUsers(string template, string pattern, string domain, int count)
        {
            return Channel.GenerateLocalUsers(template, pattern, domain, count);
        }

        public IEnumerable<UserTemplate> GetLocalUserTemplates()
        {
            return Channel.GetLocalUserTemplates();
        }

        public EntitiesWithTotal<User> GetExternalUsers(int page, int perPage)
        {
            return Channel.GetExternalUsers(page, perPage);
        }

        public int GetExternalUserCount()
        {
            return Channel.GetExternalUserCount();
        }

        public IEnumerable<string> SearchExternalUsers(string search)
        {
            return Channel.SearchExternalUsers(search);
        }

        public User GetExternalUser(int id)
        {
            return Channel.GetExternalUser(id);
        }

        public User AddExternalUser(User user)
        {
            return Channel.AddExternalUser(user);
        }

        public User UpdateExternalUser(User user)
        {
            return Channel.UpdateExternalUser(user);
        }

        public bool DeleteExternalUser(int id)
        {
            return Channel.DeleteExternalUser(id);
        }

        public IEnumerable<TimeTable> GetTimeTables()
        {
            return Channel.GetTimeTables();
        }

        public IEnumerable<TimeTableTypeInfo> GetTimeTableTypes()
        {
            return Channel.GetTimeTableTypes();
        }

        public IDictionary<string, string> GetTimeTableTypeData(string type)
        {
            return Channel.GetTimeTableTypeData(type);
        }

        public TimeTable GetTimeTable(int id)
        {
            return Channel.GetTimeTable(id);
        }

        public TimeTable AddTimeTable(TimeTable timeTable)
        {
            return Channel.AddTimeTable(timeTable);
        }

        public TimeTable UpdateTimeTable(TimeTable timeTable)
        {
            return Channel.UpdateTimeTable(timeTable);
        }

        public bool DeleteTimeTable(int id)
        {
            return Channel.DeleteTimeTable(id);
        }

        public Stream ExportExternalUsers()
        {
            return Channel.ExportExternalUsers();
        }

        public ImportResult ImportExternalUsers(Stream stream)
        {
            return Channel.ImportExternalUsers(stream);
        }

        public ImportResult ImportExternalUsersWithOverwrite(Stream stream)
        {
            return Channel.ImportExternalUsersWithOverwrite(stream);
        }

        public string[] GetCertificateFiles()
        {
            return Channel.GetCertificateFiles();
        }

        public void Start()
        {
            Channel.Start();
        }

        public void Stop()
        {
            Channel.Stop();
        }

        public bool IsRunning()
        {
            return Channel.IsRunning();
        }

        public void SendMail(MailMessage msg)
        {
            Channel.SendMail(msg);
        }

        public IEnumerable<Attachment> GetAttachments()
        {
            return Channel.GetAttachments();
        }

        public Attachment UploadAttachment(string name, int size, Stream stream)
        {
            return Channel.UploadAttachment(name, size, stream);
        }

        public Stream DownloadAttachment(string name)
        {
            return Channel.DownloadAttachment(name);
        }

        public bool RenameAttachment(string oldName, string newName)
        {
            return Channel.RenameAttachment(oldName, newName);
        }

        public bool DeleteAttachment(string name)
        {
            return Channel.DeleteAttachment(name);
        }

        public IEnumerable<MailTemplate> GetMailTemplates()
        {
            return Channel.GetMailTemplates();
        }
    }
}