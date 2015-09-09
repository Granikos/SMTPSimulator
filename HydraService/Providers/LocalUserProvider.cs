using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(ILocalUserProvider))]
    public class LocalUserProvider : DefaultProvider<LocalUser>, ILocalUserProvider
    {
        [ImportMany]
        private IEnumerable<IUserTemplateProvider> _templateProviders;

        public LocalUserProvider()
            : base("LocalUsers")
        {
        }

        public int ImportFromCSV(Stream stream)
        {
            try
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var config = new CsvConfiguration
                    {
                        Delimiter = ";"
                    };
                    config.RegisterClassMap<CsvMap>();

                    var csv = new CsvReader(reader, config);

                    var count = 0;
                    foreach (var user in csv.GetRecords<LocalUser>())
                    {
                        if (Add(user) != null)
                        {
                            count++;
                        }
                    }
                    return count;
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
            config.RegisterClassMap<ExportMap>();

            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1000, true))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(All());

                return All().Count();
            }
        }

        public bool Generate(string templateName, string pattern, string domain, int count)
        {
            var parts = templateName.Split(new[] { '/' }, 2);
            var template = _templateProviders
                .SelectMany(t => t.All())
                .First(t => t.GetType().Name.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase)
                            && t.Name.Equals(parts[1], StringComparison.InvariantCultureIgnoreCase));

            foreach (var user in template.Generate(pattern, domain, count))
            {
                if (Add(user) == null) return false;
            }

            return true;
        }

        public IEnumerable<UserTemplate> GetTemplates()
        {
            return _templateProviders
                .SelectMany(t => t.All())
                .Select(t => new UserTemplate(t.GetType().Name + "/" + t.Name, t.DisplayName, t.SupportsPattern))
                .OrderBy(t => t.DisplayName);
        }

        public IEnumerable<string> SearchMailboxes(string search, int max)
        {
            return All()
                    .Select(u => String.Format("{0} {1} <{2}>", u.FirstName, u.LastName, u.Mailbox))
                    .Where(m => CultureInfo.InvariantCulture.CompareInfo.IndexOf(m, search, CompareOptions.IgnoreCase) >= 0)
                    .Take(max);
        }

#if DEBUG
        protected override IEnumerable<LocalUser> Initializer()
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

        protected override IOrderedEnumerable<LocalUser> ApplyOrder(IEnumerable<LocalUser> entities)
        {
            return entities.OrderBy(u => u.Mailbox, StringComparer.InvariantCultureIgnoreCase);
        }

        private class CsvMap : CsvClassMap<LocalUser>
        {
            public CsvMap()
            {
                Map(m => m.Mailbox).Name("Mailbox", "Email Address", "EmailAddress");
                Map(m => m.FirstName).Name("FirstName", "First Name");
                Map(m => m.LastName).Name("LastName", "Last Name");
            }
        }

        private class ExportMap : CsvClassMap<LocalUser>
        {
            public ExportMap()
            {
                Map(m => m.Id).Ignore();
                Map(m => m.FirstName).Name("First Name").Index(0);
                Map(m => m.LastName).Name("Last Name").Index(1);
                Map(m => m.Mailbox).Name("Email Address").Index(2);
            }
        }
    }
}