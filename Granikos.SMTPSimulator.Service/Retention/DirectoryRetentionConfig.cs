using System;

namespace Granikos.SMTPSimulator.Service.Retention
{
    public class DirectoryRetentionConfig
    {
        private int _maxFiles = int.MaxValue;
        private long _maxSize = long.MaxValue;
        private TimeSpan _maxTime = TimeSpan.MaxValue;
        private int _minFiles = 1;
        private TimeSpan _minTime = TimeSpan.Zero;

        public string Directory { get; set; }

        public int MinFiles
        {
            get { return _minFiles; }
            set
            {
                if (!(value >= 1)) throw new ArgumentOutOfRangeException();
                if (!(value <= MaxFiles)) throw new ArgumentOutOfRangeException();

                _minFiles = value;
            }
        }

        public int MaxFiles
        {
            get { return _maxFiles; }
            set
            {
                if (!(value >= MinFiles)) throw new ArgumentOutOfRangeException();

                _maxFiles = value;
            }
        }

        public long MaxSize
        {
            get { return _maxSize; }
            set
            {
                if (!(value >= 1)) throw new ArgumentOutOfRangeException();

                _maxSize = value;
            }
        }

        public TimeSpan MaxTime
        {
            get { return _maxTime; }
            set
            {
                if (!(value >= MinTime)) throw new ArgumentOutOfRangeException();

                _maxTime = value;
            }
        }

        public TimeSpan MinTime
        {
            get { return _minTime; }
            set
            {
                if (!(value >= TimeSpan.Zero)) throw new ArgumentOutOfRangeException();
                if (!(value <= MaxTime)) throw new ArgumentOutOfRangeException();

                _minTime = value;
            }
        }

        public DirectoryRetentionConfig Copy()
        {
            return (DirectoryRetentionConfig)MemberwiseClone();
        }
    }
}