using System;
using HydraCore.AuthMethods;
using HydraTest.CommandHandlers;
using Xunit;

namespace HydraTest.AuthMethods
{
    public class LoginTest : TestBase
    {
        [Fact]
        public void TestProcessInitial()
        {
            var method = new LoginAuthMethod();

            string challenge;
            var result = method.ProcessResponse(Transaction, null, out challenge);

            Assert.True(result);
            Assert.Equal("Username:", challenge);
        }

        [Fact]
        public void TestProcessUsername()
        {
            const string expectedUsername = "fubar";
            var method = new LoginAuthMethod();

            Transaction.HasPropertyString = s =>
            {
                switch (s)
                {
                    case "Username":
                        return false;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            };

            string username = null;
            bool permanent = false;

            Transaction.SetPropertyStringObjectBoolean = (s, v, p) =>
            {
                switch (s)
                {
                    case "Username":
                        username = (string) v;
                        permanent = p;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            };

            string challenge;
            var result = method.ProcessResponse(Transaction, expectedUsername, out challenge);

            Assert.True(result);
            Assert.Equal("Password:", challenge);
            Assert.Equal(expectedUsername, username);
            Assert.True(permanent);
        }

        [Fact]
        public void TestProcessPassword()
        {
            const string expectedPassword = "fubar";
            var method = new LoginAuthMethod();

            Transaction.HasPropertyString = s =>
            {
                switch (s)
                {
                    case "Username":
                        return true;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            };

            string password = null;
            bool permanent = false;

            Transaction.SetPropertyStringObjectBoolean = (s, v, p) =>
            {
                switch (s)
                {
                    case "Password":
                        password = (string)v;
                        permanent = p;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            };

            string challenge;
            var result = method.ProcessResponse(Transaction, expectedPassword, out challenge);

            Assert.True(result);
            Assert.Null(challenge);
            Assert.Equal(expectedPassword, password);
            Assert.True(permanent);
        }

        [Fact]
        public void TestAbort()
        {
            var method = new LoginAuthMethod();

            bool usernameReset = false;
            bool passwordReset = false;

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
