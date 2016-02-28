using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Linq.SqlClient;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Granikos.NikosTwo.Service.Database.Models;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;

namespace Granikos.NikosTwo.Service.Database.Providers
{
    [Export(typeof (IExternalUserProvider))]
    public class ExternalUserProvider : DefaultProvider<ExternalUser, IUser>, IExternalUserProvider<ExternalUser>, IExternalUserProvider
    {
        public ExternalUserProvider(): base(ModelHelpers.ConvertTo<ExternalUser>)
        {
        }

        private void OnUsersClear()
        {
            _domainCounts = null;
        }

        private void OnUserRemoved(User user)
        {
            if (_domainCounts != null)
            {
                var domain = user.Mailbox.Split('@')[1];
                if (_domainCounts.ContainsKey(domain))
                {
                    var count = _domainCounts[domain] - 1;

                    if (count <= 0)
                    {
                        _domainCounts.Remove(domain);
                    }
                    else
                    {
                        _domainCounts[domain] = count;
                    }
                }
            }
        }

        private void OnUserAdded(User user)
        {
            if (_domainCounts != null)
            {
                var domain = user.Mailbox.Split('@')[1];
                if (_domainCounts.ContainsKey(domain))
                {
                    _domainCounts[domain]++;
                }
                else
                {
                    _domainCounts.Add(domain, 1);
                }
            }
        }

        public IEnumerable<ExternalUser> GetByDomain(string domain)
        {
            domain = domain.StartsWith("*")? domain.Substring(1) : "@" + domain;

            return Database.Set<ExternalUser>().Where(u => u.Mailbox.ToLower().EndsWith(domain));
        }

        private Dictionary<string, int> _domainCounts;

        private void RefreshDomains()
        {
            Contract.Ensures(_domainCounts != null);

            var domainCounts = All()
                .Select(u => u.Mailbox.Split('@')[1])
                .GroupBy(d => d, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(d => d.Key, d => d.Count());

            _domainCounts = new Dictionary<string, int>(domainCounts, StringComparer.OrdinalIgnoreCase);
        }

        IEnumerable<IUser> IExternalUserProvider<IUser>.GetByDomain(string domain)
        {
            return GetByDomain(domain);
        }

        public IEnumerable<ValueWithCount<string>> SearchDomains(string domain)
        {
            if (_domainCounts == null)
            {
                RefreshDomains();
            }

            return _domainCounts
                .Where(
                    pair =>
                        CultureInfo.InvariantCulture.CompareInfo.IndexOf(pair.Key, domain, CompareOptions.IgnoreCase) >=
                        0)
                .Select(d => new ValueWithCount<string>(d.Key, d.Value));
        }

        public IEnumerable<string> SearchMailboxes(string search, int max)
        {
            return
                All().Select(u => string.Format("{0} {1} <{2}>", u.FirstName, u.LastName, u.Mailbox))
                    .Where(
                        m => CultureInfo.InvariantCulture.CompareInfo.IndexOf(m, search, CompareOptions.IgnoreCase) >= 0)
                    .Take(max);
        }

#if DEBUG
        protected IEnumerable<User> Initializer()
        {
            yield return new ExternalUser
            {
                Mailbox = "bernd.mueller@test.de"
            };
            yield return new ExternalUser
            {
                Mailbox = "max.muetze@test.de"
            };
            yield return new ExternalUser
            {
                Mailbox = "manuel.krebber@domain.com"
            };
        }
#endif

        protected override IOrderedQueryable<ExternalUser> ApplyOrder(IQueryable<ExternalUser> entities)
        {
            return entities.OrderBy(u => u.Mailbox.ToLower());
        }
    }
}