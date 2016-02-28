using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Granikos.NikosTwo.Service.Models.Providers
{
    public interface ICertificateProvider
    {
        X509Certificate2 GetCertificate(string name, string password);

        IEnumerable<string> ListCertificates();
    }
}