using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.ConfigurationService.Models
{
    public class Certificate : ICertificate
    {
        public string Name { get; set; }

        public byte[] Content { get; set; }
    }
}