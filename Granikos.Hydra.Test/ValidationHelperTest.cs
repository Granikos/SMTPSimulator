using Granikos.NikosTwo.Core;
using Xunit;

namespace NikosTwoTest
{
    public class ValidationHelperTest
    {
        [Theory]
        [InlineData("a.a")]
        [InlineData("a.a0")]
        [InlineData("a-a.aa")]
        [InlineData("a-0.b-a")]
        [InlineData("a-b-c.d")]
        [InlineData("a1.b.c")]
        [InlineData("a-b.d0.c")]
        [InlineData("o.t-9")]
        [InlineData("z-2.f6")]
        [InlineData("x.z9.u")]
        [InlineData("vy.o9.e-9.dn")]
        [InlineData("z.c")]
        [InlineData("p-8.i")]
        [InlineData("y4o29-i.k-8.n8.t.h-d8.h-0")]
        [InlineData("z.y-0")]
        [InlineData("a.a-9.u")]
        [InlineData("zz.mu")]
        public void TestIsValidDomainNameSuccess(string domain)
        {
            Assert.True(domain.IsValidDomainName());
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("a-")]
        [InlineData("a0")]
        [InlineData("a.")]
        [InlineData("a.-")]
        [InlineData("a.a-")]
        public void TestIsValidDomainNameFail(string domain)
        {
            Assert.False(domain.IsValidDomainName());
        }

        [Theory]
        [InlineData("domain.com")]
        [InlineData("[127.0.0.1]")]
        [InlineData("[1.1.1.1]")]
        [InlineData("[IPv6:FE80:0000:0000:0000:0202:B3FF:FE1E:8329]")]
        [InlineData("[IPv6:FE80::0202:B3FF:FE1E:8329]")]
        [InlineData("[IPv6:2607:f0d0:1002:51::4]")]
        [InlineData("[IPv6:fe80::230:48ff:fe33:bc33]")]
        public void TestIsValidDomainSuccess(string address)
        {
            Assert.True(address.IsValidDomain());
        }

        [Theory]
        [InlineData("127.0.0.1")]
        [InlineData("[]")]
        [InlineData("[IPv6:abc]")]
        [InlineData("IPv6:FE80:0000:0000:0000:0202:B3FF:FE1E:8329")]
        [InlineData("FE80::0202:B3FF:FE1E:8329")]
        [InlineData("[IPv6:2607:f0d0:1002:51::4")]
        [InlineData("IPv6:fe80::230:48ff:fe33:bc33]")]
        public void TestIsValidDomainFail(string address)
        {
            Assert.False(address.IsValidDomain());
        }
    }
}