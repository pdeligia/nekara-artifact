// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;

using Microsoft.CoyoteActors.IO;
using Microsoft.CoyoteActors.Utilities;

namespace Microsoft.CoyoteActors.Runtime
{
    /// <summary>
    /// Implements an actor that can execute asynchronously.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class AsyncActor
    {
        /// <summary>
        /// The runtime that executes this actor.
        /// </summary>
        internal ActorRuntime Runtime { get; private set; }

        /// <summary>
        /// The unique actor id.
        /// </summary>
        protected internal ActorId Id { get; private set; }

        /// <summary>
        /// Id used to identify subsequent operations performed by this actor.
        /// </summary>
        protected internal abstract Guid OperationGroupId { get; set; }

        /// <summary>
        /// The logger installed to the Coyote runtime.
        /// </summary>
        protected ILogger Logger => this.Runtime.Logger;

        /// <summary>
        /// Initializes this actor.
        /// </summary>
        internal void Initialize(ActorRuntime runtime, ActorId mid)
        {
            this.Runtime = runtime;
            this.Id = mid;
        }

        /// <summary>
        /// Returns the hashed state of the actor using the specified level of abstraction.
        /// </summary>
        internal virtual int GetHashedState(AbstractionLevel abstractionLevel) => 0;

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is AsyncActor m &&
                this.GetType() == m.GetType())
            {
                return this.Id.Value == m.Id.Value;
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return this.Id.Value.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current actor.
        /// </summary>
        public override string ToString()
        {
            return this.Id.Name;
        }
    }
}
