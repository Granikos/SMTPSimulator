using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [UnsecureAllowed]
    [ExportMetadata("Command", "HELO")]
    [Export(typeof (ICommandHandler))]
    public class HELOHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            transaction.Reset();
            transaction.Initialize(parameters);

            return new SMTPResponse(SMTPStatusCode.Okay, Server.Config.Greet);
        }
    }
}