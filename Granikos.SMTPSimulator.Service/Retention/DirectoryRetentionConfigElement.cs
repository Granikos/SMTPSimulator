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
using System.Configuration;

namespace Granikos.SMTPSimulator.Service.Retention
{
    public class DirectoryRetentionConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Directory
        {
            get { return (string)base["name"]; }
        }

        [ConfigurationProperty("minFiles", IsRequired = false, DefaultValue = 1)]
        public int MinFiles
        {
            get { return (int)base["minFiles"]; }
        }

        [ConfigurationProperty("maxFiles", IsRequired = false, DefaultValue = int.MaxValue)]
        public int MaxFiles
        {
            get { return (int)base["maxFiles"]; }
        }

        [ConfigurationProperty("maxSize", IsRequired = false, DefaultValue = long.MaxValue)]
        public long MaxSize
        {
            get { return (long)base["maxSize"]; }
        }

        [ConfigurationProperty("maxTime", IsRequired = false)]
        public TimeSpan? MaxTime
        {
            get { return (base["maxTime"] as TimeSpan?) ?? TimeSpan.MaxValue; }
        }

        [ConfigurationProperty("minTime", IsRequired = false)]
        public TimeSpan? MinTime
        {
            get { return (base["minTime"] as TimeSpan?) ?? TimeSpan.Zero; }
        }

        public DirectoryRetentionConfig GetConfig()
        {
            return new DirectoryRetentionConfig
            {
                Directory = Directory,
                MaxSize = MaxSize,
                MinTime = MinTime.Value,
                MaxFiles = MaxFiles,
                MinFiles = MinFiles,
                MaxTime = MaxTime.Value
            };
        }
    }
}