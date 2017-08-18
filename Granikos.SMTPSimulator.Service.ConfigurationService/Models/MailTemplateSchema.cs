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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.ConfigurationService.Models
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    [XmlRoot(Namespace = "http://tempuri.org/MailTemplateSchema.xsd", IsNullable = false)]
    public class SMTPSimulatorXml
    {
        [XmlElement("MailTemplate")]
        public MailTemplate MailTemplate { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    [DataContract]
    public class MailTemplate : IMailTemplate
    {
        [DataMember]
        [XmlIgnore]
        public string File { get; set; }

        [DataMember]
        [XmlIgnore]
        [Key]
        public int Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public EncodingType HeaderEncoding { get; set; }

        [DataMember]
        public EncodingType SubjectEncoding { get; set; }

        [DataMember]
        public EncodingType BodyEncoding { get; set; }

        [DataMember]
        public string Html { get; set; }

        [DataMember]
        public string Text { get; set; }

        [XmlAttribute]
        [DataMember]
        public MailTemplateTypeBehaviour Behaviour { get; set; }
    }
}
