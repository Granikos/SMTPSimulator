using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
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