using System;
using System.Net;
using HydraCore;
using Xunit;

namespace HydraTest
{
    public class IPSubnetTest
    {
        [Fact]
        public void TestSubnet()
        {
            var ip = IPAddress.Parse("192.168.0.1");
            var subnetMask = IPAddress.Parse("255.255.255.0");
            var ip2 = IPAddress.Parse("192.168.0.25");
            var ip3 = IPAddress.Parse("192.168.2.25");

            var subnet = new IPSubnet(ip, subnetMask);

            Assert.True(subnet.Contains(ip));
            Assert.True(subnet.Contains(ip2));
            Assert.False(subnet.Contains(ip3));
        }

        [Fact]
        public void TestParse()
        {
            var ip = IPAddress.Parse("192.168.0.1");
            var subnetMask = IPAddress.Parse("255.255.255.0");

            var subnet1 = new IPSubnet(ip, subnetMask);
            var subnet2 = new IPSubnet(ip, 24);
            var subnet3 = new IPSubnet("192.168.0.0/24");

            Assert.Equal(subnet1, subnet2);
            Assert.Equal(subnet1, subnet3);
        }

        [Fact]
        public void TestEquals()
        {
            var ip1 = IPAddress.Parse("192.168.0.1");
            var ip2 = IPAddress.Parse("192.168.0.25");
            var subnetMask1 = IPAddress.Parse("255.255.255.0");
            var subnetMask2 = IPAddress.Parse("255.255.127.0");

            var subnet1 = new IPSubnet(ip1, subnetMask1);
            var subnet2 = new IPSubnet(ip2, subnetMask1);
            var subnet3 = new IPSubnet(ip1, subnetMask2);
            var subnet4 = new IPSubnet(ip2, subnetMask2);

            Assert.False(subnet1.Equals(null));
            Assert.Equal(subnet1, subnet2);
            Assert.NotEqual(subnet1, subnet3);
            Assert.NotEqual(subnet2, subnet3);
            Assert.NotEqual(subnet1, subnet4);
            Assert.NotEqual(subnet2, subnet4);
            Assert.Equal(subnet3, subnet4);
        }
    }
}
