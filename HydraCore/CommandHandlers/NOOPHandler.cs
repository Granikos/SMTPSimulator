using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "NOOP")]
    [Export(typeof(ICommandHandler))]
    public class NOOPHandler : ICommandHandler
    {
        public SMTPResponse Execute(SMTPTransaction transaction, string parameters, string data)
        {
            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}