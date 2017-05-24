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
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service
{
    class UserImporter
    {
        private readonly IDataProvider<IUser, int> _users;

        public UserImporter(IDataProvider<IUser, int> users)
        {
            if (users == null) throw new ArgumentNullException("users");

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
