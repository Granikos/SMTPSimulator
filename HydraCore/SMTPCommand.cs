using System;
using System.Diagnostics.Contracts;

namespace HydraCore
{
    public sealed class SMTPCommand
    {
        public SMTPCommand(string command, string parameters = null)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Equals(command.ToUpperInvariant()), "All SMTP Commands must be upper case");

            Command = command;
            Parameters = parameters;
        }

        public readonly string Command;

        public readonly string Parameters;
    }
}