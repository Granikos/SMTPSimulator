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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;
using User = Granikos.SMTPSimulator.Service.ConfigurationService.Models.User;

namespace Granikos.SMTPSimulator.Service.Providers
{
    [Export(typeof (IUserTemplateProvider))]
    public class TechnicalUserTemplates : IUserTemplateProvider
    {
        public IEnumerable<IUserTemplate> All()
        {
            yield return new TechnicalTemplate("english", "Technical (English)", "User", "{0}", "user{0}");
            yield return new TechnicalTemplate("german", "Technical (German)", "Benutzer", "{0}", "benutzer{0}");
        }

        private class TechnicalTemplate : IUserTemplate
        {
            private readonly string _firstNamePattern;
            private readonly string _lastNamePattern;
            private readonly string _mailboxPattern;

            public TechnicalTemplate(string name, string displayName, string firstNamePattern, string lastNamePattern,
                string mailboxPattern)
            {
                _firstNamePattern = firstNamePattern;
                _lastNamePattern = lastNamePattern;
                _mailboxPattern = mailboxPattern;
                Name = name;
                DisplayName = displayName;
            }

            public string Name { get; private set; }
            public string DisplayName { get; private set; }

            public bool SupportsPattern
            {
                get { return false; }
            }

            public IEnumerable<IUser> Generate(string pattern, string domain, int count)
            {
                for (var i = 1; i <= count; i++)
                {
                    yield return new User
                    {
                        FirstName = string.Format(_firstNamePattern, i),
                        LastName = string.Format(_lastNamePattern, i),
                        Mailbox = string.Format(_mailboxPattern, i) + "@" + domain
                    };
                }
            }
        }
    }
}