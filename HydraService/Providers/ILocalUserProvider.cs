using HydraService.Models;

namespace HydraService.Providers
{
    public interface ILocalUserProvider : IDataProvider<LocalUser,int>
    {
        LocalUser GetByEmail(string email);
    }
}