// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using log4net;

namespace Granikos.SMTPSimulator.Core.Logging
{
    [Export(typeof (ISMTPLogger))]
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

        public void Log(string connectorId, string session, IPEndPoint local, IPEndPoint remote, LogPartType part,
            LogEventType type,
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