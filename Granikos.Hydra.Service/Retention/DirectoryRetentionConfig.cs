using System;
using System.Diagnostics.Contracts;

namespace Granikos.Hydra.Service.Retention
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
                Contract.Requires<ArgumentOutOfRangeException>(value >= 1);
                Contract.Requires<ArgumentOutOfRangeException>(value <= MaxFiles);

                _minFiles = value;
            }
        }

        public int MaxFiles
        {
            get { return _maxFiles; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value >= MinFiles);

                _maxFiles = value;
            }
        }

        public long MaxSize
        {
            get { return _maxSize; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value >= 1);

                _maxSize = value;
            }
        }

        public TimeSpan MaxTime
        {
            get { return _maxTime; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value >= MinTime);

                _maxTime = value;
            }
        }

        public TimeSpan MinTime
        {
            get { return _minTime; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value >= TimeSpan.Zero);
                Contract.Requires<ArgumentOutOfRangeException>(value <= MaxTime);

                _minTime = value;
            }
        }

        public DirectoryRetentionConfig Copy()
        {
            return (DirectoryRetentionConfig)MemberwiseClone();
        }
    }
}