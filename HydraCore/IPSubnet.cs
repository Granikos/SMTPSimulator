using System;
using System.Diagnostics.Contracts;
using System.Net;

namespace HydraCore
{
    public struct IPSubnet
    {
        public readonly IPAddress NetworkAddress;
        public readonly IPAddress Subnet;

        public IPSubnet(IPAddress ip, IPAddress subnet)
        {
            Contract.Requires<ArgumentNullException>(ip != null);
            Contract.Requires<ArgumentNullException>(subnet != null);
            Contract.Requires<ArgumentException>(ip.GetAddressBytes().Length == subnet.GetAddressBytes().Length,
                "Lengths of IP address and subnet mask do not match.");

            NetworkAddress = CalculateNetworkAddress(ip, subnet);
            Subnet = subnet;
        }

        public IPSubnet(string ipString)
        {
            Contract.Requires<ArgumentNullException>(ipString != null);
            Contract.Requires<ArgumentException>(ipString.Split('/').Length == 2, "The IP has an invalid format");

            var parts = ipString.Split('/');

            var ip = IPAddress.Parse(parts[0]);
            Subnet = CreateSubnetMaskByNetBitLength(int.Parse(parts[1]));

            NetworkAddress = CalculateNetworkAddress(ip, Subnet);
        }

        public IPSubnet(IPAddress ip, int size)
            : this(ip, CreateSubnetMaskByNetBitLength(size))
        {
        }

        public bool Contains(IPAddress address)
        {
            return CalculateNetworkAddress(address, Subnet).Equals(NetworkAddress);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is IPSubnet && Equals((IPSubnet) obj);
        }

        public bool Equals(IPSubnet other)
        {
            return Equals(NetworkAddress, other.NetworkAddress) && Equals(Subnet, other.Subnet);
        }

        public override int GetHashCode()
        {
            //TODO : Improve
            return NetworkAddress.GetHashCode() & Subnet.GetHashCode();
        }
        public static IPAddress CreateSubnetMaskByNetBitLength(int length)
        {
            Contract.Requires<ArgumentException>(length >= 2 && length < 32, "Network size must at least be 2.");

            const long mask = 0xFFFFFFFF;
            long sizeMask = (1 << (32 - length)) - 1;
            var subnetMask = mask ^ sizeMask;

            byte[] byteMask =
            {
                (byte) ((subnetMask >> 24) & 0xFF),
                (byte) ((subnetMask >> 16) & 0xFF),
                (byte) ((subnetMask >> 8) & 0xFF),
                (byte) (subnetMask & 0xFF)
            };

            return new IPAddress(byteMask);
        }

        public static IPAddress CalculateNetworkAddress(IPAddress ip, IPAddress subnetMask)
        {
            Contract.Requires<ArgumentNullException>(ip != null);
            Contract.Requires<ArgumentNullException>(subnetMask != null);
            Contract.Requires<ArgumentException>(ip.GetAddressBytes().Length == subnetMask.GetAddressBytes().Length,
                "Lengths of IP address and subnet mask do not match.");

            var ipAdressBytes = ip.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }
    }
}