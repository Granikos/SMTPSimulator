using System;
using System.Text;
using HydraCore;
using HydraCore.CommandHandlers.Fakes;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest
{
    public class TransactionTest
    {
        [Fact]
        public void TestConstructor()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();

                var transaction = new SMTPTransaction(server);

                Assert.Same(server.Instance, transaction.Server);
                Assert.False(transaction.InDataMode);
                Assert.False(transaction.Initialized);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestSetAndGetProperty(bool permanent)
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                const int value1 = 5;
                const string value2 = "foo";
                const bool value3 = false;
                var value4 = new object();

                transaction.SetProperty("foo", value1, permanent);
                var actualValue1 = transaction.GetProperty<int>("foo");
                Assert.Equal(value1, actualValue1);

                transaction.SetProperty("foo", value2, permanent);
                var actualValue2 = transaction.GetProperty<string>("foo");
                Assert.Equal(value2, actualValue2);

                transaction.SetProperty("foo", value3, permanent);
                var actualValue3 = transaction.GetProperty<bool>("foo");
                Assert.Equal(value3, actualValue3);

                transaction.SetProperty("foo", value4, permanent);
                var actualValue4 = transaction.GetProperty<object>("foo");
                Assert.Equal(value4, actualValue4);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestGetListProperty(bool permanent)
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                var list = transaction.GetListProperty<string>("foo", permanent);

                Assert.NotNull(list);
                Assert.Empty(list);

                list.Add("fubar");

                list = transaction.GetListProperty<string>("foo", permanent);

                Assert.NotNull(list);
                Assert.Contains("fubar", list);

                transaction.SetProperty("foo", null, permanent);

                list = transaction.GetListProperty<string>("foo", permanent);

                Assert.NotNull(list);
                Assert.Empty(list);
            }
        }

        [Fact]
        public void TestOvershadowingProperties()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                const string value1 = "bar";
                const string value2 = "baz";

                transaction.SetProperty("foo", value1, true);
                Assert.Equal(value1, transaction.GetProperty<string>("foo"));

                transaction.SetProperty("foo", value2, false);
                Assert.Equal(value2, transaction.GetProperty<string>("foo"));

                transaction.SetProperty("foo", null, false);
                Assert.Equal(value1, transaction.GetProperty<string>("foo"));

                transaction.SetProperty("foo", null, true);
                Assert.Equal(default(string), transaction.GetProperty<string>("foo"));
            }
        }

        [Fact]
        public void TestHasProperty()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                Assert.False(transaction.HasProperty("foo"));

                transaction.SetProperty("foo", 5, false);
                Assert.True(transaction.HasProperty("foo"));

                transaction.SetProperty("foo", null, false);
                Assert.False(transaction.HasProperty("foo"));

                transaction.SetProperty("foo", 5, true);
                Assert.True(transaction.HasProperty("foo"));

                transaction.SetProperty("foo", null, true);
                Assert.False(transaction.HasProperty("foo"));
            }
        }

        [Fact]
        public void TestGetNonExistantProperty()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                Assert.Equal(default(int), transaction.GetProperty<int>("nonExistant"));
                Assert.Equal(default(bool), transaction.GetProperty<bool>("nonExistant"));
                Assert.Equal(default(string), transaction.GetProperty<string>("nonExistant"));
            }
        }

        [Fact]
        public void TestResetProperties()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                transaction.SetProperty("foo", "bar", false);
                transaction.SetProperty("foo2", "baz", true);
                Assert.True(transaction.HasProperty("foo"));
                Assert.Equal("bar", transaction.GetProperty<string>("foo"));
                Assert.True(transaction.HasProperty("foo2"));
                Assert.Equal("baz", transaction.GetProperty<string>("foo2"));

                transaction.Reset();
                Assert.False(transaction.HasProperty("foo"));
                Assert.Equal(default(string), transaction.GetProperty<string>("foo"));
                Assert.True(transaction.HasProperty("foo2"));
                Assert.Equal("baz", transaction.GetProperty<string>("foo2"));
            }
        }

        [Fact]
        public void TestResetDataMode()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                Assert.False(transaction.InDataMode);

                transaction.StartDataMode((s, builder) => false, s => new SMTPResponse(SMTPStatusCode.Okay));
                Assert.True(transaction.InDataMode);

                transaction.Reset();
                Assert.False(transaction.InDataMode);
            }
        }

        [Fact]
        public void TestNonDataModeError()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                Assert.False(transaction.InDataMode);
                Assert.Throws<InvalidOperationException>(() => transaction.HandleData(""));
                Assert.Throws<InvalidOperationException>(() => transaction.HandleDataLine("", new StringBuilder()));
            }
        }

        [Fact]
        public void TestClose()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                var closed = false;

                transaction.OnClose += smtpTransaction => closed = true;

                Assert.False(transaction.Closed);

                transaction.Close();

                Assert.True(transaction.Closed);
                Assert.True(closed);
            }
        }

        [Fact]
        public void TestInitialize()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                const string clientId = "ClientID";

                Assert.False(transaction.Initialized);

                transaction.Initialize(clientId);

                Assert.True(transaction.Initialized);
                Assert.Equal(clientId, transaction.ClientIdentifier);
            }
        }

        [Fact]
        public void TestExecuteSuccess()
        {
            const string command = "Test";
            var expectedResponse = new SMTPResponse(SMTPStatusCode.NotAvailiable, "Fu", "bar");
            const string expectedParams = "Fubar blubb";

            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                SMTPTransaction actualTransaction = null;
                string actualParams = null;

                var handler = new StubICommandHandler
                {
                    ExecuteSMTPTransactionString = (smtpTransaction, s) =>
                    {
                        actualTransaction = smtpTransaction;
                        actualParams = s;

                        return expectedResponse;
                    }
                };

                server.GetHandlerString = s =>
                {
                    if (s == command) return handler;
                    throw new InvalidOperationException("Invalid name.");
                };

                var response = transaction.ExecuteCommand(new SMTPCommand(command, expectedParams));

                Assert.Same(expectedResponse, response);
                Assert.Equal(expectedParams, actualParams);
                Assert.Same(transaction, actualTransaction);
            }
        }

        [Fact]
        public void TestExecuteFail()
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                server.GetHandlerString = s => null;

                var response = transaction.ExecuteCommand(new SMTPCommand("NonExistentCommand", "Params"));

                Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestDataHandling(bool expectedResult)
        {
            using (ShimsContext.Create())
            {
                var server = new ShimSMTPCore();
                var transaction = new SMTPTransaction(server);

                Assert.False(transaction.InDataMode);

                const string expectedData = "Some data";
                string actualData = null;
                const string expectedLine = "Some line";
                string actualLine = null;
                var expectedStringBuilder = new StringBuilder();
                StringBuilder actualStringBuilder = null;
                var expectedResponse = new SMTPResponse(SMTPStatusCode.Okay);

                transaction.StartDataMode((line, builder) =>
                {
                    actualLine = line;
                    actualStringBuilder = builder;
                    return expectedResult;
                }, data =>
                {
                    actualData = data;
                    return expectedResponse;
                });

                var actualResult = transaction.HandleDataLine(expectedLine, expectedStringBuilder);
                var actualResponse = transaction.HandleData(expectedData);

                Assert.Equal(expectedResult, actualResult);
                Assert.Same(expectedResponse, actualResponse);
                Assert.Same(expectedStringBuilder, actualStringBuilder);
                Assert.Equal(expectedData, actualData);
                Assert.Equal(expectedLine, actualLine);
            }
        }
    }
}