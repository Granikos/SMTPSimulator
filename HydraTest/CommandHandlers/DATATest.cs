using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.CommandHandlers.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class DATATest : TestBase
    {
        [Fact]
        public void TestSuccess()
        {
            AddTransactionProperty("MailInProgress", true);
            AddTransactionListProperty("ForwardPath", new List<MailPath> {new MailPath("fubar", "fubar.de")});

            var dataMode = false;
            var dataHandlerCalled = false;
            var dataLineHandlerCalled = false;
            Func<string, StringBuilder, bool> dataLineHandler = null;
            Func<string, SMTPResponse> dataHandler = null;

            Transaction.StartDataModeFuncOfStringStringBuilderBooleanFuncOfStringSMTPResponse =
                (func, func1) =>
                {
                    dataMode = true;
                    dataLineHandler = func;
                    dataHandler = func1;
                };

            ShimDATAHandler.DataHandlerSMTPTransactionString = (transaction, s) =>
            {
                dataHandlerCalled = true;
                return null;
            };

            ShimDATAHandler.DataLineHandlerStringStringBuilder = (s, builder) =>
            {
                dataLineHandlerCalled = true;
                return false;
            };

            var handler = new DATAHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "");

            if (dataHandler != null) dataHandler(null);
            if (dataLineHandler != null) dataLineHandler(null, null);

            Assert.Equal(SMTPStatusCode.StartMailInput, response.Code);
            Assert.True(dataMode);
            Assert.True(dataHandlerCalled);
            Assert.True(dataLineHandlerCalled);
        }

        [Fact]
        public void TestSyntaxError()
        {
            var handler = new DATAHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "fubar");

            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }

        [Fact]
        public void TestBadSequenceBecauseNotMailing()
        {
            AddTransactionProperty("MailInProgress", false);
            AddTransactionListProperty("ForwardPath", new List<MailPath> {new MailPath("fubar", "fubar.de")});

            var handler = new DATAHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "");

            Assert.Equal(SMTPStatusCode.BadSequence, response.Code);
        }

        [Fact]
        public void TestBadSequenceBecauseNoRecipients()
        {
            AddTransactionProperty("MailInProgress", true);
            AddTransactionListProperty("ForwardPath", new List<MailPath>());

            var handler = new DATAHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "");

            Assert.Equal(SMTPStatusCode.BadSequence, response.Code);
        }

        [Fact]
        public void TestDataHandler()
        {
            var expectedSender = new MailPath("tester", "test.de");
            var expectedRecipients = new List<MailPath> {new MailPath("tester", "fubar.de")};
            var expectedBody = "Body";

            AddTransactionProperty("ReversePath", () => expectedSender);
            AddTransactionListProperty("ForwardPath", () => expectedRecipients);

            var reset = false;

            Transaction.Reset = () => { reset = true; };

            MailPath actualSender = null;
            List<MailPath> actualRecipients = null;
            string actualBody = null;

            Core.TriggerNewMessageSMTPTransactionMailPathMailPathArrayString = (transaction, sender, recipients, body) =>
            {
                actualSender = sender;
                actualRecipients = recipients.ToList();
                actualBody = body;
            };

            var response = DATAHandler.DataHandler(Transaction, expectedBody);

            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Equal(expectedSender, actualSender);
            Assert.Equal(expectedRecipients, actualRecipients);
            Assert.Equal(expectedBody, actualBody);
            Assert.True(reset);
        }

        [Theory]
        [InlineData(".", "", false, "")]
        [InlineData("..", ".", true, "")]
        [InlineData("test", "test", true, "")]
        [InlineData("test2", "test1\r\ntest2", true, "test1")]
        [InlineData(".", "test1", false, "test1")]
        public void TestDataLineHandler(string line, string dataAfter, bool expectedResult, string dataBefore)
        {
            var sb = new StringBuilder(dataBefore);
            var actualResult = DATAHandler.DataLineHandler(line, sb);

            Assert.Equal(expectedResult, actualResult);
            Assert.Equal(dataAfter, sb.ToString());
        }
    }
}