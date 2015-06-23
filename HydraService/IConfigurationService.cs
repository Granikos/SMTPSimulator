using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using HydraService.Models;

namespace HydraService
{
    [ServiceContract]
    public interface IConfigurationService
    {
        [OperationContract]
        void SetProperty(string name, string value);

        [OperationContract]
        [WebGet(
            UriTemplate = "Domains",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<Domain> GetDomains();

        [WebGet(
            UriTemplate = "Domains/{domain}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        Domain GetDomain(string domain);

        [WebInvoke(
            UriTemplate = "Domains",
            Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        Domain UpdateDomain(Domain domain);

        [WebInvoke(
            UriTemplate = "Domains/{domain}",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        Domain AddDomain(string domain);

        [WebInvoke(
            UriTemplate = "Domains/{id}",
            Method = "DELETE",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        bool DeleteDomain(int id);

        // TODO: Clean up
        [OperationContract]
        [WebGet(
            UriTemplate = "DefaultRecieveConnector",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        RecieveConnector GetDefaultRecieveConnector();

        [OperationContract]
        [WebGet(
            UriTemplate = "RecieveConnectors",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<RecieveConnector> GetRecieveConnectors();

        [OperationContract]
        [WebGet(
            UriTemplate = "RecieveConnectors/{id}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        RecieveConnector GetRecieveConnector(int id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "RecieveConnectors",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        RecieveConnector AddRecieveConnector(RecieveConnector connector);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "RecieveConnectors",
            Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        RecieveConnector UpdateRecieveConnector(RecieveConnector connector);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "RecieveConnectors/{id}",
            Method = "DELETE",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        bool DeleteRecieveConnector(int id);

        [OperationContract]
        [WebGet(
            UriTemplate = "DefaultSendConnector",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        SendConnector GetEmptySendConnector();

        [WebGet(
            UriTemplate = "DefaultSendConnector",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        SendConnector GetDefaultSendConnector();

        [WebInvoke(
            UriTemplate = "DefaultSendConnector/{id}",
            Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        bool SetDefaultSendConnector(int id);

        [OperationContract]
        [WebGet(
            UriTemplate = "SendConnectors",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<SendConnector> GetSendConnectors();

        [OperationContract]
        [WebGet(
            UriTemplate = "SendConnectors/{id}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        SendConnector GetSendConnector(int id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "SendConnectors",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        SendConnector AddSendConnector(SendConnector connector);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "SendConnectors",
            Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        SendConnector UpdateSendConnector(SendConnector connector);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "SendConnectors/{id}",
            Method = "DELETE",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        bool DeleteSendConnector(int id);

        [OperationContract]
        IEnumerable<LocalUser> GetLocalUsers();

        [OperationContract]
        LocalUser GetLocalUser(int id);

        [OperationContract]
        LocalUser AddLocalUser(LocalUser user);

        [OperationContract]
        LocalUser UpdateLocalUser(LocalUser user);

        [OperationContract]
        bool DeleteLocalUser(int id);

        [OperationContract]
        [WebGet(
            UriTemplate = "Certificates",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        string[] GetCertificateFiles();

        [OperationContract]
        [WebGet(
            UriTemplate = "Server/Start",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        void Start();

        [OperationContract]
        [WebGet(
            UriTemplate = "Server/Stop",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        void Stop();

        [OperationContract]
        [WebGet(
            UriTemplate = "Server/IsRunning",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        bool IsRunning();
    }
}