// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
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
            if (config == null) throw new ArgumentNullException();
            if (!(Directory.Exists(config.Directory))) throw new ArgumentException();

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
                if (value == null) throw new ArgumentNullException();
                _config = value;
            }
        }
    }
}
