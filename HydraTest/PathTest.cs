using System;
using HydraCore;
using Xunit;

namespace HydraTest
{
    public class PathTest
    {
        [Theory]
        [InlineData("<tester@test.de>", "tester", "test.de", new string[0])]
        [InlineData("<a.b@d.e>", "a.b", "d.e", new string[0])]
        [InlineData("<\"Na,sowas\"@d.e>", "Na,sowas", "d.e", new string[0])]
        [InlineData("<a.b@[127.0.0.1]>", "a.b", "[127.0.0.1]", new string[0])]
        [InlineData("<@foo.de,@bla.de:tester@test.de>", "tester", "test.de", new[] { "foo.de", "bla.de" })]
        [InlineData("<@[IPv6:2607:f0d0:1002:51::4]:tester@test.de>", "tester", "test.de", new[] { "[IPv6:2607:f0d0:1002:51::4]" })]
        public void TestFromString(string input, string localPart, string domain, string[] atDomains)
        {
            var path = Path.FromString(input);
            Assert.Equal(localPart, path.LocalPart);
            Assert.Equal(domain, path.Domain);
            Assert.Equal(atDomains, path.AtDomains);
        }

        [Theory]
        [InlineData("tester@test.de")]
        [InlineData("<tester@test>")]
        [InlineData("<:tester@test.de>")]
        [InlineData("<foo.de:tester@test.de>")]
        [InlineData("<@foo:tester@test.de>")]
        [InlineData("<space tester@test.de>")]
        public void TestFromStringError(string input)
        {
            Assert.Throws<ArgumentException>(() => Path.FromString(input));
        }

        [Theory]
        [InlineData("<tester@test.de>", "tester", "test.de", new string[0])]
        [InlineData("<a.b@d.e>", "a.b", "d.e", new string[0])]
        [InlineData("<\"Na,sowas\"@d.e>", "Na,sowas", "d.e", new string[0])]
        [InlineData("<a.b@[127.0.0.1]>", "a.b", "[127.0.0.1]", new string[0])]
        [InlineData("<@foo.de,@bla.de:tester@test.de>", "tester", "test.de", new[] { "foo.de", "bla.de" })]
        [InlineData("<@[IPv6:2607:f0d0:1002:51::4]:tester@test.de>", "tester", "test.de", new[] { "[IPv6:2607:f0d0:1002:51::4]" })]
        public void TestToString(string output, string localPart, string domain, string[] atDomains)
        {
            var path = new Path(localPart, domain, atDomains);
            Assert.Equal(output, path.ToString());
        }

        [Fact]
        public void TestToStringEmpty()
        {
            Assert.Equal("<>", Path.Empty.ToString());
        }

        [Fact]
        public void TestEquals()
        {
            var path1 = new Path("tester", "test.de");
            var path2 = new Path("Tester", "Test.de");
            var path3 = new Path("tester2", "test.de");
            var path4 = new Path("tester", "test.de", "fubar.de");

            Assert.Equal(path1, path2);
            Assert.NotEqual(path1, path3);
            Assert.Equal(path1, path4);
            Assert.NotEqual(path2, path3);
            Assert.Equal(path2, path4);
            Assert.NotEqual(path3, path4);

            Assert.False(path1.Equals(null));
            Assert.False(path1.Equals(new object()));
            Assert.True(path1.Equals((object)path2));
            Assert.False(path1.Equals((object)path3));
        }
    }
}
