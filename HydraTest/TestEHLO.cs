using System.Collections.Generic;
using System.Collections.Specialized;
using HydraCore;
using HydraCore.CommandHandlers;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace HydraTest
{
    public class TestEHLO
    {
        [Theory]
        [InlineData("test")]
        public void TestSuccess(string parameters)
        {
            const string greet = "Test Greet";

            using (ShimsContext.Create())
            {
                var handler = new MAILHandler();

                var init = false;
                string clientId = null;

                var server = new HydraCore.Fakes.ShimSMTPCore
                {
                    GreetGet = () => greet
                };

                var transaction = new HydraCore.Fakes.ShimSMTPTransaction
                {
                    InitializeString = s =>
                    {
                        init = true;
                        clientId = s;
                    }
                };

                handler.Initialize(server);

                var response = handler.Execute(transaction, parameters);

                Assert.Equals(SMTPStatusCode.Okay, response.Code);
                Assert.AreEqual(1, response.Args.Length);
                Assert.AreEqual(greet, response.Args[0]);
                Assert.AreEqual(parameters, clientId);
                Assert.IsTrue(init);
            }
        }
    }
}