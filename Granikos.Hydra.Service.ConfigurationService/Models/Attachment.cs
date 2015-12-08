using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.ConfigurationService.Models
{
    public class Attachment : IAttachment
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public byte[] Content { get; set; }
    }
}
