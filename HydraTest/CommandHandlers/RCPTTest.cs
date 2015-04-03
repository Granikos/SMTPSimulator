using System;
using System.Collections.Generic;
using HydraCore;
using HydraCore.CommandHandlers;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class RCPTTest : TestBase
    {
        [Fact]
        public void TestSuccess()
        {
            var handler = new RCPTHandler();

            var paths = new List<Path>();
            var permanent = false;

            Transaction.GetPropertyOf1String(name =>
            {
                switch (name)
                {
                    case "MailInProgress":
                        return true;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            });

            Transaction.GetListPropertyOf1StringBoolean((name, p) =>
            {
                switch (name)
                {
                    case "ForwardPath":
                        permanent = p;
                        return paths;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            });

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "TO:<test@test.de>");
            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Contains(new Path("test", "test.de"), paths);
            Assert.False(permanent);
        }
        [Fact]
        public void TestSuccessWithPostmaster()
        {
            var handler = new RCPTHandler();

            var paths = new List<Path>();
            var permanent = false;

            Transaction.GetPropertyOf1String(name =>
            {
                switch (name)
                {
                    case "MailInProgress":
                        return true;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            });

            Transaction.GetListPropertyOf1StringBoolean((name, p) =>
            {
                switch (name)
                {
                    case "ForwardPath":
                        permanent = p;
                        return paths;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            });

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "TO:<postmaster>");
            Assert.Equal(SMTPStatusCode.Okay, response.Code);
            Assert.Contains(Path.Postmaster, paths);
            Assert.False(permanent);
        }

        [Fact]
        public void TestSequenceError()
        {
            var handler = new RCPTHandler();
            Transaction.GetPropertyOf1String(name =>
            {
                switch (name)
                {
                    case "MailInProgress":
                        return false;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            });

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "TO:<test@test.de>");
            Assert.Equal(SMTPStatusCode.BadSequence, response.Code);
        }

        [Fact]
        public void TestNoParamError()
        {
            var handler = new RCPTHandler();

            Transaction.GetPropertyOf1String(name =>
            {
                switch (name)
                {
                    case "MailInProgress":
                        return true;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            });

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "");
            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }

        [Fact]
        public void TestBadParamError()
        {
            var handler = new RCPTHandler();

            Transaction.GetPropertyOf1String(name =>
            {
                switch (name)
                {
                    case "MailInProgress":
                        return true;
                    default:
                        throw new InvalidOperationException("Invalid name.");
                }
            });

            handler.Initialize(Core);

            var response = handler.Execute(Transaction, "fubar");
            Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
        }
    }
}