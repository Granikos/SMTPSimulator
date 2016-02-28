using System.IO;
using System.Text;
using System.Xml.Serialization;
using Granikos.NikosTwo.Service.ConfigurationService.Models;

namespace Granikos.NikosTwo.Service
{
    class MailTemplateExporter
    {
        public void ExportAsXml(Stream stream, MailTemplate template)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1000, true))
            {
                var n2 = new NikosTwoXml {MailTemplate = template};
                var serializer = new XmlSerializer(typeof(NikosTwoXml));
                serializer.Serialize(writer, n2);
            }
        }
    }
}