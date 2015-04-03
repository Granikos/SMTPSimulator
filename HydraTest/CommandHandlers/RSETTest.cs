using HydraCore;
using HydraCore.CommandHandlers;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class RSETTest : TestBase
    {
        [Fact]
        public void TestSuccess()
        {
            var handler = new RSETHandler();

            var reset = false;
            Transaction.Reset = () => reset = true;

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, null);
            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.True(reset);
        }

        [Fact]
        public void TestError()
        {
            var handler = new RSETHandler();

            var response = handler.Execute(Transaction, "fubar");
            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}