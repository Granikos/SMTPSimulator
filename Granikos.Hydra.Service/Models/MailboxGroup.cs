using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
{
    [DataContract]
    public class MailboxGroup : IEntity<int>
    {
        private int[] _mailboxIds;

        public MailboxGroup(string name)
        {
            Name = name;
        }

        [DataMember]
        [Required]
        public string Name { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        [Required]
        public int[] MailboxIds
        {
            get { return _mailboxIds ?? new int[0]; }
            set { _mailboxIds = value; }
        }
    }
}