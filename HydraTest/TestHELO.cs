using HydraCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HydraTest
{
    [TestClass]
    public class TestHELO : MailBaseTest
    {
        [TestMethod]
        public void TestSuccess()
        {
            Server.Greet = "Test Greet";

            var response = ExecuteCommand("HELO", "test-domain.com");

            Assert.AreEqual(SMTPStatusCode.Okay, response.Code);
            Assert.AreEqual(1, response.Args.Length);
            Assert.AreEqual(Server.Greet, response.Args[0]);
            Assert.AreEqual("test-domain.com", Transaction.ClientIdentifier);
            Assert.IsTrue(Transaction.Initialized);
        }

        [TestMethod]
        public void TestErrorWithNoParams()
        {
            var response = ExecuteCommand("HELO");

            Assert.AreEqual(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}