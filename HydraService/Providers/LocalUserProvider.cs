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
    [Export(typeof(ILocalUserProvider))]
    public class LocalUserProvider : DefaultProvider<LocalUser>, ILocalUserProvider
    {
        public LocalUserProvider()
        {
            Add(new LocalUser
                {
                    FirstName = "Bernd",
                    LastName = "Müller",
                    Mailbox = "bernd.mueller@test.de"
                });
            Add(new LocalUser
                {
                    FirstName = "Eva",
                    LastName = "Schmidt",
                    Mailbox = "eva.schmidt@test.de"
                });
        }

        class CsvMap : CsvClassMap<LocalUser>
        {
            public CsvMap()
            {
                Map(m => m.Mailbox).Name("Mailbox", "Email Address", "EmailAddress");
                Map(m => m.FirstName).Name("FirstName", "First Name");
                Map(m => m.LastName).Name("LastName", "Last Name");
            }
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

        class ExportMap : CsvClassMap<LocalUser>
        {
            public ExportMap()
            {
                Map(m => m.Id).Ignore();
                Map(m => m.FirstName).Name("First Name").Index(0);
                Map(m => m.LastName).Name("Last Name").Index(1);
                Map(m => m.Mailbox).Name("Email Address").Index(2);
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

        public bool Generate(string template, string pattern, int count)
        {
            throw new NotImplementedException();
        }
    }
}