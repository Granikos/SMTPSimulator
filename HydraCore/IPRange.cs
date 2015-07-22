using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace HydraCore
{
    [DataContract]
    public class IPRange
    {
        private IPAddress _start;
        private IPAddress _end;

        // TODO: Use converter instead to remove dependency
        [JsonConstructor]
        public IPRange(string startString, string endString)
        {
            Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(startString));
            Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(endString));

            StartString = startString;
            EndString = endString;
        }

        public IPRange(IPAddress start, IPAddress end)
        {
            Contract.Requires<ArgumentNullException>(start != null);
            Contract.Requires<ArgumentNullException>(end != null);
            Contract.Requires<ArgumentException>(start.AddressFamily == end.AddressFamily);

            _start = start;
            _end = end;
        }

        public IPAddress Start
        {
            get { return _start; }
        }

        public IPAddress End
        {
            get { return _end; }
        }

        [DataMember]
        public string StartString
        {
            get { return _start.ToString(); }
            set { _start = IPAddress.Parse(value); }
        }

        [DataMember]
        public string EndString
        {
            get { return _end.ToString(); }
            set { _end = IPAddress.Parse(value); }
        }

        public bool Contains(IPAddress address)
        {
            if (address.AddressFamily != Start.AddressFamily)
            {
                return false;
            }

            var lowerBytes = Start.GetAddressBytes();
            var upperBytes = End.GetAddressBytes();
            byte[] addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (int i = 0; i < lowerBytes.Length &&
                            (lowerBoundary || upperBoundary); i++)
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