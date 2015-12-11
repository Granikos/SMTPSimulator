using System.ComponentModel.Composition;
using System.Linq;
using Granikos.Hydra.Service.Database.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Database.Providers
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