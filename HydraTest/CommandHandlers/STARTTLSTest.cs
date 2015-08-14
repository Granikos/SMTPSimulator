using System;
using System.Collections.Generic;
using System.Linq;
using HydraCore;
using HydraCore.AuthMethods;
using HydraCore.AuthMethods.Fakes;
using HydraCore.CommandHandlers;
using HydraCore.CommandHandlers.Fakes;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class STARTTLSTest : TestBase
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        public void TestInitialize(bool TLSEnabled, bool TLSActive, bool inEHLO)
        {
            using (ShimsContext.Create())
            {
                var ehloLines = new List<Func<SMTPTransaction, string>>();
                AddCoreListProperty("EHLOLines", () => ehloLines);

                Transaction.TLSActiveGet = () => TLSActive;
                Transaction.SettingsGet = () => new StubIReceiveSettings
                {
                    EnableTLSGet = () => TLSEnabled
                };

                var handler = new STARTTLSHandler();
                handler.Initialize(Core);

                if (inEHLO)
                {
                    Assert.Contains("STARTTLS", ehloLines.Select(e => e(Transaction)));
                }
                else
                {
                    Assert.DoesNotContain("STARTTLS", ehloLines.Select(e => e(Transaction)));
                }
            }
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, true, true, false)]
        [InlineData(false, true, true, false)]
        [InlineData(false, false, true, false)]
        [InlineData(true, false, true, false)]
        [InlineData(false, true, false, true)]
        public void TestOnCommandExecute(bool unsecuredAllowed, bool requireEncryption, bool isSecured, bool error)
        {
            using (ShimsContext.Create())
            {
                Transaction.SettingsGet = () => new StubIReceiveSettings
                {
                    RequireTLSGet = () => requireEncryption
                };

                Transaction.TLSActiveGet = () => isSecured;

                ICommandHandler otherHandler = unsecuredAllowed
                    ? (ICommandHandler)new HandlerWithUnsecureAllowed()
                    : new StubICommandHandler();

                var handler = new STARTTLSHandler();
                var args = new CommandExecuteEventArgs(Transaction, otherHandler, "");
                handler.OnCommandExecute(otherHandler, args);

                if (error)
                {
                    Assert.NotNull(args.Response);
                    Assert.Equal(SMTPStatusCode.AuthRequired, args.Response.Code);
                }
                else
                {
                    Assert.Null(args.Response);
                }
            }
        }


        [Theory]
        [InlineData(false, false, "", SMTPStatusCode.BadSequence)]
        [InlineData(false, true, "", SMTPStatusCode.BadSequence)]
        [InlineData(true, true, "", SMTPStatusCode.BadSequence)]
        [InlineData(false, false, "non empty", SMTPStatusCode.SyntaxError)]
        [InlineData(false, true, "non empty", SMTPStatusCode.SyntaxError)]
        [InlineData(true, false, "non empty", SMTPStatusCode.SyntaxError)]
        [InlineData(true, true, "non empty", SMTPStatusCode.SyntaxError)]
        public void TestDoExecuteError(bool enableTLS, bool isSecured, string parameters, SMTPStatusCode code)
        {
            using (ShimsContext.Create())
            {
                Transaction.SettingsGet = () => new StubIReceiveSettings
                {
                    EnableTLSGet = () => enableTLS
                };
                Transaction.TLSActiveGet = () => isSecured;

                bool started = false;
                Transaction.StartTLSCancelEventArgs = args =>
                {
                    started = true;
                };

                var handler = new STARTTLSHandler();
                var response = handler.DoExecute(Transaction, parameters);

                Assert.Equal(code, response.Code);
                Assert.False(started);
            }
        }

        [Fact]
        public void TestDoExecuteSuccess()
        {
            using (ShimsContext.Create())
            {
                Transaction.SettingsGet = () => new StubIReceiveSettings
                {
                    EnableTLSGet = () => true
                };
                Transaction.TLSActiveGet = () => false;

                bool started = false;
                Transaction.StartTLSCancelEventArgs = args =>
                {
                    started = true;
                };

                var handler = new STARTTLSHandler();
                var response = handler.DoExecute(Transaction, "");

                Assert.Equal(SMTPStatusCode.Ready, response.Code);
                Assert.True(started);
            }
        }

        [Fact]
        public void TestDoExecuteCancel()
        {
            using (ShimsContext.Create())
            {
                Transaction.SettingsGet = () => new StubIReceiveSettings
                {
                    EnableTLSGet = () => true
                };
                Transaction.TLSActiveGet = () => false;

                bool called = false;
                Transaction.StartTLSCancelEventArgs = args =>
                {
                    called = true;
                    args.Cancel = true;
                };

                var handler = new STARTTLSHandler();
                var response = handler.DoExecute(Transaction, "");

                Assert.Equal(SMTPStatusCode.TLSNotAvailiable, response.Code);
                Assert.True(called);
            }
        }

        [UnsecureAllowed]
        private class HandlerWithUnsecureAllowed : ICommandHandler
        {
            public void Initialize(SMTPCore core)
            {
                throw new NotImplementedException();
            }

            public SMTPResponse Execute(SMTPTransaction transaction, string parameters)
            {
                throw new NotImplementedException();
            }
        }
    }
}