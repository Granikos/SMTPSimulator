using System.Net;

namespace Granikos.NikosTwo.Core.Logging
{
    public interface ISMTPLogger
    {
        void StartSession(string session);

        void Log(string connectorId, string session, IPEndPoint local, IPEndPoint remote, LogPartType part,
            LogEventType type, string data);

        void EndSession(string session);
    }
}