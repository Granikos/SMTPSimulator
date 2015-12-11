using System.IO;
using System.Text;
using System.Xml.Serialization;
using Granikos.Hydra.Service.ConfigurationService.Models;

namespace Granikos.Hydra.Service
{
    class MailTemplateExporter
    {
        public void ExportAsXml(Stream stream, MailTemplate template)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1000, true))
            {
                var n2 = new NikosTwo {MailTemplate = template};
                var serializer = new XmlSerializer(typeof(NikosTwo));
                serializer.Serialize(writer, n2);
            }
        }
    }
}