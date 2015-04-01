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
            Assert.AreEqual("tester@domain.com", Transaction.ReversePath);
            Assert.IsTrue(Transaction.MailInProgress);

            response = ExecuteCommand("RCPT", "TO:<tester2@domain.com>");
            Assert.AreEqual(SMTPStatusCode.Okay, response.Code);
            CollectionAssert.Contains(Transaction.ForwardPath, "tester2@domain.com");
            Assert.IsTrue(Transaction.MailInProgress);

            response = ExecuteCommand("DATA");
            Assert.AreEqual(SMTPStatusCode.StartMailInput, response.Code);
            Assert.IsTrue(Transaction.MailInProgress);
            Assert.IsTrue(Transaction.DataMode);

            string sender = null;
            string[] recipients = new string[0];
            string body = null;

            Server.OnNewMessage += (t, s, r, b) =>
            {
                sender = s;
                recipients = r;
                body = b;
            };

            response = HandleData("DATA", "Test Mail Content...");
            Assert.AreEqual(SMTPStatusCode.Okay, response.Code);
            Assert.IsFalse(Transaction.MailInProgress);
            Assert.IsFalse(Transaction.DataMode);

            Assert.AreEqual("tester@domain.com", sender);
            Assert.AreEqual(1, recipients.Length);
            Assert.AreEqual("tester2@domain.com", recipients[0]);
            Assert.AreEqual("Test Mail Content...", body);
        }
    }
}