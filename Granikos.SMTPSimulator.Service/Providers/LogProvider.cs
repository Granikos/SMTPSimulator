using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Providers
{
    [Export(typeof (ILogProvider))]
    public class FileLogProvider : ILogProvider
    {
        private string LogFolder
        {
            get
            {
                var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var logFolder = ConfigurationManager.AppSettings["LogFolder"];
                return Path.Combine(folder, logFolder);
            }
        }

        public string[] FileNames
        {
            get
            {
                var length = Path.GetFullPath(LogFolder).Length + 1;

                return Directory.GetFiles(LogFolder, "*", SearchOption.AllDirectories)
                    .Select(p => p.Substring(length))
                    .ToArray();
            }
        }

        public void GetFile(Stream stream, string name)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!(stream.CanWrite)) throw new ArgumentException();
            if (name == null) throw new ArgumentNullException("name");
            if (Path.IsPathRooted(name)) throw new ArgumentException();
            if (name.Contains("..")) throw new ArgumentException();

            var logFile = Path.Combine(LogFolder, name);

            using (var logStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                logStream.CopyTo(stream);
            }
        }
    }
}