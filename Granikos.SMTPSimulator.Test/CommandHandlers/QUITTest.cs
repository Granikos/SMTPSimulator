using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.SmtpServer.CommandHandlers;
using Xunit;

namespace SMTPSimulatorTest.CommandHandlers
{
    public class QUITTest : TestBase
    {
        [Fact]
        public void TestSuccess()
        {
            var handler = new QUITHandler();

            var closed = false;
            Transaction.Close = () => closed = true;

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, null);
            Assert.Equal(SMTPStatusCode.Closing, response.Code);
            Assert.True(closed);
        }

        [Fact]
        public void TestError()
        {
            var handler = new QUITHandler();

            var response = handler.Execute(Transaction, "fubar");
            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}