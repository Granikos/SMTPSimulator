//-------------------------------------------------------------------------------
// <copyright file="EventInspector.cs" company="bbv Software Services AG">
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
    using System.Reflection;

    /// <summary>
    /// The <see cref="EventInspector"/> scans classes for publications or subscriptions.
    /// </summary>
    internal class EventInspector
    {
        /// <summary>
        /// Processes a publishers.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <param name="register">true to register publications, false to unregister them.</param>
        /// <param name="eventTopicHost">The event topic host.</param>
        /// <param name="factory">The factory to create handlers and scope matchers.</param>
        /// <remarks>Scans the members of the <paramref name="publisher"/> and registers or unregisters publications.</remarks>
        public void ProcessPublisher(object publisher, bool register, IEventTopicHost eventTopicHost, IFactory factory)
        {
            var type = publisher.GetType();
            foreach (EventInfo info in publisher.GetType().GetEvents())
            {
                foreach (EventPublicationAttribute attr in info.GetCustomAttributes(typeof(EventPublicationAttribute), true))
                {
                    HandlePublisher(publisher, register, info, attr, eventTopicHost, factory);
                }
            }
        }

        /// <summary>
        /// Processes the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="register">true to register subscriptions, false to unregister them.</param>
        /// <param name="eventTopicHost">The event topic host.</param>
        /// <remarks>Scans the members of the <paramref name="subscriber"/> and registers or unregisters subscriptions.</remarks>
        /// <param name="factory">The factory to create handlers and scope matchers.</param>
        public void ProcessSubscriber(object subscriber, bool register, IEventTopicHost eventTopicHost, IFactory factory)
        {
            foreach (MethodInfo info in subscriber.GetType().GetMethods())
            {
                foreach (EventSubscriptionAttribute attr in info.GetCustomAttributes(typeof(EventSubscriptionAttribute), true))
                {
                    HandleSubscriber(subscriber, register, info, attr, eventTopicHost, factory);
                }
            }
        }

        /// <summary>
        /// Gets the parameter types for a method.
        /// </summary>
        /// <param name="info">method info</param>
        /// <returns>Array of the types of the parameters of the specified method.</returns>
        private static Type[] GetParamTypes(MethodInfo info)
        {
            ParameterInfo[] paramInfos = info.GetParameters();
            Type[] paramTypes = new Type[paramInfos.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                paramTypes[i] = paramInfos[i].ParameterType;
            }

            return paramTypes;
        }

        /// <summary>
        /// Gets the event topic with the specified URI from the specified host.
        /// </summary>
        /// <param name="eventTopicHost">The event topic host.</param>
        /// <param name="topic">The topic URI.</param>
        /// <returns>The requested event topic, either newly created or the one that already existed.</returns>
        private static IEventTopic GetEventTopic(IEventTopicHost eventTopicHost, string topic)
        {
            if (eventTopicHost.EventTopics.ContainsKey(topic))
            {
                return eventTopicHost.EventTopics[topic];
            }
            
            EventTopic eventTopic = new EventTopic(topic);
            eventTopicHost.EventTopics.Add(topic, eventTopic);
            return eventTopic;
        }

        /// <summary>
        /// Handles the publisher.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <param name="register">true to register publications, false to unregister them.</param>
        /// <param name="info">The published event..</param>
        /// <param name="attr">The attribute</param>
        /// <param name="eventTopicHost">The event topic host.</param>
        /// <param name="factory">The factory to create handlers and scope matchers.</param>
        private static void HandlePublisher(
            object publisher, 
            bool register, 
            EventInfo info, 
            EventPublicationAttribute attr, 
            IEventTopicHost eventTopicHost, 
            IFactory factory)
        {
            IEventTopic topic = GetEventTopic(eventTopicHost, attr.Topic);
            if (register)
            {
                topic.AddPublication(publisher, info.Name, factory.CreatePublicationScopeMatcher(attr.ScopeMatcherType));
            }
            else
            {
                topic.RemovePublication(publisher, info.Name);
            }
        }

        /// <summary>
        /// Handles the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="register">true to register subscriptions, false to unregister them.</param>
        /// <param name="info">The handler method.</param>
        /// <param name="attr">The subscription attribute.</param>
        /// <param name="eventTopicHost">The event topic host.</param>
        /// <param name="factory">The factory to create handlers and scope matchers.</param>
        private static void HandleSubscriber(
            object subscriber, 
            bool register, 
            MethodInfo info, 
            EventSubscriptionAttribute attr,
            IEventTopicHost eventTopicHost, 
            IFactory factory)
        {
            IEventTopic topic = GetEventTopic(eventTopicHost, attr.Topic);
            if (register)
            {
                Type[] paramTypes = GetParamTypes(info);

                topic.AddSubscription(
                    subscriber, 
                    info.Name, 
                    paramTypes, 
                    factory.CreateHandler(attr.HandlerType), 
                    factory.CreateSubscriptionScopeMatcher(attr.ScopeMatcherType));
            }
            else
            {
                topic.RemoveSubscription(subscriber, info.Name);
            }
        }
    }
}
