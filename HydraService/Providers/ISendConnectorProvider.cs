using HydraService.Models;

namespace HydraService.Providers
{
    public interface ISendConnectorProvider : IDataProvider<SendConnector, int>
    {
        int DefaultId { get; set; }

        SendConnector DefaultConnector { get; } 
    }
}