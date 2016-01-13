using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Granikos.Hydra.Service.Retention
{
    public class RetentionManager
    {
        private DirectoryRetentionConfig _config;

        public RetentionManager(string dir)
        {
            Contract.Requires<ArgumentNullException>(dir != null);
            Contract.Requires<ArgumentException>(Directory.Exists(dir));

            _config = new DirectoryRetentionConfig
            {
                Directory = dir
            };
        }

        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public void Run()
        {
            long totalSize = 0;
            bool deleting = false;
            var count = 0;
            var minTime = _config.MinTime == TimeSpan.MaxValue? DateTime.MinValue : DateTime.Now - _config.MinTime;
            var maxTime = _config.MaxTime == TimeSpan.MaxValue? DateTime.MinValue : DateTime.Now - _config.MaxTime;
            foreach (var fileInfo in Directory.GetFiles(_config.Directory)
                .Select(f => new FileInfo(f))
                .OrderByDescending(i => i.LastWriteTime))
            {
                totalSize += fileInfo.Length;
                count++;

                Console.WriteLine("{0}\t{1}\t{2}\t{3}", fileInfo.Name, BytesToString(fileInfo.Length), BytesToString(totalSize), fileInfo.LastWriteTime);

                if (deleting)
                {
                    Console.WriteLine("deleting");
                    continue;
                }

                if (count <= _config.MinFiles || fileInfo.LastWriteTime > minTime)
                {
                    Console.WriteLine("keeping because of mins");
                    continue;
                }

                if (count > _config.MaxFiles)
                {
                    deleting = true;
                    Console.WriteLine("Max files reached, start deleting");
                    continue;
                }

                if (totalSize > _config.MaxSize)
                {
                    deleting = true;
                    Console.WriteLine("Max size reached, start deleting");
                    continue;
                }

                if (fileInfo.LastWriteTime < maxTime)
                {
                    deleting = true;
                    Console.WriteLine("Max time reached, start deleting");
                    continue;
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
