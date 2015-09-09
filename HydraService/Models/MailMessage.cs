using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public class MailMessage
    {
        [DataMember]
        public int? ConnectorId { get; set; }

        [DataMember]
        public string Sender { get; set; }

        [DataMember]
        public string[] Recipients { get; set; }

        [DataMember]
        public string Content { get; set; }
    }
}