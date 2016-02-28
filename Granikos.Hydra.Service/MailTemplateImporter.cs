using System.IO;
using System.Text;
using System.Xml.Serialization;
using Granikos.NikosTwo.Service.ConfigurationService.Models;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;

namespace Granikos.NikosTwo.Service
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
                var serializer = new XmlSerializer(typeof(NikosTwoXml));
                var n2 = (NikosTwoXml)serializer.Deserialize(reader);

                return _mailTemplates.Add(n2.MailTemplate).ConvertTo<MailTemplate>();
            }
        }
    }
}