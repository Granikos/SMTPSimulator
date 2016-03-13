using System.Runtime.Serialization;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class MailMessage
    {
        [DataMember]
        public SendConnector Connector { get; set; }

        [DataMember]
        public string Sender { get; set; }

        [DataMember]
        public string[] Recipients { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string Html { get; set; }
    }
}