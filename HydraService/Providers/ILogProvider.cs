using System.IO;

namespace HydraService.Providers
{
    public interface ILogProvider
    {
        string[] FileNames { get; }

        void GetFile(Stream stream, string name);
    }
}