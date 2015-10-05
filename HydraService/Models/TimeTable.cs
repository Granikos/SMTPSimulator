using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public class TimeTable : IEntity<int>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public string MailContent { get; set; }

        [DataMember]
        public SendConnector SendConnector { get; set; }

        [DataMember]
        public int Id { get; set; }
    }
}