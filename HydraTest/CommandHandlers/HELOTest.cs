using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class HELOTest
    {
        [Fact]
        public void TestSuccess()
        {
            const string greet = "Test Greet";

            using (ShimsContext.Create())
            {
                var handler = new HELOHandler();

                var init = false;
                string clientId = null;
                var reset = false;

                var server = new ShimSMTPCore
                {
                    GreetGet = () => greet
                };

                var transaction = new ShimSMTPTransaction
                {
                    InitializeString = s =>
                    {
                        init = true;
                        clientId = s;
                    },
                    Reset = () =>
                    {
                        reset = true;
                    }
                };

                handler.Initialize(server);

                var response = handler.Execute(transaction, "test");

                Assert.Equal(SMTPStatusCode.Okay, response.Code);
                Assert.Equal(1, response.Args.Length);
                Assert.Equal(greet, response.Args[0]);
                Assert.Equal("test", clientId);
                Assert.True(init);
                Assert.True(reset);
            }
        }

        [Fact]
        public void TestFail()
        {
            using (ShimsContext.Create())
            {
                var handler = new HELOHandler();

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction();

                handler.Initialize(server);

                var response = handler.Execute(transaction, "");

                Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
            }
        }
    }
}