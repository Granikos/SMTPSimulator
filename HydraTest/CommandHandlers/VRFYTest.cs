using System.Collections.Generic;
using System.Linq;
using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class VRFYTest
    {
        [Theory]
        [InlineData("test", new[] { "test@test.de" }, new[] { true })]
        [InlineData("test", new[] { "test@test.de", "test2@test.de" }, new[] { true, true })]
        [InlineData("test", new[] { "test@test.de", "fubar@fubar.de" }, new[] { true, false })]
        public void TestSuccess(string search, string[] emails, bool[] found)
        {
            using (ShimsContext.Create())
            {
                var mailboxes = emails.Select(e => (Mailbox)new ShimMailbox
                {
                    ToString = () => e
                }).ToList();

                var handler = new VRFYHandler();

                var server = new ShimSMTPCore
                {
                    MailboxesGet = () => mailboxes
                };

                var transaction = new ShimSMTPTransaction();

                handler.Initialize(server);

                var response = handler.Execute(transaction, search);
                var foundCount = found.Count(f => f);

                Assert.Equal(foundCount > 1? SMTPStatusCode.MailboxNameNotAllowed : SMTPStatusCode.Okay, response.Code);
                Assert.Equal(response.Args.Length, foundCount);

                for (int i = 0; i < emails.Length; i++)
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
        }

        [Fact]
        public void TestNotFound()
        {
            using (ShimsContext.Create())
            {
                var handler = new VRFYHandler();

                var server = new ShimSMTPCore
                {
                    MailboxesGet = () => new List<Mailbox>()
                };

                var transaction = new ShimSMTPTransaction();

                handler.Initialize(server);

                var response = handler.Execute(transaction, "test@test.de");
                Assert.Equal(SMTPStatusCode.MailboxUnavailiableError, response.Code);
            }
        }

        [Fact]
        public void TestErrorWithNoParams()
        {
            using (ShimsContext.Create())
            {
                var handler = new VRFYHandler();

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction();

                handler.Initialize(server);

                var response = handler.Execute(transaction, "");
                Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
            }
        }
    }
}