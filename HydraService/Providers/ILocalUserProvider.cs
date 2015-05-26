using HydraService.Models;

namespace HydraService.Providers
{
    public interface ILocalUserProvider : IDataProvider<LocalUser>
    {
        LocalUser GetByEmail(string email);
    }
}