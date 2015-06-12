using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace HydraService.Models
{
    [DataContract]
    public class SendConnector : IEntity
    {
        public SendConnector()
        {
            TLSSettings = new TLSSettings();
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public IPAddress LocalAddress { get; set; }

        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public IPAddress RemoteAddress { get; set; }

        [DataMember]
        public int RemotePort { get; set; }

        [DataMember]
        public bool EnableSsl { get; set; }

        [DataMember]
        public bool UseSmarthost { get; set; }

        [DataMember]
        public TLSSettings TLSSettings { get; set; }

        [DataMember]
        public bool UseAuth { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }
    }
}