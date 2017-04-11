using System.Runtime.Serialization;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.ConfigurationService.Models
{
    [DataContract]
    public class UserGroup : IUserGroup
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int[] UserIds { get; set; }
    }
}