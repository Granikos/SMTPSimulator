using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "NOOP")]
    [Export(typeof(ICommandHandler))]
    public class NOOPHandler : CommandHandlerBase
    {
        public override SMTPResponse Execute(SMTPTransaction transaction, string parameters)
        {
            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}