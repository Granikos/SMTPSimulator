using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "RSET")]
    [Export(typeof(ICommandHandler))]
    public class RSETHandler : ICommandHandler
    {
        public SMTPResponse Execute(SMTPTransaction transaction, string parameters, string data)
        {
            if (!String.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            transaction.Reset();
            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}