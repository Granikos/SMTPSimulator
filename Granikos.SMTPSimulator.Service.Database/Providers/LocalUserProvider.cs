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
using System.Data.Entity.SqlServer;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.Linq;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
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