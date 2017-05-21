using System;
using System.Diagnostics;
using System.Net;

namespace Granikos.SMTPSimulator.Service.Models
{
    public class IPRange : IIpRange
    {
        private IPAddress _start;
        private IPAddress _end;

        public IPRange(IPAddress start, IPAddress end)
        {
            if (start == null) throw new ArgumentNullException();
            if (end == null) throw new ArgumentNullException();
            if (!(start.AddressFamily == end.AddressFamily)) throw new ArgumentException();

            Start = start;
            End = end;
        }

        public IPAddress Start
        {
            get { return _start; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!(value.AddressFamily == End.AddressFamily)) throw new ArgumentException();

                _start = value;
            }
        }

        public IPAddress End
        {
            get { return _end; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!(value.AddressFamily == Start.AddressFamily)) throw new ArgumentException();
                
                _end = value;
            }
        }
    }
}