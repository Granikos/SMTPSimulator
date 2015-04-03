using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class DATATest : TestBase
    {
        [Fact]
        public void TestSuccess()
        {
            var handler = new DATAHandler();

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, null);
            Assert.Equal(SMTPStatusCode.Okay, response.Code);
        }
    }
}