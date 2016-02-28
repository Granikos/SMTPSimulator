namespace Granikos.NikosTwo.Service.Models.Providers
{
    public interface IReceiveConnectorProvider<TReceiveConnector> : IDataProvider<TReceiveConnector, int>
        where TReceiveConnector : IReceiveConnector
    {

        TReceiveConnector GetEmptyConnector();
    }

    public interface IReceiveConnectorProvider : IReceiveConnectorProvider<IReceiveConnector>
    {
    }
}