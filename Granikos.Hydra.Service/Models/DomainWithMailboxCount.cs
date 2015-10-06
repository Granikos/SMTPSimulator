using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
{
    [DataContract]
    public class DomainWithMailboxCount
    {
        [DataMember]
        public string DomainName { get; set; }

        [DataMember]
        public int MailboxCount { get; set; }
    }
}