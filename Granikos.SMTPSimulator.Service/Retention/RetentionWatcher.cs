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
using System.IO;

namespace Granikos.SMTPSimulator.Service.Retention
{
    public class RetentionWatcher
    {
        private readonly FileSystemWatcher _watcher;
        private readonly RetentionWorker _worker;
        private bool _running = false;

        public DirectoryRetentionConfig Config
        {
            get { return _worker.Config; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _worker.Config = value;
            }
        }

        public RetentionWatcher(DirectoryRetentionConfig config)
        {
            if (config == null) throw new ArgumentNullException();
            if (!(Directory.Exists(config.Directory))) throw new ArgumentException();

            _watcher = new FileSystemWatcher(config.Directory);

            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;

            _watcher.EnableRaisingEvents = true;

            _worker = new RetentionWorker(config);
        }

        private void OnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            if (_running) return;

            Run();
        }

        public void Run()
        {
            lock (_worker)
            {
                _running = true;
                var re = _watcher.EnableRaisingEvents;
                _watcher.EnableRaisingEvents = false;
                try
                {
                    _worker.Run();
                }
                finally
                {
                    _watcher.EnableRaisingEvents = re;
                    _running = false;
                }
            }
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
        }
    }
}