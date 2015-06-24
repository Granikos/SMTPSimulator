using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public class ExternalUser : IEntity<int>
    {
        [DataMember]
        [Required]
        public int Id { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        [Required]
        public string Mailbox { get; set; }

        [DataMember]
        [Required]
        public int DomainId { get; set; }
    }
}