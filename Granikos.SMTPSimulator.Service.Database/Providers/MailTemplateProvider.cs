using System.ComponentModel.Composition;
using System.Linq;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
{
    [Export(typeof(IMailTemplateProvider))]
    public class MailTemplateProvider : DefaultProvider<MailTemplate, IMailTemplate>, IMailTemplateProvider<MailTemplate>, IMailTemplateProvider
    {

        public MailTemplateProvider()
            : base(ModelHelpers.ConvertTo<MailTemplate>)
        {
        }

        protected override IOrderedQueryable<MailTemplate> ApplyOrder(IQueryable<MailTemplate> entities)
        {
            return entities.OrderBy(u => u.Title.ToLower());
        }
    }
}