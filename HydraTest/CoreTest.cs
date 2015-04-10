using System;
using System.Collections.Generic;
using System.Net;
using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.CommandHandlers.Fakes;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest
{
    public class CoreTest
    {
        [Fact]
        public void TestHandlers()
        {
            var initialized = false;
            SMTPCore actualCore = null;

            var handler = new StubICommandHandler
            {
                InitializeSMTPCore = c =>
                {
                    actualCore = c;
                    initialized = true;
                }
            };

            var loader = new StubICommandHandlerLoader
            {
                GetModules = () => new List<Tuple<string, ICommandHandler>>
                {
                    new Tuple<string, ICommandHandler>("Test", handler)
                }
            };

            var core = new SMTPCore(loader);

            Assert.Same(core, actualCore);
            Assert.True(initialized);

            var actualHandler = core.GetHandler("Test");

            Assert.Same(handler, actualHandler);

            var nonExistant = core.GetHandler("NonExistant");

            Assert.Null(nonExistant);
        }

        [Fact]
        public void TestTriggerNewMessage()
        {
            var core = new SMTPCore(DefaultLoader());

            var expectedTransaction = new StubSMTPTransaction(core);
            var expectedSender = Path.Empty;
            var expectedRecipients = new[] {Path.Postmaster};
            var expectedBody = "Test";

            SMTPTransaction actualTransaction = null;
            Path actualSender = null;
            Path[] actualRecipients = null;
            string actualBody = null;
            var triggered = false;

            core.OnNewMessage += (transaction, sender, recipients, body) =>
            {
                triggered = true;
                actualTransaction = transaction;
                actualSender = sender;
                actualRecipients = recipients;
                actualBody = body;
            };

            core.TriggerNewMessage(expectedTransaction, expectedSender, expectedRecipients, expectedBody);

            Assert.True(triggered);
            Assert.Equal(expectedTransaction, actualTransaction);
            Assert.Equal(expectedSender, actualSender);
            Assert.Equal(expectedRecipients, actualRecipients);
            Assert.Equal(expectedBody, actualBody);
        }

        public ICommandHandlerLoader DefaultLoader()
        {
            return new StubICommandHandlerLoader
            {
                GetModules = () => new List<Tuple<string, ICommandHandler>>()
            };
        }

        [Fact]
        public void TestSetAndGetProperty()
        {
            var core = new SMTPCore(DefaultLoader());

            const int value1 = 5;
            const string value2 = "foo";
            const bool value3 = false;
            var value4 = new object();

            core.SetProperty("foo", value1);
            var actualValue1 = core.GetProperty<int>("foo");
            Assert.Equal(value1, actualValue1);

            core.SetProperty("foo", value2);
            var actualValue2 = core.GetProperty<string>("foo");
            Assert.Equal(value2, actualValue2);

            core.SetProperty("foo", value3);
            var actualValue3 = core.GetProperty<bool>("foo");
            Assert.Equal(value3, actualValue3);

            core.SetProperty("foo", value4);
            var actualValue4 = core.GetProperty<object>("foo");
            Assert.Equal(value4, actualValue4);
        }

        [Fact]
        public void TestGetListProperty()
        {
            var core = new SMTPCore(DefaultLoader());

            var list = core.GetListProperty<string>("foo");

            Assert.NotNull(list);
            Assert.Empty(list);

            list.Add("fubar");

            list = core.GetListProperty<string>("foo");

            Assert.NotNull(list);
            Assert.Contains("fubar", list);

            core.SetProperty("foo", null);

            list = core.GetListProperty<string>("foo");

            Assert.NotNull(list);
            Assert.Empty(list);
        }

        [Fact]
        public void TestHasProperty()
        {
            var core = new SMTPCore(DefaultLoader());

            Assert.False(core.HasProperty("foo"));

            core.SetProperty("foo", 5);
            Assert.True(core.HasProperty("foo"));

            core.SetProperty("foo", null);
            Assert.False(core.HasProperty("foo"));
        }

        [Fact]
        public void TestGetNonExistantProperty()
        {
            var core = new SMTPCore(DefaultLoader());

            Assert.Equal(default(int), core.GetProperty<int>("nonExistant"));
            Assert.Equal(default(bool), core.GetProperty<bool>("nonExistant"));
            Assert.Equal(default(string), core.GetProperty<string>("nonExistant"));
        }

        [Fact]
        public void TestStartTransaction()
        {
            const string banner = "Test Banner";
            const string serverName = "Server Name";
            var expectedGreeting = String.Format("{0} {1}", serverName, banner);

            var core = new SMTPCore(DefaultLoader())
            {
                Banner = banner,
                ServerName = serverName
            };

            var ip = IPAddress.Parse("127.0.0.1");

            using (ShimsContext.Create())
            {
                SMTPCore actualCore = null;

                ShimSMTPTransaction.ConstructorSMTPCore = (smtpTransaction, smtpCore) => { actualCore = smtpCore; };

                SMTPResponse reponse;
                core.StartTransaction(ip, out reponse);

                Assert.Equal(SMTPStatusCode.Ready, reponse.Code);
                Assert.Equal(expectedGreeting, reponse.Args[0]);
                Assert.Same(core, actualCore);
            }
        }

        [Fact]
        public void TestStartTransactionValidationSuccess()
        {
            var core = new SMTPCore(DefaultLoader());

            var ip = IPAddress.Parse("127.0.0.1");

            SMTPTransaction actualTransaction = null;
            IPAddress actualIP = null;
            core.OnConnect += (transaction, args) =>
            {
                actualTransaction = transaction;
                actualIP = args.IP;
            };

            SMTPResponse reponse;
            var expectedTransaction = core.StartTransaction(ip, out reponse);

            Assert.Equal(SMTPStatusCode.Ready, reponse.Code);
            Assert.Same(expectedTransaction, actualTransaction);
            Assert.Same(ip, actualIP);
        }

        [Fact]
        public void TestStartTransactionValidationFail()
        {
            var core = new SMTPCore(DefaultLoader());

            var ip = IPAddress.Parse("127.0.0.1");

            using (ShimsContext.Create())
            {
                var closed = false;
                ShimSMTPTransaction.AllInstances.Close = transaction => { closed = true; };

                core.OnConnect += (transaction, args) => { args.Cancel = true; };

                SMTPResponse reponse;
                core.StartTransaction(ip, out reponse);

                Assert.Equal(SMTPStatusCode.TransactionFailed, reponse.Code);
                Assert.True(closed);
            }
        }
    }
}