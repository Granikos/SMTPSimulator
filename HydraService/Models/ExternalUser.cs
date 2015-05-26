using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public class ExternalUser : IEntity
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Mailbox { get; set; }
    }
}