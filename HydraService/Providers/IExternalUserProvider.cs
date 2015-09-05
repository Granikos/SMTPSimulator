using System;
using System.IO;
using HydraService.Models;

namespace HydraService.Providers
{
    public interface IExternalUserProvider : IDataProvider<ExternalUser, int>
    {
        int ImportFromCSV(Stream csv, Func<string, int> domainSource);
        int ExportAsCSV(Stream csv, Func<int, string> domainSource);
    }
}