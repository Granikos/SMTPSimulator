using System;
using System.ComponentModel;
using System.Linq;
using bbv.Common.EventBroker;
using bbv.Common.EventBroker.Handlers;
using Granikos.Hydra.Core;

namespace Granikos.Hydra.SmtpServer.CommandHandlers
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