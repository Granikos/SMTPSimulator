using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
{
    [DataContract]
    public class User : IEntity<int>
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        [Required]
        public string Mailbox { get; set; }

        [DataMember]
        [Required]
        public int Id { get; set; }
    }
}