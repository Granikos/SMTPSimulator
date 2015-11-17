using System.Net.Mail;
using System.Net.Mime;
using System.Text;
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

        [Fact]
        public void TestToString()
        {
            var subject = "Test Subject with Ümläutß";
            var from = new MailAddress("fu@bar.de", "Fu Bar");
            var html = "<p>This is the <b>HTML</b> content.</p>";
            var text = "This is the plain text content. Mit Umlauten: äöüß! \u00E4 =";
            text = "Hätten Hüte ein ß im Namen, wären sie möglicherweise keine Hüte mehr,\r\nsondern Hüße.";

            var mc = new MailContent(subject, from, html, text);

            mc.BodyEncoding = Encoding.GetEncoding("ISO-8859-1"); // Encoding.UTF8;
            mc.SubjectEncoding = Encoding.UTF8;

            for (int i = 0; i < 10; i++)
            {
                mc.AddRecipient("user" + i + "@test.de", "User " + i);
            }

            mc.AddAttachment("fu.txt", "Test Anhang", Encoding.ASCII, MediaTypeNames.Text.Plain);

            var s = mc.ToString();

            Assert.Contains("Test Subject", s);
        }
    }
}