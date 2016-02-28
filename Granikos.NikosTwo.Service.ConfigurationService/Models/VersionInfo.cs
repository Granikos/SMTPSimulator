using System;
using System.Runtime.Serialization;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public struct VersionInfo
    {
        [DataMember]
        public string Assembly { get; set; }

        [DataMember]
        public Version Version { get; set; }

        [DataMember]
        public DateTime BuildDate { get; set; }
    }
}