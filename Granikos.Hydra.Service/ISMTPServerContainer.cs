namespace Granikos.Hydra.Service
{
    public interface ISMTPServerContainer
    {
        bool Running { get; }
        void StopSMTPServers();
        void StartSMTPServers();
    }
}