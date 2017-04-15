using System.IO;
using System.Text;
using System.Xml.Serialization;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service
{
    class MailTemplateImporter
    {
        private readonly IMailTemplateProvider _mailTemplates;

        public MailTemplateImporter(IMailTemplateProvider mailTemplates)
        {
            _mailTemplates = mailTemplates;
        }

        public MailTemplate ImportFromXml(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var serializer = new XmlSerializer(typeof(SMTPSimulatorXml));
                var n2 = (SMTPSimulatorXml)serializer.Deserialize(reader);

                return _mailTemplates.Add(n2.MailTemplate).ConvertTo<MailTemplate>();
            }
        }
    }
}