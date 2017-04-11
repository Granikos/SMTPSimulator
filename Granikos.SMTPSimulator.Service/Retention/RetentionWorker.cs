using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using log4net;

namespace Granikos.SMTPSimulator.Service.Retention
{
    public class RetentionWorker
    {
        private DirectoryRetentionConfig _config;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(RetentionWorker));

        public RetentionWorker(string dir)
            : this(new DirectoryRetentionConfig { Directory = dir })
        {
        }

        public RetentionWorker(DirectoryRetentionConfig config)
        {
            Contract.Requires<ArgumentNullException>(config != null);
            Contract.Requires<ArgumentException>(Directory.Exists(config.Directory));

            _config = config;
        }

        public void Run()
        {
            Logger.InfoFormat("Retention manager running on '{0}':", _config.Directory);

            long totalSize = 0;
            bool deleting = false;
            var count = 0;
            var minTime = _config.MinTime == TimeSpan.MaxValue ? DateTime.MinValue : DateTime.Now - _config.MinTime;
            var maxTime = _config.MaxTime == TimeSpan.MaxValue ? DateTime.MinValue : DateTime.Now - _config.MaxTime;
            foreach (var fileInfo in Directory.GetFiles(_config.Directory)
                .Select(f => new FileInfo(f))
                .OrderByDescending(i => i.LastWriteTime))
            {
                totalSize += fileInfo.Length;
                count++;

                if (deleting)
                {
                    Logger.InfoFormat("Retention: Deleting '{0}'.", fileInfo.Name);
                    fileInfo.Delete();
                    continue;
                }

                if (count <= _config.MinFiles || fileInfo.LastWriteTime > minTime)
                {
                    Logger.DebugFormat("Retention: Keeping '{0}' because of minimum requirements.", fileInfo.Name);
                    continue;
                }

                if (count > _config.MaxFiles)
                {
                    deleting = true;
                    Logger.Debug("Retention: Maximum number of files exceeded, start deleting.");
                }
                else if (totalSize > _config.MaxSize)
                {
                    deleting = true;
                    Logger.Debug("Retention: Maximum total file size exceeded, start deleting.");
                }
                else if (fileInfo.LastWriteTime < maxTime)
                {
                    deleting = true;
                    Logger.Debug("Retention: Maximum history time span exceeded, start deleting older files.");
                }

                if (deleting)
                {
                    Logger.InfoFormat("Retention: Deleting '{0}'.", fileInfo.Name);
                    fileInfo.Delete();
                }
            }
        }

        public IEnumerable<string> FilesToDelete
        {
            get
            {
                var totalSize = 0L;
                var deleting = false;
                var count = 0;
                var minTime = _config.MinTime == TimeSpan.MaxValue ? DateTime.MinValue : DateTime.Now - _config.MinTime;
                var maxTime = _config.MaxTime == TimeSpan.MaxValue ? DateTime.MinValue : DateTime.Now - _config.MaxTime;

                foreach (var fileInfo in Directory.GetFiles(_config.Directory)
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(i => i.LastWriteTime))
                {
                    totalSize += fileInfo.Length;
                    count++;

                    if (deleting)
                    {
                        yield return fileInfo.Name;
                    }

                    if (count <= _config.MinFiles || fileInfo.LastWriteTime > minTime)
                    {
                        continue;
                    }

                    if (count > _config.MaxFiles || totalSize > _config.MaxSize || fileInfo.LastWriteTime < maxTime)
                    {
                        deleting = true;
                        yield return fileInfo.Name;
                    }
                }
            }
        }

        public DirectoryRetentionConfig Config
        {
            get { return _config; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null);
                _config = value;
            }
        }
    }
}
