using System;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
{
    public class CommandExecuteEventArgs : EventArgs
    {
        public CommandExecuteEventArgs(SMTPTransaction transaction, ICommandHandler handler, string parameters)
        {
            Transaction = transaction;
            Handler = handler;
            Parameters = parameters;
        }

        public SMTPTransaction Transaction { get; private set; }
        public ICommandHandler Handler { get; private set; }
        public string Parameters { get; private set; }
        public SMTPResponse Response { get; set; }
    }
}