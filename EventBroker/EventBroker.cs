//-------------------------------------------------------------------------------
// <copyright file="EventBroker.cs" company="bbv Software Services AG">
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
    using System.Reflection;
    using log4net;
    using ScopeMatchers;

    /// <summary>
    /// The <see cref="EventBroker"/> is the facade component to the event broker framework.
    /// It provides the registration and unregistration functionality for event publisher and subscribers.
    /// </summary>
    public class EventBroker : IEventBroker
    {
        /// <summary>
        /// Logger of this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Used for thread synchronization.
        /// </summary>
        private readonly object syncRoot = new object();
        
        /// <summary>
        /// The inspector used to find publications and subscription within a class.
        /// </summary>
        private readonly EventInspector eventInspector = new EventInspector();
        
        /// <summary>
        /// The event topic host that holds all event topics of this event broker.
        /// </summary>
        private readonly IEventTopicHost eventTopicHost = new EventTopicHost();
        
        /// <summary>
        /// The factory used to create event broker related instances.
        /// </summary>
        private readonly IFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBroker"/> class.
        /// The <see cref="StandardFactory"/> is used to create <see cref="IHandler"/>s <see cref="IPublicationScopeMatcher"/>s and
        /// <see cref="ISubscriptionScopeMatcher"/>s.
        /// </summary>
        public EventBroker() : this(new StandardFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBroker"/> class.
        /// </summary>
        /// <param name="factory">The factory to create <see cref="IHandler"/>s <see cref="IPublicationScopeMatcher"/>s and
        /// <see cref="ISubscriptionScopeMatcher"/>s.</param>
        public EventBroker(IFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Registers an item with this event broker.
        /// </summary>
        /// <remarks>
        /// The item is scanned for publications and subscriptions and wired to the corresponding invokers and handlers.
        /// </remarks>
        /// <param name="item">Item to register with the event broker.</param>
        public void Register(object item)
        {
            lock (this.syncRoot)
            {
                this.eventInspector.ProcessPublisher(item, true, this.eventTopicHost, this.factory);
                this.eventInspector.ProcessSubscriber(item, true, this.eventTopicHost, this.factory);
            }

            INamedItem namedItem = item as INamedItem;
            if (namedItem != null)
            {
                log.DebugFormat("Registered item '{0}' with name '{1}'.", item, namedItem.EventBrokerItemName);
            }
            else
            {
                log.DebugFormat("Registered item '{0}'.", item);
            }
        }

        /// <summary>
        /// Unregisters the specified item from this event broker.
        /// </summary>
        /// <param name="item">The item to unregister.</param>
        public void Unregister(object item)
        {
            lock (this.syncRoot)
            {
                this.eventInspector.ProcessPublisher(item, false, this.eventTopicHost, this.factory);
                this.eventInspector.ProcessSubscriber(item, false, this.eventTopicHost, this.factory);
            }

            INamedItem namedItem = item as INamedItem;
            if (namedItem != null)
            {
                log.DebugFormat("Unregistered item '{0}' with name '{1}'.", item, namedItem.EventBrokerItemName);
            }
            else
            {
                log.DebugFormat("Unregistered item '{0}'.", item);
            }
        }

        /// <summary>
        /// Fires the specified topic direclty on the <see cref="IEventBroker"/> without a real publisher.
        /// This is usefull when temporarily created objects need to fire events.
        /// The event is fired globally but can be subscribed with <see cref="ScopeMatchers.ISubscriptionScopeMatcher"/>.
        /// </summary>
        /// <param name="topic">The topic URI.</param>
        /// <param name="sender">The sender (which is also the publisher for this event).</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void Fire(string topic, object sender, EventArgs e)
        {
            if (this.eventTopicHost.EventTopics.ContainsKey(topic))
            {
                this.eventTopicHost.EventTopics[topic].Fire(
                    sender,
                    e,
                    new SpontaneousPublication(sender, new PublishGlobal()));
            }
            else
            {
                log.DebugFormat(
                    "Directly fired event '{0}' with sender '{1}' and EventArgs '{2}'. No handler registered.",
                    topic, 
                    sender, 
                    e);
            }
        }

        /// <summary>
        /// Describes all event topics of this event broker:
        /// publications, subscriptions, names, thread options, scopes, event args.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void DescribeTo(TextWriter writer)
        {
            this.eventTopicHost.DescribeTo(writer);
        }

        #region Dispose

        /// <summary>
        /// See <see cref="IDisposable.Dispose"/> for more information.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementation of the disposable pattern.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// Unregisters the event handler of all topics
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.syncRoot)
                {
                    this.eventTopicHost.Dispose();
                }
            }
        }

        #endregion
    }
}
