using System.ComponentModel.Composition;
using Granikos.NikosTwo.Core;

namespace Granikos.NikosTwo.SmtpServer.CommandHandlers
{
    [UnsecureAllowed]
    [ExportMetadata("Command", "HELO")]
    [Export(typeof (ICommandHandler))]
    public class HELOHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            transaction.Reset();
            transaction.Initialize(parameters);

            return new SMTPResponse(SMTPStatusCode.Okay, transaction.Settings.Greet);
        }
    }
}