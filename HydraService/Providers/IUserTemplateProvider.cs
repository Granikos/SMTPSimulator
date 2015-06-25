using System.Collections.Generic;

namespace HydraService.Providers
{
    public interface IUserTemplateProvider
    {
        IEnumerable<IUserTemplate> All();
    }
}