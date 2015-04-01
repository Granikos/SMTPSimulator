using System;
using System.Collections.Generic;
using HydraCore;
using HydraCore.CommandHandlers;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Assert = Xunit.Assert;

namespace HydraTest
{
    public class TestMAIL
    {
        [Theory]
        [InlineData("")]
        [InlineData("postmaster")]
        [InlineData("test@test.de")]
        public void TestSuccess(string email)
        {
            const string greet = "Test Greet";

            using (ShimsContext.Create())
            {
                var server = new HydraCore.Fakes.ShimSMTPCore();
                server.GreetGet = () => greet;

                var transaction = new HydraCore.Fakes.ShimSMTPTransaction();
                transaction.GetPropertyOf1String(name =>
                {
                    switch (name)
                    {
                        case "MailInProgress":
                            return false;
                        default:
                            throw new AssertFailedException("The name is invalid...");
                    }
                });

                bool inProgress = false;
                string reversePath = null;

                transaction.SetPropertyStringObjectBoolean = (name, value, _) =>
                {
                    switch (name)
                    {
                        case "ReversePath":
                            reversePath = (string)value;
                            break;
                        case "MailInProgress":
                            inProgress = (bool)value;
                            break;
                        default:
                            throw new AssertFailedException("The name is invalid...");
                    }
                };

                var handler = new MAILHandler();
                var response = handler.Execute(transaction, String.Format("FROM:<{0}>", email));

                Assert.Equal(SMTPStatusCode.Okay, response.Code);
                Assert.Equal(email, reversePath);
                Assert.True(inProgress);
            }
        }

        /*
        [TestMethod]
        public void TestMail()
        {
            var response = ExecuteCommand("MAIL", "FROM:<tester@domain.com>");
            Assert.AreEqual(SMTPStatusCode.Okay, response.Code);
            Assert.AreEqual("tester@domain.com", Transaction.GetProperty<string>("ReversePath"));
            Assert.IsTrue(Transaction.GetProperty<bool>("MailInProgress"));

            response = ExecuteCommand("RCPT", "TO:<tester2@domain.com>");
            Assert.AreEqual(SMTPStatusCode.Okay, response.Code);
            CollectionAssert.Contains(Transaction.GetProperty<List<string>>("ForwardPath"), "tester2@domain.com");
            Assert.IsTrue(Transaction.GetProperty<bool>("MailInProgress"));

            response = ExecuteCommand("DATA");
            Assert.AreEqual(SMTPStatusCode.StartMailInput, response.Code);
            Assert.IsTrue(Transaction.GetProperty<bool>("MailInProgress"));
            Assert.IsTrue(Transaction.InDataMode);

            string sender = null;
            string[] recipients = new string[0];
            string body = null;

            Server.OnNewMessage += (t, s, r, b) =>
            {
                sender = s;
                recipients = r;
                body = b;
            };

            response = HandleData("Test Mail Content...");
            Assert.AreEqual(SMTPStatusCode.Okay, response.Code);
            Assert.IsFalse(Transaction.GetProperty<bool>("MailInProgress"));
            Assert.IsFalse(Transaction.InDataMode);

            Assert.AreEqual("tester@domain.com", sender);
            Assert.AreEqual(1, recipients.Length);
            Assert.AreEqual("tester2@domain.com", recipients[0]);
            Assert.AreEqual("Test Mail Content...", body);
        }
        */
    }
}