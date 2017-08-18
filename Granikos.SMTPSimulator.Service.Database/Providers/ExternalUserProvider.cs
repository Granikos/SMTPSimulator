// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
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