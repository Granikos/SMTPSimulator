namespace Granikos.SMTPSimulator.Service.Models.Providers
{
    public interface ISendConnectorProvider<TSendConnector> : IDataProvider<TSendConnector, int>
        where TSendConnector : ISendConnector
    {
        int DefaultId { get; set; }

        TSendConnector DefaultConnector { get; }

        TSendConnector GetByDomain(string domain);

        TSendConnector GetEmptyConnector();
    }

    public interface ISendConnectorProvider : ISendConnectorProvider<ISendConnector>
    {
    }
}