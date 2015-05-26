using HydraService.Models;

namespace HydraService.Providers
{
    public interface IExternalUserProvider : IDataProvider<ExternalUser>
    {
        ExternalUser GetByEmail(string email);
    }
}