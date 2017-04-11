using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
{
    [Export(typeof(ILocalMailboxGroupProvider))]
    public class LocalMailboxGroupProvider : DefaultProvider<LocalUserGroup,IUserGroup>, ILocalMailboxGroupProvider<LocalUserGroup>, ILocalMailboxGroupProvider
    {
        public LocalMailboxGroupProvider() : base(LocalUserGroup.From)
        {
        }

        public LocalUserGroup Add(string name)
        {
            return Add(new LocalUserGroup {Name = name});
        }

        IUserGroup ILocalMailboxGroupProvider<IUserGroup>.Add(string name)
        {
            return Add(name);
        }

        protected override IOrderedQueryable<LocalUserGroup> ApplyOrder(IQueryable<LocalUserGroup> entities)
        {
            return entities.Include(g => g.Users).OrderBy(g => g.Name.ToLower());
        }

        protected override void OnUpdate(LocalUserGroup entity, LocalUserGroup dbEntity)
        {
            dbEntity.Users.Clear();

            var users = Database.LocalUsers.Where(u => entity.UserIds.Contains(u.Id)).ToList();

            foreach (var user in users)
            {
                dbEntity.Users.Add(user);
            }
        }
    }
}