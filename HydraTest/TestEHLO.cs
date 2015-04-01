using HydraCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HydraTest
{
    [TestClass]
    public class TestEHLO : MailBaseTest
    {
        [TestMethod]
        public void TestSuccess()
        {
            Server.Greet = "Test Greet";

            var response = ExecuteCommand("EHLO", "test-domain.com");

            Assert.AreEqual(SMTPStatusCode.Okay, response.Code);
            Assert.AreEqual(1, response.Args.Length);
            Assert.AreEqual(Server.Greet, response.Args[0]);
            Assert.AreEqual("test-domain.com", Transaction.ClientIdentifier);
            Assert.IsTrue(Transaction.Initialized);
        }

        [TestMethod]
        public void TestErrorWithNoParams()
        {
            var response = ExecuteCommand("EHLO");

            Assert.AreEqual(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}