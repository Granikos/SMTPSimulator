using System;
using System.Xml.Serialization;

namespace Granikos.SMTPSimulator.Service.Models
{
    public interface IMailTemplate : IEntity<int>
    {
        string Title { get; set; }
        string Subject { get; set; }
        EncodingType HeaderEncoding { get; set; }
        EncodingType SubjectEncoding { get; set; }
        EncodingType BodyEncoding { get; set; }
        string Html { get; set; }
        string Text { get; set; }
        MailTemplateTypeBehaviour Behaviour { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    public enum EncodingType
    {
        ASCII, UTF8, UTF32
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    public enum MailTemplateTypeBehaviour
    {
        SMTPCompliant
    }
}