using System.Collections.Generic;
using System.ServiceModel;

namespace HydraService
{
    [ServiceContract]
    public interface IConfigurationService
    {
        [OperationContract]
        void SetProperty(string name, string value);

        [OperationContract]
        IList<ServerBindingConfiguration> GetServerBindings();

        [OperationContract]
        IList<LocalUser> GetLocalUsers();

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