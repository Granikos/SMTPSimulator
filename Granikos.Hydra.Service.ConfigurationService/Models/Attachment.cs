using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    public class Attachment : IAttachment
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public byte[] Content { get; set; }
    }
}
