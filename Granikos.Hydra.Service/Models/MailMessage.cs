using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
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