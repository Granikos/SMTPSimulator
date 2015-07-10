using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(IExternalUserProvider))]
    public class ExternalUserProvider : DefaultProvider<ExternalUser>, IExternalUserProvider
    {
        public ExternalUserProvider()
        {
            Add(new ExternalUser
            {
                Mailbox = "bernd.mueller",
                DomainId = 1
            });
            Add(new ExternalUser
            {
                Mailbox = "max.muetze",
                DomainId = 1
            });
            Add(new ExternalUser
            {
                Mailbox = "manuel.krebber",
                DomainId = 2
            });
        }

        protected override IOrderedEnumerable<ExternalUser> ApplyOrder(IEnumerable<ExternalUser> entities)
        {
            return entities.OrderBy(u => u.Mailbox, StringComparer.InvariantCultureIgnoreCase);
        }

        class CsvMap : CsvClassMap<ExternalUser>
        {
            public CsvMap()
            {
                Map(m => m.Mailbox).Name("Mailbox", "Email Address", "EmailAddress");
                Map(m => m.FirstName).Name("FirstName", "First Name");
                Map(m => m.LastName).Name("LastName", "Last Name");
            }
        }

        public int ImportFromCSV(Stream stream, Func<string,int> domainSource)
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
                    foreach (var user in csv.GetRecords<ExternalUser>())
                    {
                        var parts = user.Mailbox.Split(new[] { '@' }, 2);
                        user.Mailbox = parts[0];
                        user.DomainId = domainSource(parts[1]);

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

        class UserProxy : ExternalUser
        {
            public string Domain { get; set; }

            public string Address { get { return String.Format("{0}@{1}", Mailbox, Domain); } }
        }

        class ProxyMap : CsvClassMap<UserProxy>
        {
            public ProxyMap()
            {
                Map(m => m.Mailbox).Ignore();
                Map(m => m.Domain).Ignore();
                Map(m => m.Id).Ignore();
                Map(m => m.FirstName).Name("First Name").Index(0);
                Map(m => m.LastName).Name("Last Name").Index(1);
                Map(m => m.Address).Name("Email Address").Index(2);
            }
        }

        public int ExportAsCSV(Stream stream, Func<int, string> domainSource)
        {
            var config = new CsvConfiguration
            {
                Delimiter = ";"
            };
            config.RegisterClassMap<ProxyMap>();

            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1000, true))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(All().Select(u => new UserProxy
                {
                    Domain = domainSource(u.DomainId),
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Mailbox = u.Mailbox,
                }));

                return All().Count();
            }
        }
    }
}