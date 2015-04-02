using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class NOOPTest
    {
        [Fact]
        public void TestSuccess()
        {
            using (ShimsContext.Create())
            {
                var handler = new NOOPHandler();

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction();

                handler.Initialize(server);

                var response = handler.Execute(transaction, null);
                Assert.Equal(SMTPStatusCode.Okay, response.Code);
            }
        }
    }
}