using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using HydraCore;
using HydraService.Models;

namespace HydraService
{
    [ServiceContract]
    public interface IConfigurationService
    {
        [OperationContract]
        void SetProperty(string name, string value);

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
        RecieveConnector AddRecieveConnector(RecieveConnector binding);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "RecieveConnectors",
            Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        RecieveConnector UpdateRecieveConnector(RecieveConnector binding);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "RecieveConnectors/{id}",
            Method = "DELETE",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        bool DeleteRecieveConnector(int id);

        [OperationContract]
        IEnumerable<ServerSubnetConfiguration> GetSubnets();

        [OperationContract]
        ServerSubnetConfiguration GetSubnet(int id);

        [OperationContract]
        ServerSubnetConfiguration AddSubnet(ServerSubnetConfiguration subnet);

        [OperationContract]
        ServerSubnetConfiguration UpdateSubnet(ServerSubnetConfiguration subnet);

        [OperationContract]
        bool DeleteSubnet(int id);

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