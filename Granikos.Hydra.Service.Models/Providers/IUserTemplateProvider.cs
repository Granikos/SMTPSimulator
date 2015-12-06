using System.Collections.Generic;

namespace Granikos.Hydra.Service.Models.Providers
{
    public interface IUserTemplateProvider
    {
        IEnumerable<IUserTemplate> All();
    }
}