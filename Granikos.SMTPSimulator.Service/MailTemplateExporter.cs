using System.IO;
using System.Text;
using System.Xml.Serialization;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;

namespace Granikos.SMTPSimulator.Service
{
    class MailTemplateExporter
    {
        public void ExportAsXml(Stream stream, MailTemplate template)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1000, true))
            {
                var n2 = new SMTPSimulatorXml {MailTemplate = template};
                var serializer = new XmlSerializer(typeof(SMTPSimulatorXml));
                serializer.Serialize(writer, n2);
            }
        }
    }
}