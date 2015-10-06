using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    public interface IDomainProvider : IDataProvider<Domain, int>
    {
        Domain GetByName(string name);
    }
}