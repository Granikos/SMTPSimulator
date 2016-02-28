using System.Collections.Generic;

namespace Granikos.NikosTwo.Service.Models
{
    public interface IUserTemplate
    {
        string Name { get; }
        string DisplayName { get; }
        bool SupportsPattern { get; }
        IEnumerable<IUser> Generate(string pattern, string domain, int count);
    }
}