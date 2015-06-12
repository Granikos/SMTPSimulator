using System;
using System.ComponentModel;
using System.Linq;
using bbv.Common.EventBroker;
using bbv.Common.EventBroker.Handlers;

namespace HydraCore.CommandHandlers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UnsecureAllowedAttribute : Attribute
    {
    }

    public class StartTLSEventArgs : CancelEventArgs
    {
        public StartTLSEventArgs(SMTPTransaction transaction)
        {
            Transaction = transaction;
        }

        public SMTPTransaction Transaction { get; private set; }
    }

    [UnsecureAllowed]
    [CommandHandler(Command = "STARTTLS")]
    public class STARTTLSHandler : CommandHandlerBase
    {
        [EventSubscription("CommandExecution", typeof (Publisher))]
        public void OnCommandExecute(object sender, CommandExecuteEventArgs args)
        {
            var type = args.Handler.GetType();
            var unsecuredAllowed = type.GetCustomAttributes(typeof(UnsecureAllowedAttribute), false).Any();
            var requireEncryption = args.Transaction.Settings.RequireTLS;
            var isSecured = args.Transaction.TLSEnabled;

            if (!unsecuredAllowed && requireEncryption && !isSecured)
            {
                args.Response = new SMTPResponse(SMTPStatusCode.AuthRequired, "Must issue a STARTTLS command first");
            }
        }

        public override void Initialize(SMTPCore core)
        {
            base.Initialize(core);

            core.GetListProperty<Func<SMTPTransaction, string>>("EHLOLines").Add(transaction => transaction.TLSEnabled? null : "STARTTLS");
        }

        [EventPublication("StartTLS")]
        public event EventHandler<CancelEventArgs> OnStartTLS;

        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (!String.IsNullOrEmpty(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            if (transaction.GetProperty<bool>("TLS"))
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