using System.Collections.Generic;

namespace Granikos.Hydra.Service.Models.Providers
{
    public interface IMailTemplateProvider
    {
        IEnumerable<IMailTemplate> GetMailTemplates();
    }
}