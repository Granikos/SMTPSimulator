using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.ConfigurationService.Models
{
    public class Attachment : IAttachment
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public byte[] Content { get; set; }
    }
}
