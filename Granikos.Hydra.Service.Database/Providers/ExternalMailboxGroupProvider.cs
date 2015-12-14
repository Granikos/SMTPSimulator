using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using Granikos.Hydra.Service.Database.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Database.Providers
{
    [Export(typeof(IExternalMailboxGroupProvider))]
    public class ExternalMailboxGroupProvider : DefaultProvider<ExternalUserGroup,IUserGroup>, IExternalMailboxGroupProvider<ExternalUserGroup>, IExternalMailboxGroupProvider
    {
        public ExternalMailboxGroupProvider() : base(ExternalUserGroup.From)
        {
        }

        public ExternalUserGroup Add(string name)
        {
            return Add(new ExternalUserGroup { Name = name });
        }

        IUserGroup IExternalMailboxGroupProvider<IUserGroup>.Add(string name)
        {
            return Add(name);
        }

        protected override IOrderedQueryable<ExternalUserGroup> ApplyOrder(IQueryable<ExternalUserGroup> entities)
        {
            return entities.Include(g => g.Users).OrderBy(g => g.Name.ToLower());
        }

        protected override void OnUpdate(ExternalUserGroup entity, ExternalUserGroup dbEntity)
        {
            dbEntity.Users.Clear();

            var users = Database.ExternalUsers.Where(u => entity.UserIds.Contains(u.Id)).ToList();

            foreach (var user in users)
            {
                dbEntity.Users.Add(user);
            }
        }
    }
}
