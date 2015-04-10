//-------------------------------------------------------------------------------
// <copyright file="EventTopic.cs" company="bbv Software Services AG">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Exceptions;
    using log4net;
    using ScopeMatchers;
    
    /// <summary>
    /// Represents a point of communication on a certain topic between the topic publishers and the topic subscribers.
    /// </summary>
    internal class EventTopic : IEventTopic
    {
        /// <summary>
        /// Logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The URI that identifies this event topic uniquely on an event broker.
        /// </summary>
        private readonly string uri;

        /// <summary>
        /// List of all publications that fire this event topic.
        /// </summary>
        private readonly List<Publication> publications = new List<Publication>();

        /// <summary>
        /// List of all subscriptions that listen to this event topic.
        /// </summary>
        private readonly List<Subscription> subscriptions = new List<Subscription>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTopic"/> class.
        /// </summary>
        /// <param name="uri">The topic URI.</param>
        public EventTopic(string uri)
        {
            this.uri = uri;
            
            log.DebugFormat("Topic created: '{0}'.", uri);
        }

        /// <summary>
        /// Gets the count of registered publications with this <see cref="EventTopic"/>.
        /// </summary>
        public int PublicationCount
        {
            get
            {
                this.Clean();
                return this.publications.Count;
            }
        }

        /// <summary>
        /// Gets the topic URI.
        /// </summary>
        /// <value>The topic URI.</value>
        public string Uri
        {
            get { return this.uri; }
        }

        /// <summary>
        /// Gets the count of registered subscriptions to this <see cref="EventTopic"/>.
        /// </summary>
        public int SubscriptionCount
        {
            get
            {
                this.Clean();
                return this.subscriptions.Count;
            }
        }

        /// <summary>
        /// Adds a publication to the topic.
        /// </summary>
        /// <param name="publisher">The object that publishes the event that will fire the topic.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="scopeMatcher">Matcher for publication scope.</param>
        public void AddPublication(object publisher, string eventName, IPublicationScopeMatcher scopeMatcher)
        {
            this.Clean();
            this.ThrowIfRepeatedPublication(publisher, eventName);

            Publication publication = new Publication(this, publisher, eventName, scopeMatcher);
            this.publications.Add(publication);

            foreach (Subscription subscription in this.subscriptions)
            {
                this.ThrowIfPublisherAndSubscriberEventArgsMismatch(subscription, publication);
            }

            log.DebugFormat(
                "Added publication '{0}.{1}' to topic '{2}' with scope matcher '{3}'.", 
                publisher.GetType().FullName, 
                eventName,
                this.uri, 
                scopeMatcher);
        }

        /// <summary>
        /// Removes a publication from the topic.
        /// </summary>
        /// <param name="publisher">The object that contains the publication.</param>
        /// <param name="eventName">The name of event on the publisher that fires the topic.</param>
        public void RemovePublication(object publisher, string eventName)
        {
            this.Clean();
            Publication publication = this.FindPublication(publisher, eventName);
            if (publication != null)
            {
                this.publications.Remove(publication);
                publication.Dispose();

                log.DebugFormat(
                    "Removed publication '{0}.{1}' from topic '{2}'.", 
                    publisher.GetType().FullName, 
                    eventName,
                    this.uri);
            }
        }

        /// <summary>
        /// Adds a subcription to this <see cref="EventTopic"/>.
        /// </summary>
        /// <param name="subscriber">The object that contains the method that will handle the <see cref="EventTopic"/>.</param>
        /// <param name="handlerMethodName">The name of the method on the subscriber that will handle the <see cref="EventTopic"/>.</param>
        /// <param name="parameterTypes">Defines the types and order of the parameters for the subscriber. For none pass null.
        /// Use this overload when there are several methods with the same name on the subscriber.</param>
        /// <param name="handler">The handler that is used to execute the subscription.</param>
        /// <param name="subscriptionScopeMatcher">Matcher for the subscription scope.</param>
        public void AddSubscription(object subscriber, string handlerMethodName, Type[] parameterTypes, IHandler handler, ISubscriptionScopeMatcher subscriptionScopeMatcher)
        {
            this.Clean();
            Subscription subscription = new Subscription(
                subscriber, 
                handlerMethodName,
                parameterTypes, 
                handler, 
                subscriptionScopeMatcher);
            this.subscriptions.Add(subscription);

            foreach (Publication publication in this.publications)
            {
                this.ThrowIfPublisherAndSubscriberEventArgsMismatch(subscription, publication);
            }

            log.DebugFormat(
                "Added subscription '{0}.{1}' to topic '{2}'.", 
                subscriber.GetType().FullName, 
                handlerMethodName,
                this.uri);
        }

        /// <summary>
        /// Removes a subscription from this <see cref="EventTopic"/>.
        /// </summary>
        /// <param name="subscriber">The object that contains the method that will handle the <see cref="EventTopic"/>.</param>
        /// <param name="handlerMethodName">The name of the method on the subscriber that will handle the <see cref="EventTopic"/>.</param>
        public void RemoveSubscription(object subscriber, string handlerMethodName)
        {
            this.Clean();
            Subscription subscription = this.FindSubscription(subscriber, handlerMethodName);
            if (subscription != null)
            {
                this.subscriptions.Remove(subscription);

                log.DebugFormat(
                    "Removed subscription '{0}.{1}' from topic '{2}'.", 
                    subscriber.GetType().FullName, 
                    handlerMethodName,
                    this.uri);
            }
        }

        /// <summary>
        /// Describes this event topic:
        /// publications, subscriptions, names, thread options, scopes, event args.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void DescribeTo(TextWriter writer)
        {
            writer.Write("EventTopic: ");
            writer.Write(this.Uri);
            writer.WriteLine();

            writer.WriteLine("Publishers:");
            foreach (Publication publication in this.publications)
            {
                publication.DescribeTo(writer);
                writer.WriteLine();
            }

            writer.WriteLine("Subscribers:");
            foreach (Subscription subscription in this.subscriptions)
            {
                subscription.DescribeTo(writer);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Called to free resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Fires the <see cref="IEventTopic"/>.
        /// </summary>
        /// <param name="sender">The object that acts as the sender of the event to the subscribers. 
        /// Not always the publisher (it's the sender provided in the event call).</param>
        /// <param name="e">An <see cref="EventArgs"/> instance to be passed to the subscribers.</param>
        /// <param name="publication">The publication firing the event topic.</param>
        public void Fire(object sender, EventArgs e, IPublication publication)
        {
            this.Clean();

            EventTopicFireDelegate[] handlers = this.GetHandlers(publication);
            this.CallSubscriptionHandlers(sender, e, handlers, publication);

            INamedItem namedItem = publication.Publisher as INamedItem;
            if (namedItem != null)
            {
                log.DebugFormat(
                    "Fired event '{0}'. Invoked by publisher '{1}' with name '{2}' with sender '{3}' and EventArgs '{4}'.",
                    this.uri, 
                    publication.Publisher, 
                    namedItem.EventBrokerItemName, 
                    sender, 
                    e);
            }
            else
            {
                log.DebugFormat(
                    "Fired event '{0}'. Invoked by publisher '{1}' with sender '{2}' and EventArgs '{3}'.",
                    this.uri, 
                    publication.Publisher, 
                    sender, 
                    e);
            }
        }

        /// <summary>
        /// Checks if the specified publication has been registered with this <see cref="EventTopic"/>.
        /// </summary>
        /// <param name="publisher">The object that contains the publication.</param>
        /// <param name="eventName">The name of event on the publisher that fires the topic.</param>
        /// <returns>true if the topic contains the requested publication; otherwise false.</returns>
        public bool ContainsPublication(object publisher, string eventName)
        {
            this.Clean();
            return this.FindPublication(publisher, eventName) != null;
        }

        /// <summary>
        /// Checks if the specified subscription has been registered with this <see cref="EventTopic"/>.
        /// </summary>
        /// <param name="subscriber">The object that contains the method that will handle the <see cref="EventTopic"/>.</param>
        /// <param name="handlerMethodName">The name of the method on the subscriber that will handle the <see cref="EventTopic"/>.</param>
        /// <returns>true, if the topic contains the subscription; otherwise false.</returns>
        public bool ContainsSubscription(object subscriber, string handlerMethodName)
        {
            this.Clean();
            return this.FindSubscription(subscriber, handlerMethodName) != null;
        }

        /// <summary>
        /// Called to free resources.
        /// </summary>
        /// <param name="disposing">Should be true when calling from Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Publication publication in this.publications)
                {
                    publication.Dispose();
                }

                this.publications.Clear();
            }
        }

        /// <summary>
        /// Checks whether the event of the publisher has to be relayed to the subscriber (Scope).
        /// </summary>
        /// <param name="publication">The publication.</param>
        /// <param name="subscription">The subscription.</param>
        /// <returns><code>true</code> if the event has to be relayed</returns>
        private static bool CheckScope(IPublication publication, Subscription subscription)
        {
            INamedItem p = publication.Publisher as INamedItem;
            INamedItem s = subscription.Subscriber as INamedItem;

            string publisherName = p != null ? p.EventBrokerItemName : string.Empty;
            string subscriberName = s != null ? s.EventBrokerItemName : string.Empty;

            return publication.PublicationScopeMatcher.Match(publisherName, subscriberName) && subscription.SubscriptionScopeMatcher.Match(publisherName, subscriberName);
        }

        /// <summary>
        /// Searches for a already registered publication for the same publisher and event.
        /// </summary>
        /// <param name="publisher">The publisher that will be registered newly.</param>
        /// <param name="eventName">Name of the published event.</param>
        /// <returns>The publication that is already registered.</returns>
        private Publication FindPublication(object publisher, string eventName)
        {
            Publication publication = this.publications.Find(
                match => match.Publisher == publisher &&
                         match.EventName == eventName);
            return publication;
        }

        /// <summary>
        /// Gets the handlers of this even topic
        /// </summary>
        /// <param name="publication">The publication (used for logging).</param>
        /// <returns>Array of delegates, the handlers for this even topic.</returns>
        private EventTopicFireDelegate[] GetHandlers(IPublication publication)
        {
            List<EventTopicFireDelegate> result = new List<EventTopicFireDelegate>();

            foreach (Subscription subscription in this.subscriptions)
            {
                if (CheckScope(publication, subscription))
                {
                    EventTopicFireDelegate handler = subscription.GetHandler();
                    if (handler != null)
                    {
                        result.Add(handler);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns the subscription of the specified subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber to look for.</param>
        /// <param name="handlerMethodName">Name of the handler method to look for.</param>
        /// <returns>The subscription for the specified subscriber and handler method, null if not found.</returns>
        private Subscription FindSubscription(object subscriber, string handlerMethodName)
        {
            this.Clean();

            return this.subscriptions.Find(
                match => match.Subscriber == subscriber && match.HandlerMethodName == handlerMethodName);
        }

        /// <summary>
        /// Perform a sanity cleaning of the dead references to publishers and subscribers
        /// </summary>
        /// <devdoc>As the topic maintains <see cref="WeakReference"/> to publishers and subscribers,
        /// those instances that are finished but hadn't been removed from the topic will leak. This method
        /// deals with that case.</devdoc>
        private void Clean()
        {
            foreach (Subscription subscription in this.subscriptions.ToArray())
            {
                if (subscription.Subscriber == null)
                {
                    this.subscriptions.Remove(subscription);
                }
            }

            foreach (Publication publication in this.publications.ToArray())
            {
                if (publication.Publisher == null)
                {
                    this.publications.Remove(publication);
                    publication.Dispose();
                }
            }
        }

        /// <summary>
        /// Calls the subscription handlers.
        /// </summary>
        /// <param name="sender">The publisher.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <param name="handlers">The handlers to call.</param>
        /// <param name="publication">The publication firing the event topic.</param>
        private void CallSubscriptionHandlers(object sender, EventArgs e, IEnumerable<EventTopicFireDelegate> handlers, IPublication publication)
        {
            List<Exception> exceptions = new List<Exception>();

            foreach (EventTopicFireDelegate handler in handlers)
            {
                handler(this, sender, e, publication, exceptions);
            }

            switch (exceptions.Count)
            {
                case 0:
                    break;

                case 1:
                    this.TraceExceptions(exceptions);
                    throw new EventTopicException(this, exceptions[0]);

                default:
                    this.TraceExceptions(exceptions);
                    throw new EventTopicException(this, new ReadOnlyCollection<Exception>(exceptions));       
            }
        }

        /// <summary>
        /// Logs the specified exceptions.
        /// </summary>
        /// <param name="exceptions">The exceptions.</param>
        private void TraceExceptions(IEnumerable<Exception> exceptions)
        {
            if (log.IsErrorEnabled)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("Errors occured during firing of topic '{0}':", this.uri));

                foreach (Exception e in exceptions)
                {
                    sb.AppendLine(e.ToString());
                }

                log.Error(sb.ToString());
            }
        }

        /// <summary>
        /// Throws a <see cref="RepeatedPublicationException"/> if a duplicate publication is detected.
        /// </summary>
        /// <param name="publisher">The publisher to add.</param>
        /// <param name="eventName">Name of the event to add.</param>
        /// <exception cref="RepeatedPublicationException">Thrown if a duplicate publication is detected.</exception>
        private void ThrowIfRepeatedPublication(object publisher, string eventName)
        {
            if (this.FindPublication(publisher, eventName) != null)
            {
                throw new RepeatedPublicationException(publisher, eventName);
            }
        }

        /// <summary>
        /// Throws an <see cref="EventTopicException"/> if publisher and subscriber use incompatible event args.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <param name="publication">The publication.</param>
        /// <exception cref="EventTopicException">Thrown if publisher and subscriber use incompatible event args</exception>
        private void ThrowIfPublisherAndSubscriberEventArgsMismatch(Subscription subscription, Publication publication)
        {
            Type publisherEventArgsType = publication.EventHandlerType;
            Type subscriberEventArgsType = subscription.EventHandlerType;

            // map the nongeneric EventHandler to the generic version because the subscriber uses always the generic version
            if (publisherEventArgsType == typeof(EventHandler)) 
            {
                publisherEventArgsType = typeof(EventHandler<EventArgs>);
            }

            if (!subscriberEventArgsType.IsAssignableFrom(publisherEventArgsType))
            {
                StringWriter writer = new StringWriter();
                writer.Write("Publication ");
                writer.WriteLine();
                publication.DescribeTo(writer);
                writer.WriteLine();
                writer.Write("does not match with subscription ");
                writer.WriteLine();
                subscription.DescribeTo(writer);

                throw new EventTopicException(writer.GetStringBuilder().ToString());
            }
        }
    }
}
