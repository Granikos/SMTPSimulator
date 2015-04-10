//-------------------------------------------------------------------------------
// <copyright file="EventSubscriptionAttribute.cs" company="bbv Software Services AG">
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
    using ScopeMatchers;

    /// <summary>
    /// Declares a handler as an <see cref="IEventTopic"/> subscription.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class EventSubscriptionAttribute : Attribute
    {
        /// <summary>
        /// The URI of the event topic this publication refers to.
        /// </summary>
        private readonly string topic;

        /// <summary>
        /// Which threading strategy handler is used for this subscription.
        /// </summary>
        private readonly Type handlerType;

        /// <summary>
        /// Which scope matcher is used for this subscription.
        /// </summary>
        private readonly Type scopeMatcherType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriptionAttribute"/> class using the specified handler to execute the subscription.
        /// </summary>
        /// <param name="topic">The name of the <see cref="IEventTopic"/> to subscribe to.</param>
        /// <param name="handlerType">The type of the handler to execute the subscription (on publisher thread, user interface, ...).</param>
        public EventSubscriptionAttribute(string topic, Type handlerType)
            : this(topic, handlerType, typeof(SubscribeGlobal))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriptionAttribute"/> class using the specified handler to execute the subscription and the
        /// specified subscription scope matcher.
        /// </summary>
        /// <param name="topic">The name of the <see cref="IEventTopic"/> to subscribe to.</param>
        /// <param name="handlerType">The type of the handler to execute the subscription (on publisher thread, user interface, ...).</param>
        /// <param name="scopeMatcherType">Type of the scope matcher.</param>
        public EventSubscriptionAttribute(string topic, Type handlerType, Type scopeMatcherType)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException("topic must not be null or empty.", "topic");
            }
            
            this.topic = topic;
            this.handlerType = handlerType;
            this.scopeMatcherType = scopeMatcherType;
        }

        /// <summary>
        /// Gets the name of the <see cref="IEventTopic"/> the decorated method is subscribed to.
        /// </summary>
        public string Topic
        {
            get { return this.topic; }
        }

        /// <summary>
        /// Gets the type of the subscription execution handler.
        /// </summary>
        /// <value>The type of the subscription execution handler.</value>
        public Type HandlerType
        {
            get { return this.handlerType; }
        }

        /// <summary>
        /// Gets the type of the scope matcher.
        /// </summary>
        /// <value>The type of the scope matcher.</value>
        public Type ScopeMatcherType
        {
            get { return this.scopeMatcherType; }
        }
    }
}
