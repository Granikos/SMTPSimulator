using HydraService.Models;

namespace HydraService.Providers
{
    public interface IExternalUserProvider : IDataProvider<ExternalUser, int>
    {
        ExternalUser GetByEmail(string email);
    }
}