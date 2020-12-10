// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.CoyoteActors.Runtime
{
    /// <summary>
    /// Contains the origin information of an <see cref="Event"/>.
    /// </summary>
    [DataContract]
    internal class EventOriginInfo
    {
        /// <summary>
        /// The sender actor id.
        /// </summary>
        [DataMember]
        internal ActorId SenderActorId { get; private set; }

        /// <summary>
        /// The sender actor name.
        /// </summary>
        [DataMember]
        internal string SenderActorName { get; private set; }

        /// <summary>
        /// The sender actor state name.
        /// </summary>
        [DataMember]
        internal string SenderStateName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventOriginInfo"/> class.
        /// </summary>
        internal EventOriginInfo(ActorId senderActorId, string senderActorName, string senderStateName)
        {
            this.SenderActorId = senderActorId;
            this.SenderActorName = senderActorName;
            this.SenderStateName = senderStateName;
        }
    }
}
