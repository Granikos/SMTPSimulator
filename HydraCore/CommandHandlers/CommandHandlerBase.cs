using System;
using bbv.Common.EventBroker;

namespace HydraCore.CommandHandlers
{
    public abstract class CommandHandlerBase : ICommandHandler
    {
        public SMTPCore Server { get; private set; }

        public virtual void Initialize(SMTPCore core)
        {
            Server = core;
        }

        public virtual void Initialize(SMTPTransaction transaction)
        {
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