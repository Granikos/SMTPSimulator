using System.IO;
using HydraService.Models;

namespace HydraService.Providers
{
    public interface ILocalUserProvider : IDataProvider<LocalUser,int>
    {
        int ImportFromCSV(Stream csv);

        int ExportAsCSV(Stream csv);

        bool Generate(string template, string pattern, int count);
    }
}