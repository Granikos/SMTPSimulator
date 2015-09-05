using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public class Domain : IEntity<int>
    {
        public Domain(string domainName)
        {
            DomainName = domainName;
        }

        [DataMember]
        public string DomainName { get; set; }

        [DataMember]
        public int? SendConnectorId { get; set; }

        [DataMember]
        public int Id { get; set; }
    }
}