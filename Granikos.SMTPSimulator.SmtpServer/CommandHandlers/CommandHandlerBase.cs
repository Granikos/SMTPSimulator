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
using bbv.Common.EventBroker;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
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