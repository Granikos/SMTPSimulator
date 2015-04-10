namespace HydraCore.CommandHandlers
{
    public interface ICommandHandler
    {
        void Initialize(SMTPCore core);
        SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }
}