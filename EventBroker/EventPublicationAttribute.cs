//-------------------------------------------------------------------------------
// <copyright file="EventPublicationAttribute.cs" company="bbv Software Services AG">
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
    /// Declares an event as an <see cref="IEventTopic"/> publication.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = true)]
    public sealed class EventPublicationAttribute : Attribute
    {
        /// <summary>
        /// The URI of the event topic this publication refers to.
        /// </summary>
        private readonly string topic;

        /// <summary>
        /// Which scope matcher is used for this publication.
        /// </summary>
        private readonly Type scopeMatcherType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPublicationAttribute"/> class with
        /// global publication scope.
        /// </summary>
        /// <param name="topic">The topic URI.</param>
        public EventPublicationAttribute(string topic) : this(topic, typeof(PublishGlobal))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPublicationAttribute"/> class with 
        /// the specified publication scope matcher.
        /// </summary>
        /// <param name="topic">The topic URI.</param>
        /// <param name="scopeMatcherType">Type of the scope matcher.</param>
        public EventPublicationAttribute(string topic, Type scopeMatcherType)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException("topic must not be null or empty.", "topic");
            }
            
            this.topic = topic;
            this.scopeMatcherType = scopeMatcherType;
        }

        /// <summary>
        /// Gets the topic URI.
        /// </summary>
        /// <value>The topic URI.</value>
        public string Topic
        {
            get { return this.topic; }
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
