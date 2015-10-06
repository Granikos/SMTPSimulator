using System;
using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
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