using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(IUserTemplateProvider))]
    public class TechnicalUserTemplates : IUserTemplateProvider
    {
        class TechnicalTemplate : IUserTemplate
        {
            private readonly string _firstNamePattern;
            private readonly string _lastNamePattern;
            private readonly string _mailboxPattern;

            public TechnicalTemplate(string name, string displayName, string firstNamePattern, string lastNamePattern, string mailboxPattern)
            {
                _firstNamePattern = firstNamePattern;
                _lastNamePattern = lastNamePattern;
                _mailboxPattern = mailboxPattern;
                Name = name;
                DisplayName = displayName;
            }

            public string Name { get; private set; }

            public string DisplayName { get; private set; }

            public bool SupportsPattern { get { return false; } }

            public IEnumerable<LocalUser> Generate(string pattern, string domain, int count)
            {
                for (var i = 1; i <= count; i++)
                {
                    yield return new LocalUser
                    {
                        FirstName = String.Format(_firstNamePattern, i),
                        LastName = String.Format(_lastNamePattern, i),
                        Mailbox = String.Format(_mailboxPattern, i) + "@" + domain,
                    };
                }
            }
        }

        public IEnumerable<IUserTemplate> All()
        {
            yield return new TechnicalTemplate("english", "Technical (English)", "User", "{0}", "user{0}");
            yield return new TechnicalTemplate("german", "Technical (German)", "Benutzer", "{0}", "benutzer{0}");
        }
    }
}