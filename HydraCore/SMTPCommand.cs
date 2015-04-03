using System;
using System.Diagnostics.Contracts;

namespace HydraCore
{
    public sealed class SMTPCommand
    {
        public readonly string Command;
        public readonly string Parameters;

        public SMTPCommand(string command, string parameters = null)
        {
            Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command));

            Command = command;
            Parameters = parameters;
        }
    }
}