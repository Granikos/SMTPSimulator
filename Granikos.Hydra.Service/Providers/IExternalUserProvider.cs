using System;
using System.Collections.Generic;
using System.IO;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    public interface IExternalUserProvider : IDataProvider<User, int>
    {
        int ImportFromCSV(Stream csv, bool overwrite);
        int ExportAsCSV(Stream csv);
        IEnumerable<string> SearchMailboxes(string search, int max);
        IEnumerable<User> GetByDomain(string domain);
        IEnumerable<ValueWithCount<string>> SearchDomains(string domain);
    }
}