using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
{
    [DataContract]
    public class TimeTable : IEntity<int>
    {
        public TimeTable()
        {
            MinRecipients = MaxRecipients = 1;
        }

        [DataMember]
        [Required]
        public string Name { get; set; }

        [DataMember]
        [Required]
        public IDictionary<string, string> Parameters { get; set; }

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public string RecipientMailbox { get; set; }

        [DataMember]
        public int? RecipientGroupId { get; set; }

        [DataMember]
        public bool StaticRecipient { get; set; }

        [DataMember]
        public string SenderMailbox { get; set; }

        [DataMember]
        public int? SenderGroupId { get; set; }

        [DataMember]
        public bool StaticSender { get; set; }

        [DataMember]
        [Range(1, 4)]
        public int MinRecipients { get; set; }

        [DataMember]
        [Range(1,4)]
        public int MaxRecipients { get; set; }

        [DataMember]
        public string MailContent { get; set; }

        [DataMember]
        public SendConnector SendConnector { get; set; }

        [DataMember]
        [Required]
        public int Id { get; set; }

        [DataMember]
        [Required]
        public string Type { get; set; }

        [DataMember]
        [Required]
        public string MailType { get; set; }

        [DataMember]
        public string ReportMailAddress { get; set; }

        [DataMember]
        public ReportType ReportType { get; set; }

        [DataMember]
        public ProtocolLevel ProtocolLevel { get; set; }

        [DataMember]
        public string Attachment { get; set; }

        [DataMember]
        public AttachmentType AttachmentType { get; set; }

        [DataMember]
        public bool SendEicarFile { get; set; }
    }

    public enum ReportType
    {
        Off, Daily, Weekly
    }

    public enum ProtocolLevel
    {
        Off, On, Verbose
    }

    public enum AttachmentType
    {
        Off, Fixed, Random
    }
}