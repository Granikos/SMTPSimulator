using System.Collections.Generic;

namespace HydraService.Providers
{
    public interface IDomainProvider
    {
        IEnumerable<string> GetDomains();

        bool Exists(string domain);

        bool Add(string domain);

        bool Delete(string domain);
    }
}