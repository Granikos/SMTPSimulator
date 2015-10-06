using System.Collections.Generic;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    public interface IUserTemplate
    {
        string Name { get; }
        string DisplayName { get; }
        bool SupportsPattern { get; }
        IEnumerable<LocalUser> Generate(string pattern, string domain, int count);
    }
}