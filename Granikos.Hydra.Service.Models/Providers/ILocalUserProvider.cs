using System.Collections.Generic;
using System.IO;

namespace Granikos.Hydra.Service.Models.Providers
{
    public interface ILocalUserProvider<TUser> : IDataProvider<TUser, int>
        where TUser : IUser
    {
        IEnumerable<string> SearchMailboxes(string search, int max);

        IEnumerable<TUser> GetByDomain(string domain);

        IEnumerable<ValueWithCount<string>> SearchDomains(string domain);
    }

    public interface ILocalUserProvider : ILocalUserProvider<IUser>
    {
    }
}