//-------------------------------------------------------------------------------
// <copyright file="EventTopicHost.cs" company="bbv Software Services AG">
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
    /// Default implementation of a <see cref="IEventTopicHost"/>.
    /// </summary>
    internal class EventTopicHost : IEventTopicHost
    {
        /// <summary>
        /// Map from event topic URI to event topic instance.
        /// </summary>
        private readonly Dictionary<string, EventTopic> eventTopics = new Dictionary<string, EventTopic>();

        /// <summary>
        /// Gets the event topics.
        /// </summary>
        /// <value>The event topics.</value>
        /// <remarks>
        /// Returns a nonnull instance of the dictionary.
        /// </remarks>
        public Dictionary<string, EventTopic> EventTopics
        {
            get { return this.eventTopics; }
        }

        /// <summary>
        /// Describes all event topics:
        /// publications, subscriptions, names, thread options, scopes, event args.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void DescribeTo(TextWriter writer)
        {
            foreach (IEventTopic eventTopic in this.EventTopics.Values)
            {
                eventTopic.DescribeTo(writer);
                writer.WriteLine();
            }
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
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IEventTopic eventTopic in this.eventTopics.Values)
                {
                    eventTopic.Dispose();
                }

                this.eventTopics.Clear();
            }
        }

        #endregion
    }
}
