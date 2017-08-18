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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.ConfigurationService.Models
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

        [DataMember]
        public int ReportHour { get; set; }
        [DataMember]
        public DayOfWeek ReportDay { get; set; }
    }
}