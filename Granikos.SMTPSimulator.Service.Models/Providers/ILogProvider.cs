using System.IO;

namespace Granikos.SMTPSimulator.Service.Models.Providers
{
    public interface ILogProvider
    {
        string[] FileNames { get; }
        void GetFile(Stream stream, string name);
    }
}