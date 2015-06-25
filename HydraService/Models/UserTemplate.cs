using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public class UserTemplate
    {
        public UserTemplate(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string DisplayName { get; private set; }
        
    }
}