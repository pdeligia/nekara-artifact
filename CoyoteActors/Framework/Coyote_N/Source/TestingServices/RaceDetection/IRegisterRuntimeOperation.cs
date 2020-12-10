// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Coyote.TestingServices
{
    /// <summary>
    /// Interface to register interesting runtime operations.
    /// For race detection, the interesting operations are:
    /// 1. Reads and writes to the (shared) heap
    /// 2. Enqueues (posts) and dequeues (action begins)
    /// 3. Creation of a new actor
    /// In addition, this interface also allows clients to query
    /// the runtime for the currently running actor, and whether
    /// the runtime is in an action.
    /// </summary>
    public interface IRegisterRuntimeOperation
    {
        /// <summary>
        /// InAction[actorId.Value] = true iff the runtime executing an action
        /// in actor with Id actorId
        /// Reads and writes are instrumented only provided we're in an action.
        /// </summary>
        Dictionary<ulong, bool> InAction { get; set; }

        /// <summary>
        /// InMonitor = -1 iff the runtime is not inside a monitor
        /// and the monitor id otherwise
        /// </summary>
        long InMonitor { get; set; }

        /// <summary>
        /// Process a read to a heap location.
        /// </summary>
        /// <param name="source">The actor performing the read</param>
        /// <param name="sourceInformation"> Line number of this read</param>
        /// <param name="location">The base address for the heap location read</param>
        /// <param name="objHandle">The object handle</param>
        /// <param name="offset">The offset</param>
        /// <param name="isVolatile">Was the location declared volatile?</param>
        void RegisterRead(ulong source, string sourceInformation, UIntPtr location, UIntPtr objHandle, UIntPtr offset, bool isVolatile);

        /// <summary>
        /// Process a write to a heap location.
        /// </summary>
        /// <param name="source">The actor performing the write</param>
        /// <param name="sourceInformation"> Line number of this write</param>
        /// <param name="location">The base address for the heap location written</param>
        /// <param name="objHandle">The object handle</param>
        /// <param name="offset">The offset</param>
        /// <param name="isVolatile">Was the location declared volatile?</param>
        void RegisterWrite(ulong source, string sourceInformation, UIntPtr location, UIntPtr objHandle, UIntPtr offset, bool isVolatile);

        /// <summary>
        /// Process the enqueue of an event by an actor.
        /// </summary>
        /// <param name="source">The id of the actor that is the origin of the enqueue/post</param>
        /// <param name="target">The id of the actor receiving the event</param>
        /// <param name="e">The event sent</param>
        /// <param name="sequenceNumber">Is n if this is the n'th enqueue</param>
        void RegisterEnqueue(ActorId source, ActorId target, Event e, ulong sequenceNumber);

        /// <summary>
        /// Process the deq and begin of an action by an actor.
        /// </summary>
        /// <param name="source">The id of the actor that originally posted the event</param>
        /// <param name="target">The id of the actor processing the event</param>
        /// <param name="e">The event being processed</param>
        /// <param name="sequenceNumber">Is n if this is the n'th enqueue</param>
        void RegisterDequeue(ActorId source, ActorId target, Event e, ulong sequenceNumber);

        /// <summary>
        /// Update the internal data structures and vector clocks when an actor creates another actor.
        /// </summary>
        /// <param name="source">The id of the actor that is the creator.</param>
        /// <param name="target">The id of the actor that is freshly created.</param>
        void RegisterCreateActor(ActorId source, ActorId target);

        /// <summary>
        /// Set the runtime an implementer should forward TryGetCurrentActorId calls to.
        /// </summary>
        void RegisterRuntime(IActorRuntime runtime);

        /// <summary>
        /// Return true if the runtime is currently executing an actor's action.
        /// If it is, write its id to the out parameter as a ulong.
        /// </summary>
        bool TryGetCurrentActorId(out ulong actorId);

        /// <summary>
        /// Clear the internal state the reporter maintains.
        /// </summary>
        void ClearAll();
    }
}
