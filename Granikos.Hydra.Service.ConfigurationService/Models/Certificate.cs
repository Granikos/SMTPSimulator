using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    public class Certificate : ICertificate
    {
        public string Name { get; set; }

        public byte[] Content { get; set; }
    }
}