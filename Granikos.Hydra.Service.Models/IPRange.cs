using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;

namespace Granikos.NikosTwo.Service.Models
{
    public class IPRange : IIpRange
    {
        private IPAddress _start;
        private IPAddress _end;

        public IPRange(IPAddress start, IPAddress end)
        {
            Contract.Requires<ArgumentNullException>(start != null);
            Contract.Requires<ArgumentNullException>(end != null);
            Contract.Requires<ArgumentException>(start.AddressFamily == end.AddressFamily);

            Start = start;
            End = end;
        }

        public IPAddress Start
        {
            get { return _start; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, "value");
                Contract.Requires<ArgumentException>(value.AddressFamily == End.AddressFamily);

                _start = value;
            }
        }

        public IPAddress End
        {
            get { return _end; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, "value");
                Contract.Requires<ArgumentException>(value.AddressFamily == Start.AddressFamily);
                
                _end = value;
            }
        }
    }
}