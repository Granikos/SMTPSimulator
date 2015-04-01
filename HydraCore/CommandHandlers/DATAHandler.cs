using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "DATA")]
    [Export(typeof(ICommandHandler))]
    public class DATAHandler : ICommandHandler
    {
        public SMTPResponse Execute(SMTPTransaction transaction, string parameters)
        {
            if (!String.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            if (!transaction.MailInProgress || !transaction.ForwardPath.Any())
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            transaction.StartDataMode(data =>
            {
                transaction.Server.TriggerNewMessage(transaction, transaction.ReversePath, transaction.ForwardPath.ToArray(), data);

                transaction.Reset();

                return new SMTPResponse(SMTPStatusCode.Okay);
            });

            return new SMTPResponse(SMTPStatusCode.StartMailInput);
        }
    }
}