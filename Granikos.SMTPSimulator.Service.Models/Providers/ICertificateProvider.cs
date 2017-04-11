using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Granikos.SMTPSimulator.Service.Models.Providers
{
    public interface ICertificateProvider
    {
        X509Certificate2 GetCertificate(string name, string password);

        IEnumerable<string> ListCertificates();
    }
}