using System.Collections.Generic;
using System.Linq.Expressions;
using HydraCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HydraTest
{
    [TestClass]
    public class TestMAIL : MailBaseTest
    {
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
    }
}