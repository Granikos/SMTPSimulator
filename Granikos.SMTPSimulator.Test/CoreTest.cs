﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.SmtpServer;
using Granikos.SMTPSimulator.SmtpServer.CommandHandlers;
using Granikos.SMTPSimulator.SmtpServer.CommandHandlers.Fakes;
using Granikos.SMTPSimulator.SmtpServer.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace SMTPSimulatorTest
{
    public class CoreTest
    {
        [Fact]
        public void TestHandlers()
        {
            var initialized = false;
            SMTPServer actualCore = null;

            var handler = new StubICommandHandler
            {
                InitializeSMTPServer = c =>
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

            var core = new SMTPServer(loader);

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
            var core = new SMTPServer(DefaultLoader());
            var settings = new StubIReceiveSettings();

            var expectedTransaction = new StubSMTPTransaction(core, settings);
            var expectedSender = new MailPath("tester", "test.de");
            var expectedRecipients = new[] {new MailPath("fubar", "fu.com")};
            var expectedBody = "Test";

            SMTPTransaction actualTransaction = null;
            Mail actualMail = null;
            var triggered = false;

            core.OnNewMessage += (transaction, mail) =>
            {
                triggered = true;
                actualTransaction = transaction;
                actualMail = mail;
            };

            core.TriggerNewMessage(expectedTransaction, expectedSender, expectedRecipients, expectedBody);

            // TODO: Remove dependencies of test

            Assert.True(triggered);
            Assert.Equal(expectedTransaction, actualTransaction);
            Assert.Equal(expectedSender.ToMailAdress(), actualMail.From);
            Assert.Equal(expectedRecipients.Select(r => r.ToMailAdress().ToString()).ToArray(),
                actualMail.Recipients.Select(r => r.ToString()).ToArray());
            Assert.Equal(expectedBody, actualMail.Body);
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
            var core = new SMTPServer(DefaultLoader());

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
            var core = new SMTPServer(DefaultLoader());

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
            var core = new SMTPServer(DefaultLoader());

            Assert.False(core.HasProperty("foo"));

            core.SetProperty("foo", 5);
            Assert.True(core.HasProperty("foo"));

            core.SetProperty("foo", null);
            Assert.False(core.HasProperty("foo"));
        }

        [Fact]
        public void TestGetNonExistantProperty()
        {
            var core = new SMTPServer(DefaultLoader());

            Assert.Equal(default(int), core.GetProperty<int>("nonExistant"));
            Assert.Equal(default(bool), core.GetProperty<bool>("nonExistant"));
            Assert.Equal(default(string), core.GetProperty<string>("nonExistant"));
        }

        [Fact]
        public void TestStartTransaction()
        {
            const string banner = "Test Banner";
            IReceiveSettings actualSettings = null;

            var core = new SMTPServer(DefaultLoader());

            var ip = IPAddress.Parse("127.0.0.1");

            using (ShimsContext.Create())
            {
                var expectedSettings = new StubIReceiveSettings
                {
                    BannerGet = () => banner
                };

                SMTPServer actualCore = null;

                ShimSMTPTransaction.ConstructorSMTPServerIReceiveSettings = (transaction, smtpCore, settings) =>
                {
                    actualSettings = settings;
                    actualCore = smtpCore;
                };

                SMTPResponse reponse;
                core.StartTransaction(ip, expectedSettings, out reponse);

                Assert.Equal(SMTPStatusCode.Ready, reponse.Code);
                Assert.Equal(banner, reponse.Args[0]);
                Assert.Same(core, actualCore);
                Assert.Same(expectedSettings, actualSettings);
            }
        }

        [Fact]
        public void TestStartTransactionValidationSuccess()
        {
            var core = new SMTPServer(DefaultLoader());

            var ip = IPAddress.Parse("127.0.0.1");

            SMTPTransaction actualTransaction = null;
            IPAddress actualIP = null;
            core.OnConnect += (transaction, args) =>
            {
                actualTransaction = transaction;
                actualIP = args.IP;
            };

            SMTPTransaction expectedTransaction;
            SMTPResponse reponse;
            using (ShimsContext.Create())
            {
                var settings = new StubIReceiveSettings();

                expectedTransaction = core.StartTransaction(ip, settings, out reponse);
            }

            Assert.Equal(SMTPStatusCode.Ready, reponse.Code);
            Assert.Same(expectedTransaction, actualTransaction);
            Assert.Same(ip, actualIP);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(SMTPStatusCode.AuthFailed)]
        public void TestStartTransactionValidationFail(SMTPStatusCode? code)
        {
            var core = new SMTPServer(DefaultLoader());

            var ip = IPAddress.Parse("127.0.0.1");

            using (ShimsContext.Create())
            {
                var closed = false;
                ShimSMTPTransaction.AllInstances.Close = transaction => { closed = true; };

                core.OnConnect += (transaction, args) =>
                {
                    args.Cancel = true;
                    args.ResponseCode = code;
                };

                SMTPResponse reponse;
                using (ShimsContext.Create())
                {
                    var settings = new StubIReceiveSettings();

                    core.StartTransaction(ip, settings, out reponse);
                }

                Assert.Equal(code ?? SMTPStatusCode.TransactionFailed, reponse.Code);
                Assert.True(closed);
            }
        }
    }
}