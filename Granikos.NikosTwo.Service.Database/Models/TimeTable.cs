using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.Database.Models
{
    public class TimeTable : ITimeTable
    {
        private ICollection<TimeTableParameter> _internaParameters = new List<TimeTableParameter>();

        public TimeTable()
        {
            MinRecipients = MaxRecipients = 1;
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public IDictionary<string, string> Parameters
        {
            get { return InternalParameters.ToDictionary(p => p.Name, p => p.Value); }
            set
            {
                InternalParameters = value
                    .Select(entry => new TimeTableParameter { Name = entry.Key, Value = entry.Value })
                    .ToList();
            }
        }

        public virtual ICollection<TimeTableParameter> InternalParameters
        {
            get { return _internaParameters; }
            set { _internaParameters = (value ?? new List<TimeTableParameter>()); }
        }

        public bool Active { get; set; }

        public string RecipientMailbox { get; set; }

        public int RecipientGroupId { get; set; }

        public bool StaticRecipient { get; set; }

        public string SenderMailbox { get; set; }

        public int SenderGroupId { get; set; }

        public bool StaticSender { get; set; }

        [Range(1, 4)]
        public int MinRecipients { get; set; }

        [Range(1, 4)]
        public int MaxRecipients { get; set; }

        public int MailTemplateId { get; set; }

        public SendConnector SendConnector { get; set; }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }

        public string ReportMailAddress { get; set; }

        public ReportType ReportType { get; set; }

        public ProtocolLevel ProtocolLevel { get; set; }

        public string AttachmentName { get; set; }

        public AttachmentType AttachmentType { get; set; }

        public bool SendEicarFile { get; set; }

        public int MailsSuccess { get; set; }

        public int MailsError { get; set; }
        public DateTime? ActiveSince { get; set; }
        public DateTime? ActiveUntil { get; set; }
        [Range(0, 23)]
        public int ReportHour { get; set; }
        public DayOfWeek ReportDay { get; set; }

        public static TimeTable FromOther(ITimeTable source)
        {
            var target = new TimeTable();

            source.CopyTo(target);

            return target;
        }
    }

    public class TimeTableParameter
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("TimeTableId")]
        public virtual TimeTable TimeTable { get; set; }

        [Index("KeyIndex", 2, IsUnique = true)]
        public int TimeTableId { get; set; }

        [Index("KeyIndex", 1, IsUnique = true)]
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }
    }
}