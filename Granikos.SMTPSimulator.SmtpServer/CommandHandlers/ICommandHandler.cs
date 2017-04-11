using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
{
    public interface ICommandHandler
    {
        void Initialize(SMTPServer server);
        SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }
}