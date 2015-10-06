using System;
using System.ComponentModel.Composition;
using System.Net;

namespace Granikos.Hydra.Core.Logging
{
    [Export(typeof (ISMTPLogger))]
    public class ConsoleLogger : ISMTPLogger
    {
        public void StartSession(string session)
        {
        }

        public void Log(string connectorId, string session, IPEndPoint local, IPEndPoint remote, LogPartType part,
            LogEventType type,
            string data)
        {
            if (type.IsConnectionEvent())
            {
                Console.WriteLine("[{0}] {1}{2}  L:{3} R:{4}", connectorId, part.GetSymbol(), type.GetSymbol(),
                    local, remote);
            }
            else
            {
                foreach (var l in (data ?? "").Split(new[] {"\r\n"}, StringSplitOptions.None))
                {
                    Console.WriteLine("[{0}] {1}{2} {3}", connectorId, part.GetSymbol(), type.GetSymbol(), l);
                }
            }
        }

        public void EndSession(string session)
        {
        }
    }
}