using System.Collections.Generic;
using HydraService.Models;

namespace HydraService.Providers
{
    public interface IUserTemplate
    {
        string Name { get; }
        string DisplayName { get; }
        bool SupportsPattern { get; }
        IEnumerable<LocalUser> Generate(string pattern, string domain, int count);
    }
}