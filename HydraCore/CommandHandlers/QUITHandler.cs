using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [UnsecureAllowed]
    [ExportMetadata("Command", "QUIT")]
    [Export(typeof (ICommandHandler))]
    public class QUITHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (!String.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }


            transaction.Close();

            return new SMTPResponse(SMTPStatusCode.Closing);
        }
    }
}