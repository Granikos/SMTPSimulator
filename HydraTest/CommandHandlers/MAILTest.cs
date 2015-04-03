using System;
using HydraCore;
using HydraCore.CommandHandlers;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class MAILTest : TestBase
    {
        [Theory]
        // TODO: More cases
        [InlineData("", "", "")]
        [InlineData("\"fubar\"@test.de", "fubar", "test.de")]
        [InlineData("test@test.de", "test", "test.de")]
        public void TestSuccess(string email, string localPart, string domain)
        {
            AddTransactionProperty("MailInProgress", false);

            bool inProgress = false;
            Path reversePath = null;

            Transaction.SetPropertyStringObjectBoolean = (name, value, _) =>
            {
                switch (name)
                {
                    case "MailInProgress":
                        inProgress = (bool)value;
                        break;
                    case "ReversePath":
                        reversePath = (Path)value;
                        break;
                    default:
                        throw new InvalidOperationException("The name is invalid...");
                }
            };

            var handler = new MAILHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, String.Format("FROM:<{0}>", email));

            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.NotNull(reversePath);
            Assert.Equal(localPart, reversePath.LocalPart);
            Assert.Equal(domain, reversePath.Domain);
            Assert.True(inProgress);
        }

        [Fact]
        public void TestBadSequence()
        {
            AddTransactionProperty("MailInProgress", true);

            var handler = new MAILHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "FROM:<tester@test.de>");

            Assert.Equal(SMTPStatusCode.BadSequence, response.Code);
        }

        [Theory]
        [InlineData("fubar@test.de")]
        [InlineData("FROM:tester@test.de")]
        [InlineData("")]
        [InlineData("<>")]
        [InlineData("<fubar@test.de>")]
        [InlineData("FROM:<tester@test.de>fubar")]
        public void TestSyntaxError(string parameters)
        {
            AddTransactionProperty("MailInProgress", false);

            var handler = new MAILHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, parameters);

            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}