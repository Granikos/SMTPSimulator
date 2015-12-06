using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Database.Models
{
    public abstract class User : IUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required]
        public string Mailbox { get; set; }

        [Key]
        public int Id { get; set; }
    }

    public class LocalUser : User
    {
        public virtual ICollection<LocalUserGroup> Groups { get; set; } 
    }

    public class ExternalUser : User
    {
        public virtual ICollection<ExternalUserGroup> Groups { get; set; } 
    }
}