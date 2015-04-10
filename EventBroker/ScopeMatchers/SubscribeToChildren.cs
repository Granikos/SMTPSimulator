//-------------------------------------------------------------------------------
// <copyright file="SubscribeToChildren.cs" company="bbv Software Services AG">
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

namespace bbv.Common.EventBroker.ScopeMatchers
{
    /// <summary>
    /// Matcher for subscriptions to events from children only.
    /// </summary>
    public class SubscribeToChildren : ISubscriptionScopeMatcher
    {
        /// <summary>
        /// Returns whether the publisher and subscriber match and the event published by the
        /// publisher will be relayed to the subscriber.
        /// <para>
        /// This is the case if the name of the subscriber is a prefix to the name of the publisher.
        /// </para>
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <param name="subscriberName">Name of the subscriber.</param>
        /// <returns><code>true</code> if event has to be sent to the subscriber.</returns>
        public bool Match(string publisherName, string subscriberName)
        {
            return publisherName.StartsWith(subscriberName);
        }

        /// <summary>
        /// Describes this scope matcher.
        /// </summary>
        /// <param name="writer">The writer the description is written to.</param>
        public void DescribeTo(System.IO.TextWriter writer)
        {
            writer.Write("publisher name starts with subscriber name");
        }
    }
}