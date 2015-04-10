//-------------------------------------------------------------------------------
// <copyright file="IFactory.cs" company="bbv Software Services AG">
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
    /// Factory for creating subscription execution handlers and scope matcher.
    /// </summary>
    /// <remarks>
    /// The factory that is used by an <see cref="IEventBroker"/> can be set on the constructor of the event broker.
    /// </remarks>
    public interface IFactory
    {
        /// <summary>
        /// Creates a subscription execution handler. This handler defines on which thread the subscription is executed.
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        /// <returns>A new subscription execution handler.</returns>
        IHandler CreateHandler(Type handlerType);

        /// <summary>
        /// Creates a publication scope matcher.
        /// </summary>
        /// <param name="scopeMatcherType">Type of the scope matcher.</param>
        /// <returns>A newly created publication scope matcher.</returns>
        IPublicationScopeMatcher CreatePublicationScopeMatcher(Type scopeMatcherType);

        /// <summary>
        /// Creates a subscription scope matcher.
        /// </summary>
        /// <param name="scopeMatcherType">Type of the scope matcher.</param>
        /// <returns>A newly create subscription scope matcher.</returns>
        ISubscriptionScopeMatcher CreateSubscriptionScopeMatcher(Type scopeMatcherType);
    }
}