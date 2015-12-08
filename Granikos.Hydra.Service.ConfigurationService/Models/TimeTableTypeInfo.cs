using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.ConfigurationService.Models
{
    [DataContract]
    public class TimeTableTypeInfo
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }
    }
}