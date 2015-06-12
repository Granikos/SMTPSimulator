using System.Net;

namespace HydraCore.Logging
{
    public interface ISMTPLogger
    {
        void Log(string connectorId, string session, IPEndPoint local, IPEndPoint remote, LogPartType part, LogEventType type, string data);
    }
}