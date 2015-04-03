using HydraCore;
using HydraCore.CommandHandlers;
using Xunit;

namespace HydraTest.CommandHandlers
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
    }
}