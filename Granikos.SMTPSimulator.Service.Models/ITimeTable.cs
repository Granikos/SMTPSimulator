using System;
using System.Collections.Generic;

namespace Granikos.SMTPSimulator.Service.Models
{
    public interface ITimeTable : IEntity<int>
    {
        string Name { get; set; }
        IDictionary<string, string> Parameters { get; set; }
        bool Active { get; set; }
        string RecipientMailbox { get; set; }
        int RecipientGroupId { get; set; }
        bool StaticRecipient { get; set; }
        string SenderMailbox { get; set; }
        int SenderGroupId { get; set; }
        bool StaticSender { get; set; }
        int MinRecipients { get; set; }
        int MaxRecipients { get; set; }
        int MailTemplateId { get; set; }
        string Type { get; set; }
        string ReportMailAddress { get; set; }
        ReportType ReportType { get; set; }
        ProtocolLevel ProtocolLevel { get; set; }
        string AttachmentName { get; set; }
        AttachmentType AttachmentType { get; set; }
        bool SendEicarFile { get; set; }
        int MailsSuccess { get; set; }
        int MailsError { get; set; }

        DateTime? ActiveSince { get; set; }
        DateTime? ActiveUntil { get; set; }

        int ReportHour { get; set; }
        DayOfWeek ReportDay { get; set; }
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