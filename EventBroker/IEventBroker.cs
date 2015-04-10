//-------------------------------------------------------------------------------
// <copyright file="IEventBroker.cs" company="bbv Software Services AG">
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

namespace bbv.Common.EventBroker
{
    using System;
    using System.IO;

    /// <summary>
    /// Interface for <see cref="EventBroker"/>.
    /// Use this interface to reference the event broker from your classes. This gives you the possibility to
    /// mock it.
    /// </summary>
    public interface IEventBroker : IDisposable
    {
        /// <summary>
        /// Registers an item with this event broker.
        /// </summary>
        /// <remarks>
        /// The item is scanned for publications and subscriptions and wired to the corresponding invokers and handlers.
        /// </remarks>
        /// <param name="item">Item to register with the event broker.</param>
        void Register(object item);

        /// <summary>
        /// Unregisters the specified item from this event broker.
        /// </summary>
        /// <param name="item">The item to unregister.</param>
        void Unregister(object item);

        /// <summary>
        /// Describes all event topics of this event broker:
        /// publications, subscriptions, names, thread options, scopes, event args.
        /// </summary>
        /// <param name="writer">The writer.</param>
        void DescribeTo(TextWriter writer);

        /// <summary>
        /// Fires the specified topic direclty on the <see cref="IEventBroker"/> without a real publisher.
        /// This is usefull when temporarily created objects need to fire events.
        /// The event is fired globally but can be subscribed with <see cref="ScopeMatchers.ISubscriptionScopeMatcher"/>.
        /// </summary>
        /// <param name="topic">The topic URI.</param>
        /// <param name="sender">The sender (which is also the publisher for this event).</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Fire(string topic, object sender, EventArgs e);
    }
}