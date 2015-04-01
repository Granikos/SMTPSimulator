namespace HydraCore.CommandHandlers
{
    public interface ICommandHandler
    {
        SMTPResponse Execute(SMTPTransaction transaction, string parameters);
    }
}
