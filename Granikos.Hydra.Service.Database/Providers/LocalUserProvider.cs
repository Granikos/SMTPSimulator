using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using Granikos.Hydra.Service.Database.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Database.Providers
{
    [Export(typeof (ILocalUserProvider))]
    public class LocalUserProvider : DefaultProvider<LocalUser, IUser>, ILocalUserProvider<LocalUser>, ILocalUserProvider
    {

        public LocalUserProvider() : base(ModelHelpers.ConvertTo<LocalUser>)
        {
        }

        public IEnumerable<string> SearchMailboxes(string search, int max)
        {
            return All()
                .Select(u => string.Format("{0} {1} <{2}>", u.FirstName, u.LastName, u.Mailbox))
                .Where(m => CultureInfo.InvariantCulture.CompareInfo.IndexOf(m, search, CompareOptions.IgnoreCase) >= 0)
                .Take(max);
        }

        IEnumerable<IUser> ILocalUserProvider<IUser>.GetByDomain(string domain)
        {
            return GetByDomain(domain);
        }

        public IEnumerable<LocalUser> GetByDomain(string domain)
        {
            if (domain.StartsWith("*"))
            {
                domain = domain.Substring(1);
                return All().Where(u => u.Mailbox.EndsWith(domain));
            }

            return All().Where(u => u.Mailbox.Equals(domain));
        }

#if DEBUG
        protected IEnumerable<User> Initializer()
        {
            yield return new LocalUser
            {
                FirstName = "Bernd",
                LastName = "Müller",
                Mailbox = "bernd.mueller@test.de"
            };
            yield return new LocalUser
            {
                FirstName = "Eva",
                LastName = "Schmidt",
                Mailbox = "eva.schmidt@test.de"
            };
        }
#endif

        protected override IOrderedQueryable<LocalUser> ApplyOrder(IQueryable<LocalUser> entities)
        {
            return entities.OrderBy(u => u.Mailbox.ToLower());
        }
    }
}