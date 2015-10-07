using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Granikos.Hydra.Core;
using Granikos.Hydra.SmtpServer;
using Granikos.Hydra.SmtpServer.AuthMethods;
using Granikos.Hydra.SmtpServer.AuthMethods.Fakes;
using Granikos.Hydra.SmtpServer.CommandHandlers;
using Granikos.Hydra.SmtpServer.CommandHandlers.Fakes;
using Granikos.Hydra.SmtpServer.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class AUTHTest : TestBase
    {
        [Fact]
        public void TestInitialize()
        {
            using (ShimsContext.Create())
            {
                var ehloLines = new List<Func<SMTPTransaction, string>>();
                AddCoreListProperty("EHLOLines", () => ehloLines);

                var loader = new StubIAuthMethodLoader
                {
                    GetModules = () => new List<Tuple<string, IAuthMethod>>
                    {
                        new Tuple<string, IAuthMethod>("Test", new StubIAuthMethod()),
                        new Tuple<string, IAuthMethod>("Something", new StubIAuthMethod())
                    }
                };
                var handler = new AUTHHandler(loader);
                handler.Initialize(Core);

                Assert.Contains("AUTH TEST SOMETHING", ehloLines.Select(e => e(Transaction)));
            }
        }

        private IAuthMethodLoader GetDefaulLoader()
        {
            return new StubIAuthMethodLoader
            {
                GetModules = () => new List<Tuple<string, IAuthMethod>>()
            };
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public void TestHandlerRequiresAuth(bool actionRequiresAuth, bool transactionRequiresAuth, bool authenticated)
        {
            using (ShimsContext.Create())
            {
                Transaction.SettingsGet = () => new StubIReceiveSettings
                {
                    RequireAuthGet = () => transactionRequiresAuth
                };

                AddTransactionProperty("Authenticated", authenticated);

                var handler = new AUTHHandler(GetDefaulLoader());
                var otherHandler = actionRequiresAuth
                    ? (ICommandHandler) new HandlerWithRequiresAuth()
                    : new StubICommandHandler();
                var args = new CommandExecuteEventArgs(Transaction, otherHandler, "");
                handler.OnCommandExecute(otherHandler, args);

                if (actionRequiresAuth && transactionRequiresAuth && !authenticated)
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
        [InlineData("")]
        [InlineData("fubar")]
        public void TestDataLineHandler(string line)
        {
            using (ShimsContext.Create())
            {
                var sb = new StringBuilder();
                var readMore = AUTHHandler.DataLineHandler(line, sb);

                Assert.False(readMore);
                Assert.Equal(line, sb.ToString());
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("fubar")]
        [InlineData("*")]
        [InlineData("=")]
        public void TestDataHandler(string line)
        {
            using (ShimsContext.Create())
            {
                SMTPTransaction actualTransaction = null;
                string decodedReponse = null;
                IAuthMethod actualAuthMethod = null;
                var expectedResponse = new SMTPResponse(SMTPStatusCode.Okay);

                ShimAUTHHandler.HandleResponseSMTPTransactionStringIAuthMethod = (transaction, data, authMethod) =>
                {
                    actualTransaction = transaction;
                    decodedReponse = data;
                    actualAuthMethod = authMethod;

                    return expectedResponse;
                };

                var method = new StubIAuthMethod();
                var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(line));

                var response = AUTHHandler.DataHandler(Transaction, encoded, method);

                Assert.Same(method, actualAuthMethod);
                Assert.Same(expectedResponse, response);
                Assert.Equal(Transaction, actualTransaction);
                Assert.Equal(line, decodedReponse);
            }
        }

        [Fact]
        public void TestDataHandlerAbort()
        {
            using (ShimsContext.Create())
            {
                var aborted = false;
                SMTPTransaction actualTransaction = null;

                var authMethod = new StubIAuthMethod
                {
                    AbortSMTPTransaction = transaction =>
                    {
                        actualTransaction = transaction;
                        aborted = true;
                    }
                };

                var response = AUTHHandler.DataHandler(Transaction, "*", authMethod);

                Assert.Equal(SMTPStatusCode.ParamError, response.Code);
                Assert.True(aborted);
                Assert.Equal(Transaction, actualTransaction);
            }
        }

        [Theory]
        [InlineData("=")]
        [InlineData("fubar")]
        [InlineData("xy=")]
        public void TestDataHandlerError(string line)
        {
            using (ShimsContext.Create())
            {
                var response = AUTHHandler.DataHandler(Transaction, line, new StubIAuthMethod());

                Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
            }
        }

        [Theory]
        [InlineData("fubar")]
        [InlineData(null)]
        [InlineData("")]
        public void TestHandleResponseFailure(string errorMessage)
        {
            const string expectedData = "Test Data";
            using (ShimsContext.Create())
            {
                SMTPTransaction actualTransaction = null;
                string decodedReponse = null;
                var authMethod = new StubIAuthMethod
                {
                    ProcessResponseSMTPTransactionStringStringOut =
                        (SMTPTransaction transaction, string data, out string c) =>
                        {
                            actualTransaction = transaction;
                            decodedReponse = data;

                            c = errorMessage;
                            return false;
                        }
                };
                var response = AUTHHandler.HandleResponse(Transaction, expectedData, authMethod);

                Assert.Equal(SMTPStatusCode.AuthFailed, response.Code);
                if (errorMessage != null)
                {
                    Assert.Equal(1, response.Args.Length);
                    Assert.Equal(errorMessage, response.Args[0]);
                }
                Assert.Equal(Transaction, actualTransaction);
                Assert.Equal(expectedData, decodedReponse);
            }
        }

        [Fact]
        public void TestHandleResponseSuccess()
        {
            const string expectedData = "Test Data";
            SMTPTransaction actualTransaction = null;
            string decodedReponse = null;
            var authenticated = false;
            var permanent = false;

            using (ShimsContext.Create())
            {
                Transaction.SetPropertyStringObjectBoolean = (name, value, p) =>
                {
                    switch (name)
                    {
                        case "Authenticated":
                            authenticated = (bool) value;
                            permanent = p;
                            break;
                        default:
                            throw new InvalidOperationException("The name is invalid.");
                    }
                };

                var authMethod = new StubIAuthMethod
                {
                    ProcessResponseSMTPTransactionStringStringOut =
                        (SMTPTransaction transaction, string data, out string challenge) =>
                        {
                            actualTransaction = transaction;
                            decodedReponse = data;

                            challenge = null;
                            return true;
                        }
                };

                var response = AUTHHandler.HandleResponse(Transaction, expectedData, authMethod);

                Assert.Equal(SMTPStatusCode.AuthSuccess, response.Code);
                Assert.Equal(Transaction, actualTransaction);
                Assert.Equal(expectedData, decodedReponse);
                Assert.True(authenticated);
                Assert.True(permanent);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("fu bar test")]
        [InlineData("test abc")]
        public void TestExecuteSyntaxError(string parameters)
        {
            using (ShimsContext.Create())
            {
                AddTransactionProperty("Authenticated", false);
                AddCoreListProperty("EHLOLines", new List<Func<SMTPTransaction, string>>());

                var method = new StubIAuthMethod();
                var loader = new StubIAuthMethodLoader
                {
                    GetModules =
                        () => new List<Tuple<string, IAuthMethod>> {new Tuple<string, IAuthMethod>("TEST", method)}
                };

                var handler = new AUTHHandler(loader);
                handler.Initialize(Core);
                var response = handler.DoExecute(Transaction, parameters);

                Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
            }
        }

        [Fact]
        public void TestExecuteBadSequenceError()
        {
            using (ShimsContext.Create())
            {
                AddTransactionProperty("Authenticated", true);

                var handler = new AUTHHandler(GetDefaulLoader());
                var response = handler.DoExecute(Transaction, "Test =");

                Assert.Equal(SMTPStatusCode.BadSequence, response.Code);
            }
        }

        [Fact]
        public void TestExecuteNotImplementedError()
        {
            using (ShimsContext.Create())
            {
                AddTransactionProperty("Authenticated", false);

                var handler = new AUTHHandler(GetDefaulLoader());
                var response = handler.DoExecute(Transaction, "NonExisting");

                Assert.Equal(SMTPStatusCode.ParamNotImplemented, response.Code);
            }
        }

        [Theory]
        [InlineData("TEST", null)]
        [InlineData("Test =", "")]
        [InlineData("test dGVzdA==", "test")]
        public void TestExecutSuccess(string parameters, string expectedDecoded)
        {
            using (ShimsContext.Create())
            {
                AddTransactionProperty("Authenticated", false);
                AddCoreListProperty("EHLOLines", new List<Func<SMTPTransaction, string>>());

                SMTPTransaction actualTransaction = null;
                string decodedReponse = null;
                IAuthMethod actualAuthMethod = null;
                var expectedResponse = new SMTPResponse(SMTPStatusCode.Okay);

                ShimAUTHHandler.HandleResponseSMTPTransactionStringIAuthMethod = (transaction, data, authMethod) =>
                {
                    actualTransaction = transaction;
                    decodedReponse = data;
                    actualAuthMethod = authMethod;

                    return expectedResponse;
                };

                var method = new StubIAuthMethod();
                var loader = new StubIAuthMethodLoader
                {
                    GetModules =
                        () => new List<Tuple<string, IAuthMethod>> {new Tuple<string, IAuthMethod>("TEST", method)}
                };

                var handler = new AUTHHandler(loader);
                handler.Initialize(Core);
                var response = handler.DoExecute(Transaction, parameters);

                Assert.Same(method, actualAuthMethod);
                Assert.Same(expectedResponse, response);
                Assert.Equal(Transaction, actualTransaction);
                Assert.Equal(expectedDecoded, decodedReponse);
            }
        }

        [Fact]
        public void TestHandleResponseChallenge()
        {
            const string expectedChallenge = "Test";
            const string expectedData = "Test Data";
            var encodedChallenge = Convert.ToBase64String(Encoding.ASCII.GetBytes(expectedChallenge));

            var dataMode = false;
            var dataHandlerCalled = false;
            var dataLineHandlerCalled = false;
            Func<string, StringBuilder, bool> dataLineHandler = null;
            Func<string, SMTPResponse> dataHandler = null;

            using (ShimsContext.Create())
            {
                Transaction.StartDataModeFuncOfStringStringBuilderBooleanFuncOfStringSMTPResponse = (func, func1) =>
                {
                    dataMode = true;
                    dataLineHandler = func;
                    dataHandler = func1;
                };

                ShimAUTHHandler.DataHandlerSMTPTransactionStringIAuthMethod = (transaction, s, arg3) =>
                {
                    dataHandlerCalled = true;
                    return null;
                };

                ShimAUTHHandler.DataLineHandlerStringStringBuilder = (s, builder) =>
                {
                    dataLineHandlerCalled = true;
                    return false;
                };

                SMTPTransaction actualTransaction = null;
                string decodedReponse = null;
                var authMethod = new StubIAuthMethod
                {
                    ProcessResponseSMTPTransactionStringStringOut =
                        (SMTPTransaction transaction, string data, out string challenge) =>
                        {
                            actualTransaction = transaction;
                            decodedReponse = data;

                            challenge = expectedChallenge;
                            return true;
                        }
                };
                var response = AUTHHandler.HandleResponse(Transaction, expectedData, authMethod);

                if (dataHandler != null) dataHandler(null);
                if (dataLineHandler != null) dataLineHandler(null, null);

                Assert.Equal(SMTPStatusCode.AuthContinue, response.Code);
                Assert.Equal(1, response.Args.Length);
                Assert.Equal(encodedChallenge, response.Args[0]);
                Assert.Equal(Transaction, actualTransaction);
                Assert.Equal(expectedData, decodedReponse);
                Assert.True(dataMode);
                Assert.True(dataHandlerCalled);
                Assert.True(dataLineHandlerCalled);
            }
        }

        [RequiresAuth]
        private class HandlerWithRequiresAuth : ICommandHandler
        {
            public void Initialize(SMTPServer server)
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