using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class RSETTest
    {
        [Fact]
        public void TestSuccess()
        {
            using (ShimsContext.Create())
            {
                var handler = new RSETHandler();

                var reset = false;

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction()
                {
                    Reset = () => reset = true
                };

                handler.Initialize(server);

                var response = handler.Execute(transaction, null);
                Assert.Equal(SMTPStatusCode.Okay, response.Code);
                Assert.True(reset);
            }
        }
    }
}