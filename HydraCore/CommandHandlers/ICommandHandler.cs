namespace HydraCore.CommandHandlers
{
    public interface ICommandHandler
    {
        void Initialize(SMTPCore core);

        void Initialize(SMTPTransaction transaction);

        SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }
}