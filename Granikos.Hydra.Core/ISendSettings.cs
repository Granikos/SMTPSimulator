namespace Granikos.Hydra.Core
{
    public interface ISendSettings
    {
        bool UseAuth { get; }
        string Username { get; }
        string Password { get; }
        bool RequireTLS { get; }
        bool EnableTLS { get; }
    }
}