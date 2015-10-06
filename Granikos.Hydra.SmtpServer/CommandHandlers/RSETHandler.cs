using System.ComponentModel.Composition;
using Granikos.Hydra.Core;

namespace Granikos.Hydra.SmtpServer.CommandHandlers
{
    [UnsecureAllowed]
    [ExportMetadata("Command", "RSET")]
    [Export(typeof (ICommandHandler))]
    public class RSETHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            transaction.Reset();
            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}