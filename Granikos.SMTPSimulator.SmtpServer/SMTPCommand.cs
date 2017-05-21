using System;

namespace Granikos.SMTPSimulator.SmtpServer
{
    public sealed class SMTPCommand
    {
        public readonly string Command;
        public readonly string Parameters;

        public SMTPCommand(string command, string parameters = null)
        {
            if (command == null) throw new ArgumentNullException();

            Command = command.ToUpperInvariant();
            Parameters = parameters;
        }
    }
}