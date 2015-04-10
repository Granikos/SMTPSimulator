//-------------------------------------------------------------------------------
// <copyright file="Subscription.cs" company="bbv Software Services AG">
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
    using System.Reflection;
    using Exceptions;
    using log4net;
    using ScopeMatchers;
    
    /// <summary>
    /// Represents a topic subscription.
    /// </summary>
    internal class Subscription
    {
        /// <summary>
        /// Logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Weak reference to the subscriber.
        /// </summary>
        private readonly WeakReference subscriber;

        /// <summary>
        /// Name of the handler method on the subscriber.
        /// </summary>
        private readonly string handlerMethodName;

        /// <summary>
        /// Method handle of the subscription handler.
        /// </summary>
        private readonly RuntimeMethodHandle methodHandle;

        /// <summary>
        /// Type of the event handler the subscription handler implements.
        /// </summary>
        private readonly Type eventHandlerType;

        /// <summary>
        /// Handle to the type of the subscriber.
        /// </summary>
        private readonly RuntimeTypeHandle typeHandle;

        /// <summary>
        /// The subscription scope matcher used for this subscription.
        /// </summary>
        private readonly ISubscriptionScopeMatcher subscriptionScopeMatcher;

        /// <summary>
        /// The handler used for this subscription.
        /// </summary>
        private readonly IHandler handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="handlerMethodName">Name of the handler method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <param name="handler">The handler used to execute the subscription.</param>
        /// <param name="subscriptionScopeMatcher">The subscription scope matcher used for this subscription.</param>
        internal Subscription(object subscriber, string handlerMethodName, Type[] parameterTypes, IHandler handler, ISubscriptionScopeMatcher subscriptionScopeMatcher)
        {
            this.subscriber = new WeakReference(subscriber);
            this.handlerMethodName = handlerMethodName;
            this.subscriptionScopeMatcher = subscriptionScopeMatcher;
            this.handler = handler;

            MethodInfo methodInfo = GetMethodInfo(subscriber, handlerMethodName, parameterTypes);
            if (methodInfo == null)
            {
                throw new SubscriberHandlerNotFoundException(subscriber.GetType(), handlerMethodName);
            }
            
            if (methodInfo.IsStatic)
            {
                throw new StaticSubscriberHandlerException(methodInfo);
            }

            this.typeHandle = subscriber.GetType().TypeHandle;
            this.methodHandle = methodInfo.MethodHandle;
            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (IsValidEventHandler(parameters))
            {
                ParameterInfo parameterInfo = methodInfo.GetParameters()[1];
                Type parameterType = parameterInfo.ParameterType;
                this.eventHandlerType = typeof(EventHandler<>).MakeGenericType(parameterType);
            }
            else
            {
                throw new InvalidSubscriptionSignatureException(methodInfo);
            }

            handler.Initalize(subscriber, handlerMethodName, parameterTypes);
        }

        /// <summary>
        /// Gets the type of the event handler this subscription is using.
        /// </summary>
        /// <value>The type of the event handler.</value>
        public Type EventHandlerType
        {
            get { return this.eventHandlerType; }
        }

        /// <summary>
        /// Gets the subscriber of the event.
        /// </summary>
        public object Subscriber
        {
            get { return this.subscriber.Target; }
        }

        /// <summary>
        /// Gets the handler method name that's subscribed to the event.
        /// </summary>
        public string HandlerMethodName
        {
            get { return this.handlerMethodName; }
        }

        /// <summary>
        /// Gets the subscription scope matcher.
        /// </summary>
        /// <value>The subscription scope matcher.</value>
        public ISubscriptionScopeMatcher SubscriptionScopeMatcher
        {
            get { return this.subscriptionScopeMatcher; }
        }

        /// <summary>
        /// Gets the handler that will be called by the <see cref="IEventTopic"/> during a firing sequence.
        /// </summary>
        /// <returns>A delegate that is used to call the subscription handler.</returns>
        public EventTopicFireDelegate GetHandler()
        {
            return this.EventTopicFireHandler;
        }

        /// <summary>
        /// Describes this subscription:
        /// name, thread option, scope, event args.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void DescribeTo(TextWriter writer)
        {
            if (this.subscriber.IsAlive)
            {
                writer.Write(this.Subscriber.GetType().FullName);

                if (this.Subscriber is INamedItem)
                {
                    writer.Write(", Name = ");
                    writer.Write(((INamedItem)Subscriber).EventBrokerItemName);
                }

                writer.Write(", Handler method = ");
                writer.Write(this.handlerMethodName);
                
                writer.Write(", Handler = ");
                writer.Write(this.handler);

                writer.Write(", EventArgs type = ");
                writer.Write(this.eventHandlerType.FullName);

                writer.Write(", scope matcher = ");
                this.subscriptionScopeMatcher.DescribeTo(writer);
            }
        }

        /// <summary>
        /// Determines whether the specified parameters are valid event handler parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>True if valid parameters.</returns>
        private static bool IsValidEventHandler(ParameterInfo[] parameters)
        {
            return parameters.Length == 2 && typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType);
        }

        /// <summary>
        /// Gets the method info of the handler method on the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="handlerMethodName">Name of the handler method.</param>
        /// <param name="parameterTypes">The parameter types to destinguish between overloaded methods.</param>
        /// <returns>The MethodInfo for the subscription handler on the subscriber.</returns>
        private static MethodInfo GetMethodInfo(object subscriber, string handlerMethodName, Type[] parameterTypes)
        {
            return parameterTypes != null ?
                subscriber.GetType().GetMethod(handlerMethodName, parameterTypes) :
                subscriber.GetType().GetMethod(handlerMethodName);
        }

        /// <summary>
        /// Handler that is called when a topic is fired.
        /// </summary>
        /// <param name="eventTopic">The event topic.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <param name="publication">The publication.</param>
        /// <param name="exceptions">The exceptions that occured during firing sequence.</param>
        private void EventTopicFireHandler(EventTopic eventTopic, object sender, EventArgs e, IPublication publication, List<Exception> exceptions)
        {
            if (this.Subscriber == null)
            {
                return;
            }
            
            INamedItem namedPublisher = publication.Publisher as INamedItem;
            INamedItem namedSubscriber = this.subscriber as INamedItem;

            Delegate subscriptionHandler = this.CreateSubscriptionDelegate();
            if (subscriptionHandler == null)
            {
                return;
            }

            log.DebugFormat(
                "Relaying event '{6}' from publisher '{0}' [{1}] to subscriber '{2}' [{3}] with EventArgs '{4}' with handler '{5}'.",
                publication.Publisher,
                namedPublisher != null ? namedPublisher.EventBrokerItemName : string.Empty,
                this.subscriber,
                namedSubscriber != null ? namedSubscriber.EventBrokerItemName : string.Empty,
                e,
                this.handler,
                eventTopic.Uri);

            this.handler.Handle(sender, e, subscriptionHandler, exceptions);

            log.DebugFormat(
                "Relayed event '{6}' from publisher '{0}' [{1}] to subscriber '{2}' [{3}] with EventArgs '{4}' with handler '{5}'.",
                publication.Publisher,
                namedPublisher != null ? namedPublisher.EventBrokerItemName : string.Empty,
                this.subscriber,
                namedSubscriber != null ? namedSubscriber.EventBrokerItemName : string.Empty,
                e,
                this.handler,
                eventTopic.Uri);
        }

        /// <summary>
        /// Creates the subscription delegate.
        /// </summary>
        /// <returns>A delegate that is used to call the subscription handler method.</returns>
        private Delegate CreateSubscriptionDelegate()
        {
            object s = this.subscriber.Target;
            return s != null ? 
                Delegate.CreateDelegate(
                    this.eventHandlerType, 
                    s, 
                    (MethodInfo)MethodBase.GetMethodFromHandle(this.methodHandle, this.typeHandle)) : 
                null;
        }
    }
}
