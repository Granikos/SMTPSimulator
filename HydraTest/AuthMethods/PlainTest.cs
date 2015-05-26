using System;
using HydraCore.AuthMethods;
using HydraTest.CommandHandlers;
using Xunit;

namespace HydraTest.AuthMethods
{
    public class PlainTest : TestBase
    {
        [Fact]
        public void TestProcessSuccess()
        {
            const string expectedUsername = "fubar";
            const string expectedPassword = "bla";

            var method = new PlainAuthMethod();

            string username = null;
            string password = null;
            var usernamePermanent = false;
            var passwordPermanent = false;

            Transaction.SetPropertyStringObjectBoolean = (s, v, p) =>
            {
                switch (s)
                {
                    case "Username":
                        username = (string) v;
                        usernamePermanent = p;
                        break;
                    case "Password":
                        password = (string) v;
                        passwordPermanent = p;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            };

            var token = string.Format("\0{0}\0{1}", expectedUsername, expectedPassword);
            string challenge;
            var result = method.ProcessResponse(Transaction, token, out challenge);

            Assert.True(result);
            Assert.Null(challenge);
            Assert.Equal(expectedUsername, username);
            Assert.Equal(expectedPassword, password);
            Assert.True(usernamePermanent);
            Assert.True(passwordPermanent);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("a b c")]
        public void TestProcessError(string input)
        {
            var method = new PlainAuthMethod();

            string challenge;
            var result = method.ProcessResponse(Transaction, input, out challenge);

            Assert.False(result);
            Assert.Null(challenge);
        }

        [Fact]
        public void TestAbort()
        {
            var method = new PlainAuthMethod();

            var usernameReset = false;
            var passwordReset = false;

            Transaction.SetPropertyStringObjectBoolean = (s, v, p) =>
            {
                switch (s)
                {
                    case "Username":
                        usernameReset = v == null && p;
                        break;
                    case "Password":
                        passwordReset = v == null && p;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            };

            method.Abort(Transaction);

            Assert.True(usernameReset);
            Assert.True(passwordReset);
        }
    }
}