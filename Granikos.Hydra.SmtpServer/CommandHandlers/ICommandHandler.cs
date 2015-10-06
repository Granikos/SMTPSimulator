using Granikos.Hydra.Core;

namespace Granikos.Hydra.SmtpServer.CommandHandlers
{
    public interface ICommandHandler
    {
        void Initialize(SMTPCore core);
        SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }
}