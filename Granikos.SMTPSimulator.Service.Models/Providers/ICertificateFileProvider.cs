using System.Collections.Generic;

namespace Granikos.SMTPSimulator.Service.Models.Providers
{
    public interface ICertificateFileProvider<TCertificate>
        where TCertificate : ICertificate
    {
        int Total { get; }

        IEnumerable<TCertificate> All();

        TCertificate Get(string name);

        bool Add(TCertificate entity);

        bool Delete(string name);

        bool Clear();
    }

    public interface ICertificateFileProvider : ICertificateFileProvider<ICertificate>
    {
    }
}