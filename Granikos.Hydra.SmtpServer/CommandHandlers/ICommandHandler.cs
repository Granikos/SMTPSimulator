using Granikos.Hydra.Core;

namespace Granikos.Hydra.SmtpServer.CommandHandlers
{
    public interface ICommandHandler
    {
        void Initialize(SMTPServer server);
        SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }
}