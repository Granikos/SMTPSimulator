using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.SqlServer;
using System.Data.Linq.SqlClient;
using System.Diagnostics.Contracts;
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
                return Database.LocalUsers.Where(u => u.Mailbox.Substring(u.Mailbox.IndexOf("@") + 1).ToLower().EndsWith(domain));
            }

            return Database.LocalUsers.Where(u => u.Mailbox.Substring(u.Mailbox.IndexOf("@") + 1).ToLower().Equals(domain));
        }

        public IEnumerable<ValueWithCount<string>> SearchDomains(string domain)
        {

            domain = domain.ToLower();

            try
            {
                return Database.LocalUsers
                    .Where(u => u.Mailbox.Substring(u.Mailbox.IndexOf("@") + 1).ToLower().Contains(domain))
                    .Select(u => u.Mailbox.Substring(u.Mailbox.IndexOf("@") + 1).ToLower())
                    .GroupBy(v => v)
                    .ToList()
                    .Select(d => new ValueWithCount<string>(d.Key, d.Count()));
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override IOrderedQueryable<LocalUser> ApplyOrder(IQueryable<LocalUser> entities)
        {
            return entities.OrderBy(u => u.Mailbox.ToLower());
        }
    }
}