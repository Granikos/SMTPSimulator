//-------------------------------------------------------------------------------
// <copyright file="IEventTopicHost.cs" company="bbv Software Services AG">
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
    using System.IO;

    /// <summary>
    /// An event topic hosts is context where publications and subscriptions are wired together through <see cref="IEventTopic"/>s.
    /// </summary>
    /// <remarks>
    /// Only publications and subscription in the same host are wired together. You can use several event topic hosts by exchanging them during registration on the <see cref="EventBroker"/>.
    /// </remarks>
    internal interface IEventTopicHost : IDisposable
    {
        /// <summary>
        /// Gets the event topics.
        /// </summary>
        /// <remarks>
        /// Returns a nonnull instance of the dictionary.
        /// </remarks>
        /// <value>The event topics.</value>
        Dictionary<string, EventTopic> EventTopics { get; }

        /// <summary>
        /// Describes all event topics:
        /// publications, subscriptions, names, thread options, scopes, event args.
        /// </summary>
        /// <param name="writer">The writer.</param>
        void DescribeTo(TextWriter writer);
    }
}
