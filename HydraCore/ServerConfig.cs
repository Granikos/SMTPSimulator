using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace HydraCore
{
    [DataContract]
    public class ServerConfig
    {
        [Required]
        [DataMember]
        public string Banner { get; set; }

        [Required]
        [DataMember]
        public string Greet { get; set; }

        [Required]
        [DataMember]
        public string ServerName { get; set; }
    }
}
