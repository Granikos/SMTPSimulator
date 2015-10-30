using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Granikos.Hydra.Service.TimeTables
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    [XmlRoot(Namespace = "http://tempuri.org/MailTemplateSchema.xsd", IsNullable = false)]
    public class NikosTwo
    {
        [XmlElement("MailTemplate")]
        public MailTemplateType[] Items { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    [DataContract]
    public class MailTemplateType
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public EncodingType HeaderEncoding { get; set; }

        [DataMember]
        public EncodingType ContentEncoding { get; set; }

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
