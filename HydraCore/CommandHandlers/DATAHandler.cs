using System;
using System.Collections.Generic;
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

            var forwardPath = transaction.GetProperty<List<string>>("ForwardPath");

            if (!transaction.GetProperty<bool>("MailInProgress") || forwardPath == null || !forwardPath.Any())
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            transaction.StartDataMode(data =>
            {
                transaction.Server.TriggerNewMessage(transaction, transaction.GetProperty<string>("ReversePath"), forwardPath.ToArray(), data);

                transaction.Reset();

                return new SMTPResponse(SMTPStatusCode.Okay);
            });

            return new SMTPResponse(SMTPStatusCode.StartMailInput);
        }
    }
}