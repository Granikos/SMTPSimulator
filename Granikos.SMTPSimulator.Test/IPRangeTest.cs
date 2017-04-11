using System.Net;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;
using Granikos.SMTPSimulator.Service.Models;
using Xunit;

namespace Granikos.SMTPSimulator.Test
{
    public class IPRangeTest
    {
        [Fact]
        public void TestProperties()
        {
            var ip1 = IPAddress.Parse("1.1.1.1");
            var ip2 = IPAddress.Parse("2.2.2.2");
            var ip3 = IPAddress.Parse("3.3.3.3");
            var ip4 = IPAddress.Parse("4.4.4.4");

            var range = new JsonIPRange(ip1, ip2);

            Assert.Equal(ip1.ToString(), range.StartString);
            Assert.Equal(ip2.ToString(), range.EndString);

            range.StartString = ip3.ToString();
            range.EndString = ip4.ToString();

            Assert.Equal(ip3, range.Start);
            Assert.Equal(ip4, range.End);
        }

        [Theory]
        [InlineData("1.1.1.1", "2.2.2.2", "1.1.1.1", true)]
        [InlineData("1.1.1.1", "2.2.2.2", "2.2.2.2", true)]
        [InlineData("1.1.1.1", "2.2.2.2", "3.3.3.3", false)]
        [InlineData("1.1.1.1", "2.2.2.2", "1.1.1.0", false)]
        [InlineData("1.1.1.1", "2.2.2.2", "1.255.255.255", true)]
        [InlineData("1.0.0.0", "1.0.0.200", "1.0.0.15", true)]
        [InlineData("1.0.0.0", "1.0.0.200", "1.0.0.201", false)]
        [InlineData("1.0.0.0", "1.0.0.200", "1.0.1.0", false)]
        [InlineData("1.0.0.0", "1.0.0.200", "0:0:0:0:0:0:0:1", false)]
        public void TestContains(string start, string end, string ip, bool contains)
        {
            var ip1 = IPAddress.Parse(start);
            var ip2 = IPAddress.Parse(end);
            var ip3 = IPAddress.Parse(ip);
            var range = new JsonIPRange(ip1, ip2);

            if (contains)
            {
                Assert.True(range.Contains(ip3));
            }
            else
            {
                Assert.False(range.Contains(ip3));
            }
        }
    }
}