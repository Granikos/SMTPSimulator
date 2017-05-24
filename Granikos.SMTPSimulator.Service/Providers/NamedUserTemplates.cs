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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;
using Newtonsoft.Json;

namespace Granikos.SMTPSimulator.Service.Providers
{
    [Export(typeof (IUserTemplateProvider))]
    public class NamedUserTemplates : IUserTemplateProvider
    {
        private string TemplateFolder
        {
            get
            {
                var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var logFolder = ConfigurationManager.AppSettings["UserTemplates"];
                return Path.Combine(folder, logFolder);
            }
        }

        public IEnumerable<IUserTemplate> All()
        {
            return Directory.GetFiles(TemplateFolder, "*.json")
                .Select(p => new NamedTemplate(Path.GetFileNameWithoutExtension(p), p));
        }

        private class NameData
        {
            public string DisplayName { get; set; }
            public string[] FirstNames { get; set; }
            public string[] LastNames { get; set; }
        }

        private class NamePattern
        {
            private static readonly Regex FirstNameRegex = new Regex(@"%(\d*)g", RegexOptions.Compiled);
            private static readonly Regex LastNameRegex = new Regex(@"%(\d*)s", RegexOptions.Compiled);
            private static readonly Regex CharReplaceRegex = new Regex(@"%r(.)([^ ])(.*)$", RegexOptions.Compiled);
            private readonly string _pattern;

            public NamePattern(string pattern)
            {
                _pattern = pattern;
            }

            public string Format(string firstName, string lastName)
            {
                var result = _pattern;

                result = FirstNameRegex.Replace(result, match =>
                {
                    if (!string.IsNullOrEmpty(match.Groups[1].Value))
                    {
                        var size = int.Parse(match.Groups[1].Value);
                        return firstName.Substring(0, size);
                    }

                    return firstName;
                });

                result = LastNameRegex.Replace(result, match =>
                {
                    if (!string.IsNullOrEmpty(match.Groups[1].Value))
                    {
                        var size = int.Parse(match.Groups[1].Value);
                        return lastName.Substring(0, size);
                    }

                    return lastName;
                });

                result = CharReplaceRegex.Replace(result,
                    match =>
                    {
                        return match.Groups[3].Value.Replace(match.Groups[1].Value[0], match.Groups[2].Value[0]);
                    });

                return result;
            }
        }

        private class NamedTemplate : IUserTemplate
        {
            private readonly NameData _nameData;

            public NamedTemplate(string name, string templateFile)
            {
                Name = name;

                using (var stream = File.OpenRead(templateFile))
                using (var reader = new StreamReader(stream))
                {
                    _nameData = JsonConvert.DeserializeObject<NameData>(reader.ReadToEnd());
                }
                ;
            }

            public string Name { get; private set; }

            public string DisplayName
            {
                get { return _nameData.DisplayName; }
            }

            public bool SupportsPattern
            {
                get { return true; }
            }

            public IEnumerable<IUser> Generate(string pattern, string domain, int count)
            {
                var boxes = new HashSet<string>();

                var random = new Random();
                for (var i = 1; i <= count; i++)
                {
                    string fn, ln, mb;

                    do
                    {
                        fn = _nameData.FirstNames[random.Next(_nameData.FirstNames.Length)];
                        ln = _nameData.LastNames[random.Next(_nameData.LastNames.Length)];
                        mb = new NamePattern(pattern).Format(fn, ln);
                    } while (boxes.Contains(mb));

                    boxes.Add(mb);

                    yield return new User
                    {
                        FirstName = fn,
                        LastName = ln,
                        Mailbox = mb + "@" + domain
                    };
                }
            }
        }
    }
}