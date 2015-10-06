using System.Collections.Generic;

namespace Granikos.Hydra.Service.Providers
{
    public interface IUserTemplateProvider
    {
        IEnumerable<IUserTemplate> All();
    }
}