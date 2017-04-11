using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(stream != null, "stream");
            Contract.Requires<ArgumentException>(stream.CanWrite);
            Contract.Requires<ArgumentNullException>(name != null, "name");
            Contract.Requires<ArgumentException>(!Path.IsPathRooted(name));
            Contract.Requires<ArgumentException>(!name.Contains(".."));

            var logFile = Path.Combine(LogFolder, name);

            using (var logStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                logStream.CopyTo(stream);
            }
        }
    }
}