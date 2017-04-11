using System.Collections.Generic;

namespace Granikos.SMTPSimulator.Service.Models.Providers
{
    public interface IUserTemplateProvider
    {
        IEnumerable<IUserTemplate> All();
    }
}