using System.Security.Cryptography.X509Certificates;
using HydraService.Models;

namespace HydraService.Providers
{
    public interface IDomainProvider : IDataProvider<Domain,int>
    {
    }
}