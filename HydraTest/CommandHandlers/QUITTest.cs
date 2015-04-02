using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class QUITTest
    {
        [Fact]
        public void TestSuccess()
        {
            using (ShimsContext.Create())
            {
                var handler = new QUITHandler();

                var closed = false;

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction()
                {
                    Close = () => closed = true
                };

                handler.Initialize(server);

                var response = handler.Execute(transaction, null);
                Assert.Equal(SMTPStatusCode.Closing, response.Code);
                Assert.True(closed);
            }
        }
    }
}