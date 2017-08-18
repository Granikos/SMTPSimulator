// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.ComponentModel;
using System.Linq;
using bbv.Common.EventBroker;
using bbv.Common.EventBroker.Handlers;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UnsecureAllowedAttribute : Attribute
    {
    }

    [UnsecureAllowed]
    [CommandHandler(Command = "STARTTLS")]
    public class STARTTLSHandler : CommandHandlerBase
    {
        [EventSubscription("CommandExecution", typeof (Publisher))]
        public void OnCommandExecute(object sender, CommandExecuteEventArgs args)
        {
            var type = args.Handler.GetType();
            var unsecuredAllowed = type.GetCustomAttributes(typeof (UnsecureAllowedAttribute), false).Any();
            var requireEncryption = args.Transaction.Settings.RequireTLS;
            var isSecured = args.Transaction.TLSActive;

            if (!unsecuredAllowed && requireEncryption && !isSecured)
            {
                args.Response = new SMTPResponse(SMTPStatusCode.AuthRequired, "Must issue a STARTTLS command first");
            }
        }

        public override void Initialize(SMTPServer server)
        {
            base.Initialize(server);

            server.GetListProperty<Func<SMTPTransaction, string>>("EHLOLines")
                .Add(transaction => transaction.TLSActive || !transaction.Settings.EnableTLS ? null : "STARTTLS");
        }

        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            if (!transaction.Settings.EnableTLS)
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence); // TODO: Check response code
            }

            if (transaction.TLSActive)
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            var args = new CancelEventArgs();

            transaction.StartTLS(args);

            if (args.Cancel)
            {
                return new SMTPResponse(SMTPStatusCode.TLSNotAvailiable, "TLS not available due to temporary reason");
            }

            return new SMTPResponse(SMTPStatusCode.Ready, "Ready to start TLS");
        }
    }
}