using Granikos.NikosTwo.Core;

namespace Granikos.NikosTwo.SmtpServer.CommandHandlers
{
    public interface ICommandHandler
    {
        void Initialize(SMTPServer server);
        SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }
}