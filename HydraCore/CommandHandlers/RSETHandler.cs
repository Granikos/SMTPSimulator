using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [UnsecureAllowed]
    [ExportMetadata("Command", "RSET")]
    [Export(typeof (ICommandHandler))]
    public class RSETHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
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