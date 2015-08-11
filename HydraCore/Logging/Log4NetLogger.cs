using System.ComponentModel.Composition;
using System.Net;
using log4net;

namespace HydraCore.Logging
{
    [Export(typeof(ISMTPLogger))]
    public class Log4NetLogger : ISMTPLogger
    {
        private static readonly ILog Logger = LogManager.GetLogger("SMTP");

        public void Log(string connectorId, string session, IPEndPoint local, IPEndPoint remote, LogPartType part, LogEventType type,
            string data)
        {
            Logger.Info(new LogEvent
            {
                Component = part.GetSymbol(),
                ConnectorId = connectorId,
                LocalIP = local,
                RemoteIP = remote,
                Message = data,
                Session = session,
                Type = type.GetSymbol()
            });
        }

    }
}