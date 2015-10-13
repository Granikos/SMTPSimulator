using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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