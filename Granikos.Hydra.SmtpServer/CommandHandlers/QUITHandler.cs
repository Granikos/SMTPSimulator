using System.ComponentModel.Composition;
using Granikos.NikosTwo.Core;

namespace Granikos.NikosTwo.SmtpServer.CommandHandlers
{
    [UnsecureAllowed]
    [ExportMetadata("Command", "QUIT")]
    [Export(typeof (ICommandHandler))]
    public class QUITHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }


            transaction.Close();

            return new SMTPResponse(SMTPStatusCode.Closing);
        }
    }
}