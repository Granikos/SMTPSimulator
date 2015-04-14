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
    }
}