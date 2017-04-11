namespace Granikos.SMTPSimulator.SmtpServer
{
    public interface IReceiveSettings
    {
        string Banner { get; }
        string Greet { get; }
        bool RequireAuth { get; }
        bool RequireTLS { get; }
        bool EnableTLS { get; }
    }
}