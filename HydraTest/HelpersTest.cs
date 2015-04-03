using HydraCore;
using Xunit;

namespace HydraTest
{
    public class HelpersTest
    {
        [Theory]
        [InlineData("foo.bar", "foo.bar")]
        [InlineData("bla blubb", "\"bla blubb\"")]
        [InlineData("foo..bar", "\"foo..bar\"")]
        [InlineData("abc\"def", "\"abc\\\"def\"")]
        [InlineData("abc\\def", "\"abc\\\\def\"")]
        [InlineData("abc\\\"def", "\"abc\\\\\\\"def\"")]
        public void TestToSMTPString(string input, string output)
        {
            Assert.Equal(input.ToSMTPString(), output);
        }

        [Theory]
        [InlineData("foo.bar", "foo.bar")]
        [InlineData("bla blubb", "\"bla blubb\"")]
        [InlineData("foo..bar", "\"foo..bar\"")]
        [InlineData("abc\"def", "\"abc\\\"def\"")]
        [InlineData("abc\\def", "\"abc\\\\def\"")]
        [InlineData("abc\\\"def", "\"abc\\\\\\\"def\"")]
        public void TestFromSMTPString(string output, string input)
        {
            Assert.Equal(input.FromSMTPString(), output);
        }
    }
}
