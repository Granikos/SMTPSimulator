//-------------------------------------------------------------------------------
// <copyright file="Publication.cs" company="bbv Software Services AG">
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
    using Exceptions;
    using log4net;
    using ScopeMatchers;
    
    /// <summary>
    /// Represents a topic publication.
    /// </summary>
    internal class Publication : IPublication
    {
        /// <summary>
        /// Logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The event topic this publication is registered on.
        /// </summary>
        private readonly EventTopic topic;

        /// <summary>
        /// Weak reference to the publisher.
        /// </summary>
        private readonly WeakReference publisher;

        /// <summary>
        /// Name of the event in the publisher class.
        /// </summary>
        private readonly string eventName;

        /// <summary>
        /// The scope matcher used on this publication.
        /// </summary>
        private readonly IPublicationScopeMatcher publicationScopeMatcher;

        /// <summary>
        /// The type of the event handler this publication is using. Used in <see cref="DescribeTo"/>.
        /// </summary>
        private readonly Type eventHandlerType;

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="Publication"/> class.
        /// </summary>
        /// <param name="topic">The event topic this publication belongs to.</param>
        /// <param name="publisher">The publisher.</param>
        /// <param name="eventName">Name of the event in the publisher class.</param>
        /// <param name="publicationScopeMatcher">The publication scope matcher.</param>
        public Publication(EventTopic topic, object publisher, string eventName, IPublicationScopeMatcher publicationScopeMatcher)
        {
            this.topic = topic;
            this.publisher = new WeakReference(publisher);
            this.eventName = eventName;
            this.publicationScopeMatcher = publicationScopeMatcher;

            EventInfo publishedEvent = publisher.GetType().GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            if (publishedEvent == null)
            {
                throw new PublisherEventNotFoundException(publisher.GetType(), eventName);
            }

            if (publishedEvent.EventHandlerType == null)
            {
                throw new Exception("EventHandlerType on published event must not be null (internal EventBroker failure).");
            }

            this.ThrowIfInvalidEventHandler(publishedEvent);
            ThrowIfEventIsStatic(publishedEvent);

            Delegate handler = Delegate.CreateDelegate(
                publishedEvent.EventHandlerType, 
                this, 
                GetType().GetMethod("PublicationHandler"));
            publishedEvent.AddEventHandler(publisher, handler);

            this.eventHandlerType = publishedEvent.EventHandlerType; // used for DescribeTo
        }

        #endregion

        #region Data

        /// <summary>
        /// Gets the publisher of the event.
        /// </summary>
        public object Publisher
        {
            get { return this.publisher.Target; }
        }

        /// <summary>
        /// Gets the publication scope matcher.
        /// </summary>
        /// <value>The publication scope matcher.</value>
        public IPublicationScopeMatcher PublicationScopeMatcher
        {
            get { return this.publicationScopeMatcher; }
        }

        /// <summary>
        /// Gets the name of the event on the <see cref="Publication.Publisher"/>.
        /// </summary>
        public string EventName
        {
            get { return this.eventName; }
        }

        #endregion

        /// <summary>
        /// Gets the type of the event handler this publication is using.
        /// </summary>
        /// <value>The type of the event handler.</value>
        public Type EventHandlerType
        {
            get { return this.eventHandlerType; }
        }

        /// <summary>
        /// Fires the event publication. This method is registered to the event on the publisher.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void PublicationHandler(object sender, EventArgs e)
        {
            log.DebugFormat(
                "Firing event '{0}'. Invoked by publisher '{1}' with EventArgs '{2}'.",
                this.topic.Uri, 
                sender, 
                e);

            this.topic.Fire(sender, e, this);
        }

        #region DescribeTo

        /// <summary>
        /// Describes this publication
        /// name, scope, event handler.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void DescribeTo(TextWriter writer)
        {
            if (this.publisher.IsAlive)
            {
                writer.Write(this.Publisher.GetType().FullName);

                if (this.Publisher is INamedItem)
                {
                    writer.Write(", Name = ");
                    writer.Write(((INamedItem)Publisher).EventBrokerItemName);
                }
                
                writer.Write(", Event = ");
                writer.Write(this.eventName);
                writer.Write(", EventHandler type = ");
                writer.Write(this.eventHandlerType.FullName);
                writer.Write(", scope matcher = ");
                this.publicationScopeMatcher.DescribeTo(writer);
            }
        }

        #endregion

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
        /// Unregisters the event handler.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Publisher != null)
                {
                    EventInfo publishedEvent = this.Publisher.GetType().GetEvent(this.eventName);

                    if (publishedEvent.EventHandlerType != null)
                    {
                        publishedEvent.RemoveEventHandler(
                            this.Publisher,
                            Delegate.CreateDelegate(publishedEvent.EventHandlerType, this, GetType().GetMethod("PublicationHandler")));
                    }
                }
            }
        }

        #endregion

        #region Publication Event Validation

        /// <summary>
        /// Throws a <see cref="StaticPublisherEventException"/> if the published event is defined static.
        /// </summary>
        /// <param name="publishedEvent">The published event.</param>
        /// <exception cref="StaticPublisherEventException">Thrown if the published event is defined static.</exception>
        private static void ThrowIfEventIsStatic(EventInfo publishedEvent)
        {
            if (publishedEvent.GetAddMethod().IsStatic || publishedEvent.GetRemoveMethod().IsStatic)
            {
                throw new StaticPublisherEventException(publishedEvent);
            }
        }

        /// <summary>
        /// Throws an <see cref="InvalidPublicationSignatureException"/> if defined event handler on publisher
        /// is not an <see cref="EventHandler"/>.
        /// </summary>
        /// <param name="info">The event info of the published event.</param>
        /// <exception cref="InvalidPublicationSignatureException">Thrown if defined event handler on publisher is not an <see cref="EventHandler"/>.</exception>
        private void ThrowIfInvalidEventHandler(EventInfo info)
        {
            if (info.EventHandlerType == null ||
                typeof(EventHandler).IsAssignableFrom(info.EventHandlerType) ||
                (info.EventHandlerType.IsGenericType &&
                 typeof(EventHandler<>).IsAssignableFrom(info.EventHandlerType.GetGenericTypeDefinition())))
            {
                return;
            }

            throw new InvalidPublicationSignatureException(info);
        }

        #endregion
    }
}
