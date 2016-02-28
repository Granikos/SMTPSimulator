using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.ConfigurationService.Models
{
    public class Certificate : ICertificate
    {
        public string Name { get; set; }

        public byte[] Content { get; set; }
    }
}