using System;
using bbv.Common.EventBroker;

namespace HydraCore.CommandHandlers
{
    public abstract class CommandHandlerBase : ICommandHandler
    {
        public SMTPCore Server { get; private set; }

        [EventPublication("CommandExecution")]
        public event EventHandler<CommandExecuteEventArgs> OnExecute;

        public virtual void Initialize(SMTPCore core)
        {
            Server = core;
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

        public abstract SMTPResponse DoExecute(SMTPTransaction transaction, string parameters);
    }
}