using System.Net;
using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public class ServerBindingConfiguration : IEntity
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public IPAddress Address { get; set; }
        
        [DataMember]
        public int Port { get; set; }
        
        [DataMember]
        public bool EnableSsl { get; set; }
        
        [DataMember]
        public bool EnforceTLS { get; set; }
    }
}
