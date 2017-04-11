using Granikos.SMTPSimulator.Core;
using Xunit;

namespace SMTPSimulatorTest
{
    public class ResponseTest
    {
        [Theory]
        [InlineData(SMTPStatusCode.Okay, new string[0], "250 Okay")]
        [InlineData(SMTPStatusCode.Okay, new[] {"Message"}, "250 Message")]
        [InlineData(SMTPStatusCode.Okay, new[] {"Message", "Message 2"}, "250-Message\r\n250 Message 2")]
        [InlineData(SMTPStatusCode.NotImplemented, new[] {"Message", "Message 2", "Message 3"},
            "502-Message\r\n502-Message 2\r\n502 Message 3")]
        public void TestToString(SMTPStatusCode code, string[] args, string expected)
        {
            var result = new SMTPResponse(code, args).ToString();

            Assert.Equal(expected, result);
        }
    }
}