using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class ConnectionTestSettings
    {
        [DataMember]
        [Required]
        public string Host { get; set; }

        [DataMember]
        [Range(0, 65535)]
        [Required]
        public int Port { get; set; }

        [DataMember]
        public bool Ssl { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string AuthMethod { get; set; }
    }
}