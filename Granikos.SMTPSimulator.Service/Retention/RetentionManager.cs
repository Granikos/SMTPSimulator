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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using log4net.Appender;

namespace Granikos.SMTPSimulator.Service.Retention
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