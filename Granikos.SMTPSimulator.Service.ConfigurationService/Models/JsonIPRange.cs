using System;
using System.Net;
using System.Runtime.Serialization;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.ConfigurationService.Models
{
    [DataContract]
    public class JsonIPRange : IIpRange
    {
        private IPAddress _start;
        private IPAddress _end;

        public JsonIPRange(IPAddress start, IPAddress end)
        {
            if (start == null) throw new ArgumentNullException();
            if (end == null) throw new ArgumentNullException();
            if (!(start.AddressFamily == end.AddressFamily)) throw new ArgumentException();

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
                if (value == null) throw new ArgumentNullException("value");
                if (!(End == null || value.AddressFamily == End.AddressFamily)) throw new ArgumentException();

                _start = value;
            }
        }

        public IPAddress End
        {
            get { return _end; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!(Start == null || value.AddressFamily == Start.AddressFamily)) throw new ArgumentException();
                
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