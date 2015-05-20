using System.Net;
using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public class ServerSubnetConfiguration : IEntity
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public IPAddress Address { get; set; }

        [DataMember]
        public int Size { get; set; }
    }
}