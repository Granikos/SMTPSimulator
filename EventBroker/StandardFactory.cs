//-------------------------------------------------------------------------------
// <copyright file="StandardFactory.cs" company="bbv Software Services AG">
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
    /// Standard implementation for the <see cref="IFactory"/> interface.
    /// </summary>
    public class StandardFactory : IFactory
    {
        /// <summary>
        /// Creates a subscription execution handler. This handler defines on which thread the subscription is executed.
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        /// <returns>A new subscription execution handler.</returns>
        public virtual IHandler CreateHandler(Type handlerType)
        {
            if (!handlerType.IsClass || !typeof(IHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException("handlerType '" + handlerType + "' has to be a class implementing bbv.Common.EventBroker.IHandler.");
            }

            return (IHandler)Activator.CreateInstance(handlerType);
        }

        /// <summary>
        /// Creates a publication scope matcher.
        /// </summary>
        /// <param name="scopeMatcherType">Type of the scope matcher.</param>
        /// <returns>
        /// A newly created publication scope matcher.
        /// </returns>
        public virtual IPublicationScopeMatcher CreatePublicationScopeMatcher(Type scopeMatcherType)
        {
            if (!scopeMatcherType.IsClass || !typeof(IPublicationScopeMatcher).IsAssignableFrom(scopeMatcherType))
            {
                throw new ArgumentException("scopeMatcherType '" + scopeMatcherType + "' has to be a class implementing bbv.Common.EventBroker.ScopeMatchers.IPublicationScopeMatcher.");
            }

            return (IPublicationScopeMatcher)Activator.CreateInstance(scopeMatcherType);
        }

        /// <summary>
        /// Creates a subscription scope matcher.
        /// </summary>
        /// <param name="scopeMatcherType">Type of the scope matcher.</param>
        /// <returns>
        /// A newly create subscription scope matcher.
        /// </returns>
        public virtual ISubscriptionScopeMatcher CreateSubscriptionScopeMatcher(Type scopeMatcherType)
        {
            if (!scopeMatcherType.IsClass || !typeof(ISubscriptionScopeMatcher).IsAssignableFrom(scopeMatcherType))
            {
                throw new ArgumentException("scopeMatcherType '" + scopeMatcherType + "' has to be a class implementing bbv.Common.EventBroker.ScopeMatchers.ISubscriptionScopeMatcher.");
            }

            return (ISubscriptionScopeMatcher)Activator.CreateInstance(scopeMatcherType);
        }
    }
}