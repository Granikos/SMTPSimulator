using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class Pop3TestResult
    {
        [DataMember]
        public bool ConnectSuccess { get; set; }

        [DataMember]
        public bool? LoginSuccess { get; set; }

        [DataMember]
        [Required]
        public string ErrorMessage { get; set; }

        [DataMember]
        public string[] Capabilities { get; set; }
    }
}