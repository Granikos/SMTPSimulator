using System.Collections.Generic;

namespace Granikos.Hydra.Service.Models.Providers
{
    public interface IMailTemplateProvider<TTemplate> : IDataProvider<TTemplate, int>
        where TTemplate : IMailTemplate
    {
    }

    public interface IMailTemplateProvider : IMailTemplateProvider<IMailTemplate>
    {
    }
}