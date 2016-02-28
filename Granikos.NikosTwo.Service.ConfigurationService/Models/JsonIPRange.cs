using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.Serialization;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.ConfigurationService.Models
{
    [DataContract]
    public class JsonIPRange : IIpRange
    {
        private IPAddress _start;
        private IPAddress _end;

        public JsonIPRange(IPAddress start, IPAddress end)
        {
            Contract.Requires<ArgumentNullException>(start != null);
            Contract.Requires<ArgumentNullException>(end != null);
            Contract.Requires<ArgumentException>(start.AddressFamily == end.AddressFamily);

            Start = start;
            End = end;
        }

        public JsonIPRange()
        {
        }

        public IPAddress Start
        {
            get { return _start; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, "value");
                Contract.Requires<ArgumentException>(End == null || value.AddressFamily == End.AddressFamily);

                _start = value;
            }
        }

        public IPAddress End
        {
            get { return _end; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, "value");
                Contract.Requires<ArgumentException>(Start == null || value.AddressFamily == Start.AddressFamily);
                
                _end = value;
            }
        }

        [DataMember]
        public string StartString
        {
            get { return Start.ToString(); }
            set { Start = IPAddress.Parse(value); }
        }
        
        [DataMember]
        public string EndString
        {
            get { return End.ToString(); }
            set { End = IPAddress.Parse(value); }
        }

        public static JsonIPRange FromOther(IIpRange range)
        {
            return new JsonIPRange(range.Start, range.End);
        }
    }
}