// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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