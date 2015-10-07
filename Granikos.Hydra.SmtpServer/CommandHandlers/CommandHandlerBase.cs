using System;
using bbv.Common.EventBroker;
using Granikos.Hydra.Core;

namespace Granikos.Hydra.SmtpServer.CommandHandlers
{
    public abstract class CommandHandlerBase : ICommandHandler
    {
        public SMTPServer Server { get; private set; }

        public virtual void Initialize(SMTPServer server)
        {
            Server = server;
        }

        public SMTPResponse Execute(SMTPTransaction transaction, string parameters)
        {
            var args = new CommandExecuteEventArgs(transaction, this, parameters);

            if (OnExecute != null) OnExecute(this, args);

            if (args.Response != null)
            {
                return args.Response;
            }

            return DoExecute(transaction, parameters);
        }

        [EventPublication("CommandExecution")]
        public event EventHandler<CommandExecuteEventArgs> OnExecute;

        public abstract SMTPResponse DoExecute(SMTPTransaction transaction, string parameters);
    }
}