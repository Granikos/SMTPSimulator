using System;
using System.Collections.Generic;
using System.Linq;
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.SmtpServer;
using Granikos.NikosTwo.SmtpServer.CommandHandlers;
using Granikos.NikosTwo.SmtpServer.Fakes;
using Xunit;

namespace NikosTwoTest.CommandHandlers
{
    public class EHLOTest : TestBase
    {
        [Theory]
        [InlineData("test", new string[0])]
        [InlineData("test", new[] {"test2"})]
        [InlineData("test", new[] {"test2", ""})]
        [InlineData("test", new[] {"test2", null})]
        [InlineData("test", new[] {"test2", "test3"})]
        public void TestSuccess(string parameters, string[] lines)
        {
            const string greet = "Test Greet";

            var handler = new EHLOHandler();

            var init = false;
            string clientId = null;
            var reset = false;

            var lines2 = lines.Select<string, Func<SMTPTransaction, string>>(l => t => l);

            AddCoreListProperty("EHLOLines", () => new List<Func<SMTPTransaction, string>>(lines2));

            Transaction.InitializeString = s =>
            {
                init = true;
                clientId = s;
            };
            Transaction.Reset = () => { reset = true; };
            Transaction.SettingsGet = () => new StubIReceiveSettings
            {
                GreetGet = () => greet
            };

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, parameters);

            var expectedLines = lines.Where(l => !string.IsNullOrEmpty(l)).ToArray();

            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Equal(1 + expectedLines.Length, response.Args.Length);
            Assert.Equal(greet, response.Args[0]);
            foreach (var line in expectedLines)
            {
                Assert.Contains(line, response.Args);
            }
            Assert.Equal(parameters, clientId);
            Assert.True(init);
            Assert.True(reset);
        }

        [Fact]
        public void TestLineCallback()
        {
            const string greet = "Test Greet";
            const string expectedLine = "fubar";

            var handler = new EHLOHandler();

            SMTPTransaction actualTransaction = null;

            AddCoreListProperty("EHLOLines", () => new List<Func<SMTPTransaction, string>>
            {
                t =>
                {
                    actualTransaction = t;
                    return expectedLine;
                }
            });

            Transaction.InitializeString = s => { };
            Transaction.Reset = () => { };
            Transaction.SettingsGet = () => new StubIReceiveSettings
            {
                GreetGet = () => greet
            };

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "test");

            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Equal(2, response.Args.Length);
            Assert.Equal(greet, response.Args[0]);
            Assert.Equal(expectedLine, response.Args[1]);
            Assert.Equal(Transaction, actualTransaction);
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