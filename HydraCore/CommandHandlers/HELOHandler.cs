using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "HELO")]
    [Export(typeof (ICommandHandler))]
    public class HELOHandler : CommandHandlerBase
    {
        public override SMTPResponse Execute(SMTPTransaction transaction, string parameters)
        {
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            transaction.Reset();
            transaction.Initialize(parameters);

            return new SMTPResponse(SMTPStatusCode.Okay, Server.Greet);
        }
    }
}