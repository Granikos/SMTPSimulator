using System.Configuration;

namespace Granikos.SMTPSimulator.Service.Retention
{
    public class RetentionConfig : ConfigurationSection
    {
        public static RetentionConfig GetConfig()
        {
            Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var s = ConfigurationManager.GetSection("RetentionManager");
            return (RetentionConfig)ConfigurationManager.GetSection("RetentionManager") ?? new RetentionConfig();
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof (DirectoryRetentionConfig), AddItemName = "Directory")]
        public DirectoryRetentionConfigCollection Directories
        {
            get { return (DirectoryRetentionConfigCollection)this[""]; }
        }

    }
}