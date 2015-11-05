using System.Net.Mail;
using Granikos.Hydra.Core;
using Xunit;

namespace Granikos.Hydra.Test.Core
{
    public class MailContentTest
    {
        [Fact]
        public void TestToSMTPString()
        {
            var content = new MailContent("Test Mail",  new MailAddress("test@test.de"), "<p>Html Content</p>", "Text Content");

            Assert.Equal("Test Mail", content.Subject);
        }
    }
}