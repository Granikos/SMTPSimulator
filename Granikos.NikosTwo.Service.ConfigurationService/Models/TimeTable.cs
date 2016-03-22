using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class TimeTable : ITimeTable
    {
        [DataMember]
        public int Id { get; set; }
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
        public int RecipientGroupId { get; set; }
        [DataMember]
        public bool StaticRecipient { get; set; }
        [DataMember]
        public string SenderMailbox { get; set; }
        [DataMember]
        public int SenderGroupId { get; set; }
        [DataMember]
        public bool StaticSender { get; set; }
        [DataMember]
        [Range(1, 4)]
        public int MinRecipients { get; set; }
        [DataMember]
        [Range(1, 4)]
        public int MaxRecipients { get; set; }
        [DataMember]
        public int MailTemplateId { get; set; }
        [DataMember]
        [Required]
        public string Type { get; set; }
        [DataMember]
        public string ReportMailAddress { get; set; }
        [DataMember]
        public ReportType ReportType { get; set; }
        [DataMember]
        public ProtocolLevel ProtocolLevel { get; set; }
        [DataMember]
        public string AttachmentName { get; set; }
        [DataMember]
        public AttachmentType AttachmentType { get; set; }
        [DataMember]
        public bool SendEicarFile { get; set; }
        [DataMember]
        public int MailsSuccess { get; set; }
        [DataMember]
        public int MailsError { get; set; }

        [DataMember]
        public DateTime? ActiveSince { get; set; }
        [DataMember]
        public DateTime? ActiveUntil { get; set; }
    }
}