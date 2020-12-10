// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using Microsoft.Coyote.TestingServices.Runtime;

namespace Microsoft.Coyote.SharedObjects
{
    /// <summary>
    /// A wrapper for a shared counter modeled using a state-actor for testing.
    /// </summary>
    internal sealed class MockSharedCounter : ISharedCounter
    {
        /// <summary>
        /// Actor modeling the shared counter.
        /// </summary>
        private readonly ActorId CounterActor;

        /// <summary>
        /// The testing runtime hosting this shared counter.
        /// </summary>
        private readonly SystematicTestingRuntime Runtime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockSharedCounter"/> class.
        /// </summary>
        public MockSharedCounter(int value, SystematicTestingRuntime runtime)
        {
            this.Runtime = runtime;
            this.CounterActor = this.Runtime.CreateActor(typeof(SharedCounterActor));
            var currentActor = this.Runtime.GetExecutingActor<Actor>();
            this.Runtime.SendEvent(this.CounterActor, SharedCounterEvent.SetEvent(currentActor.Id, value));
            currentActor.Receive(typeof(SharedCounterResponseEvent)).Wait();
        }

        /// <summary>
        /// Increments the shared counter.
        /// </summary>
        public void Increment()
        {
            this.Runtime.SendEvent(this.CounterActor, SharedCounterEvent.IncrementEvent());
        }

        /// <summary>
        /// Decrements the shared counter.
        /// </summary>
        public void Decrement()
        {
            this.Runtime.SendEvent(this.CounterActor, SharedCounterEvent.DecrementEvent());
        }

        /// <summary>
        /// Gets the current value of the shared counter.
        /// </summary>
        public int GetValue()
        {
            var currentActor = this.Runtime.GetExecutingActor<Actor>();
            this.Runtime.SendEvent(this.CounterActor, SharedCounterEvent.GetEvent(currentActor.Id));
            var response = currentActor.Receive(typeof(SharedCounterResponseEvent)).Result;
            return (response as SharedCounterResponseEvent).Value;
        }

        /// <summary>
        /// Adds a value to the counter atomically.
        /// </summary>
        public int Add(int value)
        {
            var currentActor = this.Runtime.GetExecutingActor<Actor>();
            this.Runtime.SendEvent(this.CounterActor, SharedCounterEvent.AddEvent(currentActor.Id, value));
            var response = currentActor.Receive(typeof(SharedCounterResponseEvent)).Result;
            return (response as SharedCounterResponseEvent).Value;
        }

        /// <summary>
        /// Sets the counter to a value atomically.
        /// </summary>
        public int Exchange(int value)
        {
            var currentActor = this.Runtime.GetExecutingActor<Actor>();
            this.Runtime.SendEvent(this.CounterActor, SharedCounterEvent.SetEvent(currentActor.Id, value));
            var response = currentActor.Receive(typeof(SharedCounterResponseEvent)).Result;
            return (response as SharedCounterResponseEvent).Value;
        }

        /// <summary>
        /// Sets the counter to a value atomically if it is equal to a given value.
        /// </summary>
        public int CompareExchange(int value, int comparand)
        {
            var currentActor = this.Runtime.GetExecutingActor<Actor>();
            this.Runtime.SendEvent(this.CounterActor, SharedCounterEvent.CasEvent(currentActor.Id, value, comparand));
            var response = currentActor.Receive(typeof(SharedCounterResponseEvent)).Result;
            return (response as SharedCounterResponseEvent).Value;
        }
    }
}
