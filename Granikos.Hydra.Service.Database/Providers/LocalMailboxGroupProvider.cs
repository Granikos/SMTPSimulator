using System.Collections.Generic;
using System.ComponentModel.Composition;
using Granikos.Hydra.Service.Database.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Database.Providers
{
    [Export(typeof(ILocalMailboxGroupProvider))]
    public class LocalMailboxGroupProvider : DefaultProvider<LocalUserGroup,IUserGroup>, ILocalMailboxGroupProvider<LocalUserGroup>, ILocalMailboxGroupProvider
    {
        public LocalMailboxGroupProvider() : base(LocalUserGroup.From)
        {
        }

#if DEBUG
        protected IEnumerable<UserGroup> Initializer()
        {
            yield return new LocalUserGroup("Group 1");
            yield return new LocalUserGroup("Group 2");
        }
#endif

        public LocalUserGroup Add(string name)
        {
            return Add(new LocalUserGroup {Name = name});
        }

        IUserGroup ILocalMailboxGroupProvider<IUserGroup>.Add(string name)
        {
            return Add(name);
        }
    }
}