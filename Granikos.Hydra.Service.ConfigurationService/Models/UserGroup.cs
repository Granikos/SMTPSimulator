using System.Runtime.Serialization;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
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