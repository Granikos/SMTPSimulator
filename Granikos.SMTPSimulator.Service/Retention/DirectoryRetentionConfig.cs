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