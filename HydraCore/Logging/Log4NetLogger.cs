using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using log4net;

namespace HydraCore.Logging
{
    [Export(typeof(ISMTPLogger))]
    public class Log4NetLogger : ISMTPLogger
    {
        private static readonly ILog LoggerClient = LogManager.GetLogger("SMTPClient");
        private static readonly ILog LoggerServer = LogManager.GetLogger("SMTPServer");
        private static readonly ILog LoggerOther = LogManager.GetLogger("SMTPOther");

        private static readonly Dictionary<string, int> SequenceNumbers = new Dictionary<string, int>();

        public void StartSession(string session)
        {
            SequenceNumbers.Add(session, 1);
        }

        public void Log(string connectorId, string session, IPEndPoint local, IPEndPoint remote, LogPartType part, LogEventType type,
            string data)
        {
            ILog logger;
            switch (part)
            {
                case LogPartType.Client:
                    logger = LoggerClient;
                    break;
                case LogPartType.Server:
                    logger = LoggerServer;
                    break;
                default:
                    logger = LoggerOther;
                    break;
            }

            var sequence = SequenceNumbers[session];
            SequenceNumbers[session] = sequence + 1;


            logger.Info(new LogEvent
            {
                ConnectorId = connectorId,
                LocalIP = local,
                RemoteIP = remote,
                Message = data,
                Session = session,
                Type = type.GetSymbol(),
                SequenceNumber = sequence
            });
        }

        public void EndSession(string session)
        {
            SequenceNumbers.Remove(session);
        }
    }
}