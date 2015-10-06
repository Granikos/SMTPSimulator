using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    public interface ISendConnectorProvider : IDataProvider<SendConnector, int>
    {
        int DefaultId { get; set; }
        SendConnector DefaultConnector { get; }
    }
}