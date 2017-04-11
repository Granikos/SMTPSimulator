using System.Collections.Generic;
using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.SmtpServer.CommandHandlers;
using Xunit;

namespace SMTPSimulatorTest.CommandHandlers
{
    public class RCPTTest : TestBase
    {
        [Fact]
        public void TestSuccess()
        {
            var paths = new List<MailPath>();
            var permanent = false;

            AddTransactionProperty("MailInProgress", true);
            AddTransactionListProperty("ForwardPath", () => paths, b => permanent = b);

            var handler = new RCPTHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "TO:<test@test.de>");
            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Contains(new MailPath("test", "test.de"), paths);
            Assert.False(permanent);
        }

        [Fact]
        public void TestSuccessWithPostmaster()
        {
            var paths = new List<MailPath>();
            var permanent = false;

            AddTransactionProperty("MailInProgress", true);
            AddTransactionListProperty("ForwardPath", () => paths, b => permanent = b);

            var handler = new RCPTHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "TO:<postmaster>");
            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Contains(MailPath.Postmaster, paths);
            Assert.False(permanent);
        }

        [Fact]
        public void TestSequenceError()
        {
            AddTransactionProperty("MailInProgress", false);

            var handler = new RCPTHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "TO:<test@test.de>");
            Assert.Equal(SMTPStatusCode.BadSequence, response.Code);
        }

        [Fact]
        public void TestNoParamError()
        {
            AddTransactionProperty("MailInProgress", true);

            var handler = new RCPTHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "");
            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }

        [Fact]
        public void TestBadParamError()
        {
            AddTransactionProperty("MailInProgress", true);

            var handler = new RCPTHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "fubar");
            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}