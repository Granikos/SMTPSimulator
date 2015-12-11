using System.IO;
using System.Text;
using System.Xml.Serialization;
using Granikos.Hydra.Service.ConfigurationService.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service
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
                var serializer = new XmlSerializer(typeof(NikosTwo));
                var n2 = (NikosTwo)serializer.Deserialize(reader);

                return _mailTemplates.Add(n2.MailTemplate).ConvertTo<MailTemplate>();
            }
        }
    }
}