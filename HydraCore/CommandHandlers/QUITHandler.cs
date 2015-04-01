using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "QUIT")]
    [Export(typeof(ICommandHandler))]
    public class QUITHandler : ICommandHandler
    {
        public SMTPResponse Execute(SMTPTransaction transaction, string parameters, string data)
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