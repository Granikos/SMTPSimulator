using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Granikos.SMTPSimulator.Core;
using Xunit;

namespace Granikos.SMTPSimulator.Test.Core
{
    public class MailContentTest
    {
        [Fact]
        public void TestToSMTPString()
        {
            var content = new MailContent("Test Mail",  new MailAddress("test@test.de", "Display name with Umlauts äöü"), "<p>Html Content</p>", "Text Content");

            Assert.Equal("Test Mail", content.Subject);
        }

        [Fact]
        public void TestToString()
        {
            var subject = "Test Subject with Ümläutß? and =_=";
            var from = new MailAddress("fu@bar.de", "Fu Bar äöü");
            var html = "<p>This is the <b>HTML</b> content.</p>";
            var text = "This is the plain text content. Mit Umlauten: äöüß! = _! =";

            var mc = new MailContent(subject, from, html, text);

            mc.BodyEncoding = Encoding.GetEncoding("ISO-8859-1"); // Encoding.UTF8;
            mc.SubjectEncoding = Encoding.UTF8;
            mc.HeaderEncoding = Encoding.UTF8;

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