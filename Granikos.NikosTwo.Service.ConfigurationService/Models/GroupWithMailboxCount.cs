using System.Runtime.Serialization;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class GroupWithMailboxCount
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int MailboxCount { get; set; }
    }
}