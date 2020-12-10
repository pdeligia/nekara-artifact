// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.CoyoteActors.SharedObjects
{
    /// <summary>
    /// A shared register modeled using a state-actor for testing.
    /// </summary>
    internal sealed class SharedRegisterActor<T> : Actor
        where T : struct
    {
        /// <summary>
        /// The value of the shared register.
        /// </summary>
        private T Value;

        /// <summary>
        /// The start state of this actor.
        /// </summary>
        [Start]
        [OnEntry(nameof(Initialize))]
        [OnEventDoAction(typeof(SharedRegisterEvent), nameof(ProcessEvent))]
        private class Init : ActorState
        {
        }

        /// <summary>
        /// Initializes the actor.
        /// </summary>
        private void Initialize()
        {
            this.Value = default;
        }

        /// <summary>
        /// Processes the next dequeued event.
        /// </summary>
        private void ProcessEvent()
        {
            var e = this.ReceivedEvent as SharedRegisterEvent;
            switch (e.Operation)
            {
                case SharedRegisterEvent.SharedRegisterOperation.SET:
                    this.Value = (T)e.Value;
                    break;

                case SharedRegisterEvent.SharedRegisterOperation.GET:
                    this.Send(e.Sender, new SharedRegisterResponseEvent<T>(this.Value));
                    break;

                case SharedRegisterEvent.SharedRegisterOperation.UPDATE:
                    var func = (Func<T, T>)e.Func;
                    this.Value = func(this.Value);
                    this.Send(e.Sender, new SharedRegisterResponseEvent<T>(this.Value));
                    break;
            }
        }
    }
}
