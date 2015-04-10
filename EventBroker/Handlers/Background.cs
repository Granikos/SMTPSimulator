//-------------------------------------------------------------------------------
// <copyright file="Background.cs" company="bbv Software Services AG">
//   Copyright (c) 2008 bbv Software Services AG
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Contains software or other content adapted from 
//   Smart Client – Composite UI Application Block, 
//   2005 Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace bbv.Common.EventBroker.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using log4net;

    /// <summary>
    /// Handler that executes the subscription on a thread pool worker process (asynchronous).
    /// </summary>
    public class Background : IHandler
    {
        /// <summary>
        /// Logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initalizes the handler. There is nothing to initialize for <see cref="Background"/>.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="handlerMethodName">Name of the handler method on the subscriber.</param>
        /// <param name="parameterTypes">The parameter types of the handler method on the subscriber.</param>
        public void Initalize(object subscriber, string handlerMethodName, Type[] parameterTypes)
        {
            // there is nothing to initialize
        }

        /// <summary>
        /// Executes the subscription on a thread pool worker thread.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <param name="subscriptionHandler">The subscription handler.</param>
        /// <param name="exceptions">The exceptions that occured during the execution. For implementors: Add exception here, Handle must not throw exceptions.</param>
        public void Handle(object sender, EventArgs e, Delegate subscriptionHandler, ICollection<Exception> exceptions)
        {
            ThreadPool.QueueUserWorkItem(
                delegate(object state)
                    {
                        CallInBackgroundArguments args = (CallInBackgroundArguments) state;
                        try
                        {
                            args.Handler.DynamicInvoke(args.Sender, args.EventArgs);
                        }
                        catch (Exception ex)
                        {
                            log.Error("An exception occured on background thread.", ex);
                            throw;
                        }
                    },
                    new CallInBackgroundArguments(sender, e, subscriptionHandler));
        }

        /// <summary>
        /// Struct that is passed to the background worker thread.
        /// </summary>
        private struct CallInBackgroundArguments
        {
            /// <summary>
            /// The event topic handler method on the subscriber.
            /// </summary>
            public readonly Delegate Handler;

            /// <summary>
            /// The publisher sending the event.
            /// </summary>
            public readonly object Sender;

            /// <summary>
            /// The event args of the event.
            /// </summary>
            public readonly EventArgs EventArgs;

            /// <summary>
            /// Initializes a new instance of the <see cref="CallInBackgroundArguments"/> struct.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            /// <param name="handler">The handler.</param>
            public CallInBackgroundArguments(object sender, EventArgs eventArgs, Delegate handler)
            {
                this.Sender = sender;
                this.EventArgs = eventArgs;
                this.Handler = handler;
            }
        }
    }
}