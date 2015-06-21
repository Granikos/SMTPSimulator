namespace HydraService
{
    public interface ISMTPServerContainer
    {
        bool Running { get; }
        void StopSMTPServers();
        void StartSMTPServers();
    }
}