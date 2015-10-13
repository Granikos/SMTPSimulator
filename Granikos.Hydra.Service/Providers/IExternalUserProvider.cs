using System;
using System.Collections.Generic;
using System.IO;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    public interface IExternalUserProvider : IDataProvider<ExternalUser, int>
    {
        int ImportFromCSV(Stream csv, Func<string, int> domainSource, bool overwrite);
        int ExportAsCSV(Stream csv, Func<int, string> domainSource);
        IEnumerable<string> SearchMailboxes(Func<int, string> domainSource, string search, int max);
    }
}