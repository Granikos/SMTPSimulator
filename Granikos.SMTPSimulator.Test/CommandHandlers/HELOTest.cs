using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.SmtpServer.CommandHandlers;
using Granikos.SMTPSimulator.SmtpServer.Fakes;
using Xunit;

namespace SMTPSimulatorTest.CommandHandlers
{
    public class HELOTest : TestBase
    {
        [Fact]
        public void TestSuccess()
        {
            const string greet = "Test Greet";

            var handler = new HELOHandler();

            var init = false;
            string clientId = null;
            var reset = false;

            Transaction.InitializeString = s =>
            {
                init = true;
                clientId = s;
            };
            Transaction.Reset = () => { reset = true; };
            Transaction.SettingsGet = () => new StubIReceiveSettings
            {
                GreetGet = () => greet
            };

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "test");

            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Equal(1, response.Args.Length);
            Assert.Equal(greet, response.Args[0]);
            Assert.Equal("test", clientId);
            Assert.True(init);
            Assert.True(reset);
        }

        [Fact]
        public void TestFail()
        {
            var handler = new HELOHandler();

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "");

            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}