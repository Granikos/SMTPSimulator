using System.Security.Cryptography.X509Certificates;

namespace HydraCore.CommandHandlers
{
    public interface ICommandHandler
    {
        void Initialize(SMTPCore core);

        SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }

    public abstract class CommandHandlerBase : ICommandHandler
    {
        public SMTPCore Server { get; private set; }

        public virtual void Initialize(SMTPCore core)
        {
            Server = core;
        }

        public abstract SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }
}
