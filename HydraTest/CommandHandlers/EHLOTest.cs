using System.Collections.Generic;
using HydraCore;
using HydraCore.CommandHandlers;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class EHLOTest : TestBase
    {
        [Theory]
        [InlineData("test", new string[0])]
        [InlineData("test", new[] { "test2" })]
        [InlineData("test", new[] { "test2", "test3" })]
        public void TestSuccess(string parameters, string[] lines)
        {
            const string greet = "Test Greet";

            var handler = new EHLOHandler();

            var init = false;
            string clientId = null;
            var reset = false;

            Core.GreetGet = () => greet;

            AddCoreListProperty("EHLOLines", () => new List<string>(lines));

            Transaction.InitializeString = s =>
            {
                init = true;
                clientId = s;
            };
            Transaction.Reset = () =>
            {
                reset = true;
            };

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, parameters);

            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Equal(1 + lines.Length, response.Args.Length);
            Assert.Equal(greet, response.Args[0]);
            for (int i = 0; i < lines.Length; i++)
            {
                Assert.Equal(lines[i], response.Args[i + 1]);
            }
            Assert.Equal(parameters, clientId);
            Assert.True(init);
            Assert.True(reset);
        }

        [Fact]
        public void TestFail()
        {
            var handler = new EHLOHandler();

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "");

            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}