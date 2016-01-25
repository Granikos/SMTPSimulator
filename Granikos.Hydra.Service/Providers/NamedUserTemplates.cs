using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Granikos.Hydra.Service.ConfigurationService.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;
using Newtonsoft.Json;

namespace Granikos.Hydra.Service.Providers
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