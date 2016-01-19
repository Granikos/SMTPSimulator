using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using log4net.Appender;

namespace Granikos.Hydra.Service.Retention
{
    public class RetentionManager
    {
        public const string SpecialDirectoryLog4Net = "$log4net$";

        private readonly RetentionWatcher[] _watchers;

        public RetentionManager()
        {
            var config = RetentionConfig.GetConfig();

            _watchers = config.Directories
                .Cast<DirectoryRetentionConfigElement>()
                .Select(d => d.GetConfig())
                .SelectMany(GetDirConfigs)
                .GroupBy(d => d.Directory.ToCanonicalPath())
                .Select(d => d.Last())
                .Select(d => new RetentionWatcher(d))
                .ToArray();

            new Thread(Run).Start();
        }

        private IEnumerable<DirectoryRetentionConfig> GetDirConfigs(DirectoryRetentionConfig original)
        {
            if (original.Directory.Equals(SpecialDirectoryLog4Net))
            {
                foreach (var logDir in LogManager.GetAllRepositories()
                    .SelectMany(r => r.GetAppenders())
                    .OfType<RollingFileAppender>()
                    .Select(a => Path.GetDirectoryName(a.File)))
                {
                    var newConfig = original.Copy();
                    newConfig.Directory = logDir;
                    yield return newConfig;
                }
            }
            else
            {
                yield return original;
            }
        }

        public void Run()
        {
            foreach (var retentionWatcher in _watchers)
            {
                retentionWatcher.Run();
            }
        }

        public void Start()
        {
            foreach (var retentionWatcher in _watchers)
            {
                retentionWatcher.Start();
            }
        }

        public void Stop()
        {
            foreach (var retentionWatcher in _watchers)
            {
                retentionWatcher.Stop();
            }
        }
    }
}