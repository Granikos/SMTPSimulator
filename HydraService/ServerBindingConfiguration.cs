using System.Net;
using System.Runtime.Serialization;

namespace HydraService
{
    [DataContract]
    public class ServerBindingConfiguration
    {
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
