using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Granikos.NikosTwo.Service.ConfigurationService.Models;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;

namespace Granikos.NikosTwo.Service
{
    class UserImporter
    {
        private readonly IDataProvider<IUser, int> _users;

        public UserImporter(IDataProvider<IUser, int> users)
        {
            Contract.Requires<ArgumentNullException>(users != null, "users");

            _users = users;
        }

        public int ImportFromCSV(Stream stream, bool overwrite)
        {
            try
            {
                var users = new List<User>();

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
                        _users.Clear();
                    }

                    var count = 0;
                    foreach (var user in records)
                    {
                        if (_users.Add(user) != null)
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

        private class CsvMap : CsvClassMap<User>
        {
            public CsvMap()
            {
                Map(m => m.Mailbox).Name("Mailbox", "Email Address", "EmailAddress");
                Map(m => m.FirstName).Name("FirstName", "First Name");
                Map(m => m.LastName).Name("LastName", "Last Name");
            }
        }
    }
}
