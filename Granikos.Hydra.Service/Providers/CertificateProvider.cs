using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Providers
{
    [DisplayName("Store")]
    [Export("store", typeof(ICertificateProvider))]
    public class CertificateProvider : ICertificateProvider
    {
        public X509Certificate2 GetCertificate(string name, string passsword)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, name, true);

            return certs[0];
        }

        public IEnumerable<string> ListCertificates()
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                return store.Certificates.Cast<X509Certificate2>()
                    .Select(c => c.SubjectName.Name)
                    .ToArray();
            }
            finally
            {
                store.Close();
            }
        }
    }
}
