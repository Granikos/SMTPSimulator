using System;
using System.Collections.Generic;
using HydraCore;
using HydraCore.CommandHandlers;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class RCPTTest
    {
        [Fact]
        public void TestSuccess()
        {
            using (ShimsContext.Create())
            {
                var handler = new RCPTHandler();

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction();

                var paths = new List<string>();
                var permanent = false;

                transaction.GetPropertyOf1String(name =>
                {
                    switch (name)
                    {
                        case "MailInProgress":
                            return true;
                        default:
                            throw new InvalidOperationException("Invalid name.");
                    }
                });

                transaction.GetListPropertyOf1StringBoolean((name, p) =>
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

                handler.Initialize(server);

                var response = handler.Execute(transaction, "TO:<test@test.de>");
                Assert.Equal(SMTPStatusCode.Okay, response.Code);
                Assert.Contains("test@test.de", paths);
                Assert.False(permanent);
            }
        }

        [Fact]
        public void TestSequenceError()
        {
            using (ShimsContext.Create())
            {
                var handler = new RCPTHandler();

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction();
                transaction.GetPropertyOf1String(name =>
                {
                    switch (name)
                    {
                        case "MailInProgress":
                            return false;
                        default:
                            throw new InvalidOperationException("Invalid name.");
                    }
                });

                handler.Initialize(server);

                var response = handler.Execute(transaction, "TO:<test@test.de>");
                Assert.Equal(SMTPStatusCode.BadSequence, response.Code);
            }
        }

        [Fact]
        public void TestNoParamError()
        {
            using (ShimsContext.Create())
            {
                var handler = new RCPTHandler();

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction();
                transaction.GetPropertyOf1String(name =>
                {
                    switch (name)
                    {
                        case "MailInProgress":
                            return true;
                        default:
                            throw new InvalidOperationException("Invalid name.");
                    }
                });

                handler.Initialize(server);

                var response = handler.Execute(transaction, "");
                Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
            }
        }

        [Fact]
        public void TestBadParamError()
        {
            using (ShimsContext.Create())
            {
                var handler = new RCPTHandler();

                var server = new ShimSMTPCore();
                var transaction = new ShimSMTPTransaction();
                transaction.GetPropertyOf1String(name =>
                {
                    switch (name)
                    {
                        case "MailInProgress":
                            return true;
                        default:
                            throw new InvalidOperationException("Invalid name.");
                    }
                });

                handler.Initialize(server);

                var response = handler.Execute(transaction, "fubar");
                Assert.Equal(SMTPStatusCode.SyntaxError, response.Code);
            }
        }
    }
}