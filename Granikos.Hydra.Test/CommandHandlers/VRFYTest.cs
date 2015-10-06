using System.Collections.Generic;
using System.Linq;
using Granikos.Hydra.Core;
using Granikos.Hydra.Core.Fakes;
using Granikos.Hydra.SmtpServer.CommandHandlers;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class VRFYTest : TestBase
    {
        [Theory]
        [InlineData("test", new[] {"test@test.de"}, new[] {true})]
        [InlineData("test", new[] {"test@test.de", "test2@test.de"}, new[] {true, true})]
        [InlineData("test", new[] {"test@test.de", "fubar@fubar.de"}, new[] {true, false})]
        public void TestSuccess(string search, string[] emails, bool[] found)
        {
            var mailboxes = emails.Select(e => (Mailbox) new ShimMailbox
            {
                ToString = () => e
            }).ToList();

            AddCoreListProperty("Mailboxes", () => mailboxes);

            var handler = new VRFYHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, search);
            var foundCount = found.Count(f => f);

            Assert.Equal(foundCount > 1 ? SMTPStatusCode.MailboxNameNotAllowed : SMTPStatusCode.Okay, response.Code);
            Assert.Equal(response.Args.Length, foundCount);

            for (var i = 0; i < emails.Length; i++)
            {
                if (found[i])
                {
                    Assert.Contains(emails[i], response.Args);
                }
                else
                {
                    Assert.DoesNotContain(emails[i], response.Args);
                }
            }
        }

        [Fact]
        public void TestNotFound()
        {
            var handler = new VRFYHandler();
            handler.Initialize(Core);

            AddCoreListProperty("Mailboxes", new List<Mailbox>());

            var response = handler.Execute(Transaction, "test@test.de");
            Assert.Equal(SMTPStatusCode.MailboxUnavailiableError, response.Code);
        }

        [Fact]
        public void TestErrorWithNoParams()
        {
            var handler = new VRFYHandler();
            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "");
            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}