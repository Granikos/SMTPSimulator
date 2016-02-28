using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class User : IUser
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        [Required]
        public string Mailbox { get; set; }
    }
}