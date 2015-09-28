using System;
using System.Runtime.Serialization;

namespace HydraService.Models
{
    [DataContract]
    public struct VersionInfo
    {
        [DataMember]
        public Version Version { get; set; }

        [DataMember]
        public DateTime BuildDate { get; set; }
    }
}