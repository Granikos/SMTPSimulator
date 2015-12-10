using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.ConfigurationService.Models
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    [XmlRoot(Namespace = "http://tempuri.org/MailTemplateSchema.xsd", IsNullable = false)]
    public class NikosTwo
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
