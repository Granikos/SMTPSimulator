using System.Net;
using System.Net.Security;
using System.Security.Authentication;

namespace Granikos.NikosTwo.Core
{
    public interface ISendSettings
    {
        bool UseAuth { get; }
        string Username { get; }
        string Password { get; }
        bool RequireTLS { get; }
        bool EnableTLS { get; }
        bool TLSFullTunnel { get; }
        ICredentials Credentials { get; }
        EncryptionPolicy TLSEncryptionPolicy { get; }
        SslProtocols SslProtocols { get; }
        bool ValidateCertificateRevocation { get; }
    }
}