using System.Runtime.Serialization;

namespace Granikos.SMTPSimulator.Service.Models
{
    [DataContract]
    public class UserTemplate
    {
        public UserTemplate(string name, string displayName, bool supportsPattern = false)
        {
            Name = name;
            DisplayName = displayName;
            SupportsPattern = supportsPattern;
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string DisplayName { get; private set; }

        [DataMember]
        public bool SupportsPattern { get; private set; }
    }
}