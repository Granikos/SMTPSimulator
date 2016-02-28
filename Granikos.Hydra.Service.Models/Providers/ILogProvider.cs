using System.IO;

namespace Granikos.NikosTwo.Service.Models.Providers
{
    public interface ILogProvider
    {
        string[] FileNames { get; }
        void GetFile(Stream stream, string name);
    }
}