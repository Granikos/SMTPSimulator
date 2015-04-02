using System;
using System.Collections.Generic;
using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class EHLOTest
    {
        [Theory]
        [InlineData("test", new string[0])]
        [InlineData("test", new [] {"test2"})]
        [InlineData("test", new [] {"test2", "test3"})]
        public void TestSuccess(string parameters, string[] lines)
        {
            const string greet = "Test Greet";

            using (ShimsContext.Create())
            {
                var handler = new EHLOHandler();

                var init = false;
                string clientId = null;
                var reset = false;

                var server = new ShimSMTPCore
                {
                    GreetGet = () => greet
                };

                server.GetListPropertyOf1String(s =>
                {
                    switch (s)
                    {
                        case "EHLOLines":
                            return new List<string>(lines);
                        default:
                            throw new InvalidOperationException("Invalid name.");
                    }
                });

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

                var response = handler.Execute(transaction, parameters);

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
        }

        [Fact]
        public void TestFail()
        {
            using (ShimsContext.Create())
            {
                var handler = new EHLOHandler();

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction();

                handler.Initialize(server);

                var response = handler.Execute(transaction, "");

                Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
            }
        }
    }
}