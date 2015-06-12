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

        [OperationContract]
        [WebGet(
            UriTemplate = "ServerBindings",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<RecieveConnector> GetServerBindings();

        [OperationContract]
        [WebGet(
            UriTemplate = "ServerBindings/{id}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        RecieveConnector GetServerBinding(int id);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ServerBindings",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        RecieveConnector AddServerBinding(RecieveConnector binding);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ServerBindings",
            Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        RecieveConnector UpdateServerBinding(RecieveConnector binding);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ServerBindings/{id}",
            Method = "DELETE",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        bool DeleteServerBinding(int id);

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
    }
}