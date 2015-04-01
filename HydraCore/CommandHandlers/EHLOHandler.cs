using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "EHLO")]
    [Export(typeof(ICommandHandler))]
    public class EHLOHandler : CommandHandlerBase
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