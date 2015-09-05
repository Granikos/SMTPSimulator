using HydraService.Models;

namespace HydraService.Providers
{
    public interface IDomainProvider : IDataProvider<Domain, int>
    {
        Domain GetByName(string name);
    }
}