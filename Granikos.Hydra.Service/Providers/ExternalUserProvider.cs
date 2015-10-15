using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    [Export(typeof (IExternalUserProvider))]
    public class ExternalUserProvider : DefaultProvider<User>, IExternalUserProvider
    {
        public ExternalUserProvider()
            : base("ExternalUsers")
        {
            OnAdded += OnUserAdded;
            OnRemoved += OnUserRemoved;
            OnClear += OnUsersClear;
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

        public IEnumerable<User> GetByDomain(string domain)
        {
            domain = domain.StartsWith("*")? domain.Substring(1) : "@" + domain;

            return All().Where(u => u.Mailbox.EndsWith(domain, StringComparison.InvariantCultureIgnoreCase));
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

        public int ImportFromCSV(Stream stream, bool overwrite)
        {
            try
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var config = new CsvConfiguration
                    {
                        Delimiter = ";",
                        IgnoreReadingExceptions = true
                    };
                    config.RegisterClassMap<CsvMap>();

                    var csv = new CsvReader(reader, config);
                    var records = csv.GetRecords<User>().ToList();

                    if (overwrite)
                    {
                        Clear();
                    }

                    return records.Count(user => Add(user) != null);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int ExportAsCSV(Stream stream)
        {
            var config = new CsvConfiguration
            {
                Delimiter = ";"
            };
            config.RegisterClassMap<CsvMap>();

            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1000, true))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(All());

                return All().Count();
            }
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
        protected override IEnumerable<User> Initializer()
        {
            yield return new User
            {
                Mailbox = "bernd.mueller@test.de"
            };
            yield return new User
            {
                Mailbox = "max.muetze@test.de"
            };
            yield return new User
            {
                Mailbox = "manuel.krebber@domain.com"
            };
        }
#endif

        protected override IOrderedEnumerable<User> ApplyOrder(IEnumerable<User> entities)
        {
            return entities.OrderBy(u => u.Mailbox, StringComparer.InvariantCultureIgnoreCase);
        }

        private class CsvMap : CsvClassMap<User>
        {
            public CsvMap()
            {
                Map(m => m.Id).Ignore();
                Map(m => m.Mailbox).Name("Mailbox", "Email Address", "EmailAddress").Index(2);
                Map(m => m.FirstName).Name("FirstName", "First Name").Index(0);
                Map(m => m.LastName).Name("LastName", "Last Name").Index(1);
            }
        }
    }
}