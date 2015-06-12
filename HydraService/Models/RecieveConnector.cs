using System.Net;
using System.Runtime.Serialization;
using HydraCore;

namespace HydraService.Models
{
    [DataContract]
    public class RecieveConnector : IEntity
    {
        private IPRange[] _remoteIPRanges;

        public RecieveConnector()
        {
            Port = 25;
            TLSSettings = new TLSSettings();
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public IPAddress Address { get; set; }
        
        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public IPRange[] RemoteIPRanges
        {
            get { return _remoteIPRanges ?? new IPRange[0]; }
            set { _remoteIPRanges = value; }
        }

        [DataMember]
        public bool EnableSsl { get; set; }
        
        [DataMember]
        public bool RequireTLS { get; set; }

        [DataMember]
        public bool RequireAuth { get; set; }

        [DataMember]
        public TLSSettings TLSSettings { get; set; }

        [DataMember]
        public string Banner { get; set; }
    }
}
