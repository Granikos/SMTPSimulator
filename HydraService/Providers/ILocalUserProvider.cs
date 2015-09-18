using System.Collections.Generic;
using System.IO;
using HydraService.Models;

namespace HydraService.Providers
{
    public interface ILocalUserProvider : IDataProvider<LocalUser, int>
    {
        int ImportFromCSV(Stream csv, bool overwrite);
        int ExportAsCSV(Stream csv);
        bool Generate(string template, string pattern, string domain, int count);
        IEnumerable<UserTemplate> GetTemplates();
        IEnumerable<string> SearchMailboxes(string search, int max);
    }
}