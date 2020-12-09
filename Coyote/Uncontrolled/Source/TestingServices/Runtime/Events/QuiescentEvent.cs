// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.Coyote.TestingServices.Runtime
{
    /// <summary>
    /// Signals that an actor has reached quiescence.
    /// </summary>
    [DataContract]
    internal sealed class QuiescentEvent : Event
    {
        /// <summary>
        /// The id of the actor that has reached quiescence.
        /// </summary>
        public ActorId ActorId;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuiescentEvent"/> class.
        /// </summary>
        /// <param name="mid">The id of the actor that has reached quiescence.</param>
        public QuiescentEvent(ActorId mid)
        {
            this.ActorId = mid;
        }
    }
}
