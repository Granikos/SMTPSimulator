//-------------------------------------------------------------------------------
// <copyright file="IEventTopic.cs" company="bbv Software Services AG">
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
    using ScopeMatchers;

    /// <summary>
    /// Represents a point of communication on a certain topic between the topic publishers and the topic subscribers.
    /// </summary>
    public interface IEventTopic : IDisposable
    {
        /// <summary>
        /// Gets the URI for the event topic. This URI is the unique identifier for this event topic.
        /// </summary>
        /// <value>The URI of this event topic.</value>
        string Uri { get; }

        /// <summary>
        /// Gets the count of registered publications with this <see cref="IEventTopic"/>.
        /// </summary>
        int PublicationCount { get; }

        /// <summary>
        /// Gets the count of registered subscriptions to this <see cref="IEventTopic"/>.
        /// </summary>
        int SubscriptionCount { get; }

        /// <summary>
        /// Adds a publication to the topic.
        /// </summary>
        /// <param name="publisher">The object that publishes the event that will fire the topic.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="scopeMatcher">The scope matcher for the publication.</param>
        void AddPublication(object publisher, string eventName, IPublicationScopeMatcher scopeMatcher);

        /// <summary>
        /// Removes a publication from the topic.
        /// </summary>
        /// <param name="publisher">The object that contains the publication.</param>
        /// <param name="eventName">The name of event on the publisher that fires the topic.</param>
        void RemovePublication(object publisher, string eventName);

        /// <summary>
        /// Adds a subcription to this <see cref="EventTopic"/>.
        /// </summary>
        /// <param name="subscriber">The object that contains the method that will handle the <see cref="EventTopic"/>.</param>
        /// <param name="handlerMethodName">The name of the method on the subscriber that will handle the <see cref="EventTopic"/>.</param>
        /// <param name="parameterTypes">Defines the types and order of the parameters for the subscriber. For none pass null.
        /// Use this overload when there are several methods with the same name on the subscriber.</param>
        /// <param name="handler">The handler that is used to execute the subscription.</param>
        /// <param name="subscriptionScopeMatcher">Matcher for the subscription scope.</param>
        void AddSubscription(
            object subscriber, 
            string handlerMethodName, 
            Type[] parameterTypes,
            IHandler handler, 
            ISubscriptionScopeMatcher subscriptionScopeMatcher);
        
        /// <summary>
        /// Removes a subscription from this <see cref="IEventTopic"/>.
        /// </summary>
        /// <param name="subscriber">The object that contains the method that will handle the <see cref="IEventTopic"/>.</param>
        /// <param name="handlerMethodName">The name of the method on the subscriber that will handle the <see cref="IEventTopic"/>.</param>
        void RemoveSubscription(object subscriber, string handlerMethodName);

        /// <summary>
        /// Describes this event topic:
        /// publications, subscriptions, names, thread options, scopes, event args.
        /// </summary>
        /// <param name="writer">The writer.</param>
        void DescribeTo(TextWriter writer);
    }
}
