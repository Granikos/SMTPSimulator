using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "QUIT")]
    [Export(typeof (ICommandHandler))]
    public class QUITHandler : CommandHandlerBase
    {
        public override SMTPResponse Execute(SMTPTransaction transaction, string parameters)
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