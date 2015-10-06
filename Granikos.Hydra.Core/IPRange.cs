using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.Serialization;

namespace Granikos.Hydra.Core
{
    [DataContract]
    public class IPRange
    {
        public IPRange(IPAddress start, IPAddress end)
        {
            Contract.Requires<ArgumentNullException>(start != null);
            Contract.Requires<ArgumentNullException>(end != null);
            Contract.Requires<ArgumentException>(start.AddressFamily == end.AddressFamily);

            Start = start;
            End = end;
        }

        public IPAddress Start { get; private set; }
        public IPAddress End { get; private set; }

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

        public bool Contains(IPAddress address)
        {
            if (address.AddressFamily != Start.AddressFamily)
            {
                return false;
            }

            var lowerBytes = Start.GetAddressBytes();
            var upperBytes = End.GetAddressBytes();
            var addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (var i = 0;
                i < lowerBytes.Length &&
                (lowerBoundary || upperBoundary);
                i++)
            {
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
            }

            return true;
        }
    }
}