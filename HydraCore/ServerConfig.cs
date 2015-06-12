using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace HydraCore
{
    [DataContract]
    public class ServerConfig
    {
        [Required]
        [DataMember]
        public string CertificatePath { get; set; }
    }
}
