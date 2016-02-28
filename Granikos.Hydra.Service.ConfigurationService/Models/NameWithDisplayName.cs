using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.ConfigurationService.Models
{
    [DataContract]
    public class NameWithDisplayName
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }
    }
}