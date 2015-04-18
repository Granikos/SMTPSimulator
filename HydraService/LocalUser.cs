using System.Runtime.Serialization;

namespace HydraService
{
    [DataContract]
    public class LocalUser
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Mailbox { get; set; }
    }
}