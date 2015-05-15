using System.Net;
using System.Runtime.Serialization;

namespace HydraService
{
    [DataContract]
    public class ServerSubnetConfiguration
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public IPAddress Address { get; set; }

        [DataMember]
        public int Size { get; set; }
    }
}