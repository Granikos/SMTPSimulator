using System;
using System.Diagnostics.Contracts;

namespace Granikos.SMTPSimulator.SmtpServer
{
    public sealed class SMTPCommand
    {
        public readonly string Command;
        public readonly string Parameters;

        public SMTPCommand(string command, string parameters = null)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            Command = command.ToUpperInvariant();
            Parameters = parameters;
        }
    }
}