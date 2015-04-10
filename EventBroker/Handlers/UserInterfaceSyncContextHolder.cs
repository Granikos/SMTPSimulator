//-------------------------------------------------------------------------------
// <copyright file="UserInterfaceSyncContextHolder.cs" company="bbv Software Services AG">
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
    using System.Threading;
    using Exceptions;

    /// <summary>
    /// Helper class to store the synchronisation context.
    /// </summary>
    internal class UserInterfaceSyncContextHolder
    {
        /// <summary>
        /// Gets the synchronization context that was aquired on registration.
        /// </summary>
        /// <value>The synchronization context that was aquired on registration.</value>
        public SynchronizationContext SyncContext { get; private set; }

        /// <summary>
        /// Initalizes this instance. If the current thread is not the user interface thread then an exception is thrown.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="handlerMethodName">Name of the handler method on the subscriber.</param>
        /// <param name="parameterTypes">The parameter types of the method on the subscriber.</param>
        public void Initalize(object subscriber, string handlerMethodName, Type[] parameterTypes)
        {
            // If there's a syncronization context (i.e. the WindowsFormsSynchronizationContext 
            // created to marshal back to the thread where a control was initially created 
            // in a particular thread), capture it to marshal back to it through the 
            // context, that basically goes through a Post/Send.
            if (SynchronizationContext.Current != null)
            {
                this.SyncContext = SynchronizationContext.Current;
            }
            else
            {
                throw new NotUserInterfaceThreadException(subscriber, handlerMethodName);
            }
        }
    }
}