using System.Collections.Generic;

namespace Granikos.SMTPSimulator.Service.Models.Providers
{
    public interface IMailTemplateProvider<TTemplate> : IDataProvider<TTemplate, int>
        where TTemplate : IMailTemplate
    {
    }

    public interface IMailTemplateProvider : IMailTemplateProvider<IMailTemplate>
    {
    }
}