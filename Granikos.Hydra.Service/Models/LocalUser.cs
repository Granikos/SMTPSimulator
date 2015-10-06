using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
{
    [DataContract]
    public class LocalUser : IEntity<int>
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Mailbox { get; set; }

        [DataMember]
        public int Id { get; set; }
    }
}