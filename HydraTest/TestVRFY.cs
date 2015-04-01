using HydraCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HydraTest
{
    [TestClass]
    public class TestVRFY : MailBaseTest
    {
        [TestMethod]
        public void TestSingleMailbox()
        {
            var mailbox = new Mailbox("test", "test.de", "Bernd Tester");
            Server.AddMailBox(mailbox);

            var response = ExecuteCommand("VRFY", "test@test.de");

            Assert.AreEqual(SMTPStatusCode.Okay, response.Code);
            Assert.AreEqual(1, response.Args.Length);
            Assert.AreEqual(mailbox.ToString(), response.Args[0]);
        }

        [TestMethod]
        public void TestMultipleMailboxes()
        {
            var mailbox = new Mailbox("test", "test.de", "Bernd Tester");
            var mailbox2 = new Mailbox("test2", "test.de", "Bernd Tester2");
            Server.AddMailBox(mailbox);
            Server.AddMailBox(mailbox2);

            var response = ExecuteCommand("VRFY", "test");

            Assert.AreEqual(SMTPStatusCode.MailboxNameNotAllowed, response.Code);
            Assert.AreEqual(2, response.Args.Length);
            CollectionAssert.Contains(response.Args, mailbox.ToString());
            CollectionAssert.Contains(response.Args, mailbox2.ToString());
        }

        [TestMethod]
        public void TestMailboxNotFound()
        {
            var response = ExecuteCommand("VRFY", "test@test.de");

            Assert.AreEqual(SMTPStatusCode.MailboxUnavailiableError, response.Code);
        }

        [TestMethod]
        public void TestErrorWithNoParams()
        {
            var response = ExecuteCommand("VRFY");

            Assert.AreEqual(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}