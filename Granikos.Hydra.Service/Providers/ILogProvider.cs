using System.IO;

namespace Granikos.Hydra.Service.Providers
{
    public interface ILogProvider
    {
        string[] FileNames { get; }
        void GetFile(Stream stream, string name);
    }
}