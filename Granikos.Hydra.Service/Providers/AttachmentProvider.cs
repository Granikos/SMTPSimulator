using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Providers
{
    [Export(typeof(IMailTemplateProvider))]
    public class MailTemplateProvider : IMailTemplateProvider
    {
        public IEnumerable<IMailTemplate> GetMailTemplates()
        {
            throw new NotImplementedException();
        }
    }
}
