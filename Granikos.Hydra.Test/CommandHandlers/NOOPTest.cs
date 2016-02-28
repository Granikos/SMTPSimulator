using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.SmtpServer.CommandHandlers;
using Xunit;

namespace NikosTwoTest.CommandHandlers
{
    public class NOOPTest : TestBase
    {
        [Fact]
        public void TestSuccess()
        {
            var handler = new NOOPHandler();

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, null);
            Assert.Equal(SMTPStatusCode.Okay, response.Code);
        }
    }
}