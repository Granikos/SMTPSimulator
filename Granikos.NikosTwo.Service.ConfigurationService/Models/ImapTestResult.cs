using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class ImapTestResult
    {

        [DataMember]
        public bool ConnectSuccess { get; set; }

        [DataMember]
        public bool? LoginSuccess { get; set; }

        [DataMember]
        [Required]
        public string ErrorMessage { get; set; }

        [DataMember]
        public string ProtocolLog { get; set; }

        [DataMember]
        public string[] PreAuthCapabilities { get; set; }

        [DataMember]
        public string[] PostAuthCapabilities { get; set; }
    }
}
