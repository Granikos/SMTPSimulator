using System.Collections.Generic;

namespace Granikos.Hydra.Service.Providers
{
    public interface ITimeTableType
    {
        string DisplayName { get; }

        IDictionary<string, string> Parameters { get; set; }

        string[] Data { get; set; }
    }
}