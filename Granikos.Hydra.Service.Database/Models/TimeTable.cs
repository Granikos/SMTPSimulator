using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Database.Models
{
    public class TimeTable : ITimeTable
    {
        private int _mailsSuccess;
        private int _mailsError;

        public TimeTable()
        {
            MinRecipients = MaxRecipients = 1;
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public IDictionary<string, string> Parameters { get; set; }

        public bool Active { get; set; }

        public string RecipientMailbox { get; set; }

        public int? RecipientGroupId { get; set; }

        public bool StaticRecipient { get; set; }

        public string SenderMailbox { get; set; }

        public int? SenderGroupId { get; set; }

        public bool StaticSender { get; set; }

        [Range(1, 4)]
        public int MinRecipients { get; set; }

        [Range(1,4)]
        public int MaxRecipients { get; set; }

        public int MailTemplateId { get; set; }

        public SendConnector SendConnector { get; set; }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string MailType { get; set; }

        public string ReportMailAddress { get; set; }

        public ReportType ReportType { get; set; }

        public ProtocolLevel ProtocolLevel { get; set; }

        public int AttachmentId { get; set; }

        public AttachmentType AttachmentType { get; set; }

        public bool SendEicarFile { get; set; }

        public int MailsSuccess
        {
            get { return _mailsSuccess; }
            set {  }
        }

        public int MailsError
        {
            get { return _mailsError; }
            set { }
        }

        public static TimeTable FromOther(ITimeTable source)
        {
            var target = new TimeTable();

            source.CopyTo(target);

            return target;
        }
    }
}