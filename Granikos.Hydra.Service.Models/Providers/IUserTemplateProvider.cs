using System.Collections.Generic;

namespace Granikos.NikosTwo.Service.Models.Providers
{
    public interface IUserTemplateProvider
    {
        IEnumerable<IUserTemplate> All();
    }
}