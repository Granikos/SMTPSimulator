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