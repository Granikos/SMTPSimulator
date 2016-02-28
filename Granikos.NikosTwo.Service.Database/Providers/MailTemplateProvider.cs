using System.ComponentModel.Composition;
using System.Linq;
using Granikos.NikosTwo.Service.Database.Models;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;

namespace Granikos.NikosTwo.Service.Database.Providers
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