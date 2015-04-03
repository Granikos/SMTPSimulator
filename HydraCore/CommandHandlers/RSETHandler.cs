using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "RSET")]
    [Export(typeof (ICommandHandler))]
    public class RSETHandler : CommandHandlerBase
    {
        public override SMTPResponse Execute(SMTPTransaction transaction, string parameters)
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