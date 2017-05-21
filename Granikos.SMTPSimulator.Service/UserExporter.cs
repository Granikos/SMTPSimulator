using System;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service
{
    class UserExporter
    {
        private readonly IDataProvider<IUser, int> _users;

        public UserExporter(IDataProvider<IUser, int> users)
        {
            if (users == null) throw new ArgumentNullException("users");

            _users = users;
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
                csv.WriteRecords(_users.All().Select(u => u.ConvertTo<User>()));

                return _users.Total;
            }
        }

        private class CsvMap : CsvClassMap<User>
        {
            public CsvMap()
            {
                Map(m => m.Id).Ignore();
                Map(m => m.FirstName).Name("First Name").Index(0);
                Map(m => m.LastName).Name("Last Name").Index(1);
                Map(m => m.Mailbox).Name("Email Address").Index(2);
            }
        }
    }
}