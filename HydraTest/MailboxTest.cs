using HydraCore;
using Xunit;

namespace HydraTest
{
    public class MailboxTest
    {
        [Theory]
        [InlineData("", "test", "test.de", "test@test.de")]
        [InlineData("Tester", "test", "test.de", "Tester <test@test.de>")]
        public void TestToString(string name, string localPart, string domain, string result)
        {
            Assert.Equal(result, new Mailbox(localPart, domain, name).ToString());
        }

        [Fact]
        public void TestEquals()
        {
            var mailbox1 = new Mailbox("tester", "test.de");
            var mailbox2 = new Mailbox("Tester", "Test.de");
            var mailbox3 = new Mailbox("tester", "test.de", "Tester");
            var mailbox4 = new Mailbox("tester2", "test.de");

            Assert.False(mailbox1.Equals(null));
            Assert.Equal(mailbox1, mailbox2);
            Assert.Equal(mailbox1, mailbox3);
            Assert.Equal(mailbox2, mailbox3);
            Assert.NotEqual(mailbox1, mailbox4);
            Assert.NotEqual(mailbox2, mailbox4);
            Assert.NotEqual(mailbox3, mailbox4);
            Assert.True(mailbox1.Equals((object) mailbox1));
            Assert.True(mailbox1.Equals((object) mailbox2));
            Assert.False(mailbox1.Equals((object) null));
            Assert.False(mailbox1.Equals(new object()));
        }
    }
}