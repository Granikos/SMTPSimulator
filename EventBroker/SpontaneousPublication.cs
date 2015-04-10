//-------------------------------------------------------------------------------
// <copyright file="SpontaneousPublication.cs" company="bbv Software Services AG">
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
    using ScopeMatchers;

    /// <summary>
    /// A spontaneous publiction is used when there is no real publisher but 
    /// <see cref="EventBroker.Fire"/> was called directly to fire an event.
    /// </summary>
    public class SpontaneousPublication : IPublication
    {
        /// <summary>
        /// The publisher.
        /// </summary>
        private readonly object publisher;

        /// <summary>
        /// The publication scope matcher used for this publication.
        /// </summary>
        private readonly IPublicationScopeMatcher publicationScopeMatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpontaneousPublication"/> class.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <param name="publicationScopeMatcher">The publication scope matcher.</param>
        public SpontaneousPublication(object publisher, IPublicationScopeMatcher publicationScopeMatcher)
        {
            this.publisher = publisher;
            this.publicationScopeMatcher = publicationScopeMatcher;
        }

        #region IPublication Members

        /// <summary>
        /// Gets the publisher of the event.
        /// </summary>
        public object Publisher
        {
            get { return this.publisher; }
        }

        /// <summary>
        /// Gets the publication scope matcher.
        /// </summary>
        /// <value>The publication scope matcher.</value>
        public IPublicationScopeMatcher PublicationScopeMatcher
        {
            get { return this.publicationScopeMatcher; }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // nothing to dispose here.
        }

        #endregion
    }
}