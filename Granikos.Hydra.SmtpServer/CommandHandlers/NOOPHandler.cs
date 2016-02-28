using System.ComponentModel.Composition;
using Granikos.NikosTwo.Core;

namespace Granikos.NikosTwo.SmtpServer.CommandHandlers
{
    [UnsecureAllowed]
    [ExportMetadata("Command", "NOOP")]
    [Export(typeof (ICommandHandler))]
    public class NOOPHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}