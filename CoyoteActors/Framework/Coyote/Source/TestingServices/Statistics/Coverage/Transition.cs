// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.Coyote.TestingServices.Coverage
{
    /// <summary>
    /// Specifies a program transition.
    /// </summary>
    [DataContract]
    public struct Transition
    {
        /// <summary>
        /// The origin actor.
        /// </summary>
        [DataMember]
        public readonly string ActorOrigin;

        /// <summary>
        /// The origin state.
        /// </summary>
        [DataMember]
        public readonly string StateOrigin;

        /// <summary>
        /// The edge label.
        /// </summary>
        [DataMember]
        public readonly string EdgeLabel;

        /// <summary>
        /// The target actor.
        /// </summary>
        [DataMember]
        public readonly string ActorTarget;

        /// <summary>
        /// The target state.
        /// </summary>
        [DataMember]
        public readonly string StateTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transition"/> struct.
        /// </summary>
        public Transition(string actorOrigin, string stateOrigin, string edgeLabel,
            string actorTarget, string stateTarget)
        {
            this.ActorOrigin = actorOrigin;
            this.StateOrigin = stateOrigin;
            this.EdgeLabel = edgeLabel;
            this.ActorTarget = actorTarget;
            this.StateTarget = stateTarget;
        }

        /// <summary>
        /// Pretty print.
        /// </summary>
        public override string ToString()
        {
            if (this.ActorOrigin == this.ActorTarget)
            {
                return string.Format("{0}: {1} --{2}--> {3}", this.ActorOrigin, this.StateOrigin, this.EdgeLabel, this.StateTarget);
            }

            return string.Format("({0}, {1}) --{2}--> ({3}, {4})", this.ActorOrigin, this.StateOrigin, this.EdgeLabel, this.ActorTarget, this.StateTarget);
        }
    }
}
