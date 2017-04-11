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