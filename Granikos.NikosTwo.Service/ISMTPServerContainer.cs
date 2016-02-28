namespace Granikos.NikosTwo.Service
{
    public interface ISMTPServerContainer
    {
        bool Running { get; }
        void StopSMTPServers();
        void StartSMTPServers();
    }
}