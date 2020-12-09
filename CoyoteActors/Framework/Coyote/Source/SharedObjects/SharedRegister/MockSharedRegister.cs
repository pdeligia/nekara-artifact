// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;

using Microsoft.CoyoteActors.TestingServices.Runtime;

namespace Microsoft.CoyoteActors.SharedObjects
{
    /// <summary>
    /// A wrapper for a shared register modeled using a state-actor for testing.
    /// </summary>
    internal sealed class MockSharedRegister<T> : ISharedRegister<T>
        where T : struct
    {
        /// <summary>
        /// Actor modeling the shared register.
        /// </summary>
        private readonly ActorId RegisterActor;

        /// <summary>
        /// The testing runtime hosting this shared register.
        /// </summary>
        private readonly SystematicTestingRuntime Runtime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockSharedRegister{T}"/> class.
        /// </summary>
        public MockSharedRegister(T value, SystematicTestingRuntime runtime)
        {
            this.Runtime = runtime;
            this.RegisterActor = this.Runtime.CreateActor(typeof(SharedRegisterActor<T>));
            this.Runtime.SendEvent(this.RegisterActor, SharedRegisterEvent.SetEvent(value));
        }

        /// <summary>
        /// Reads and updates the register.
        /// </summary>
        public T Update(Func<T, T> func)
        {
            var currentActor = this.Runtime.GetExecutingActor<Actor>();
            this.Runtime.SendEvent(this.RegisterActor, SharedRegisterEvent.UpdateEvent(func, currentActor.Id));
            var e = currentActor.Receive(typeof(SharedRegisterResponseEvent<T>)).Result as SharedRegisterResponseEvent<T>;
            return e.Value;
        }

        /// <summary>
        /// Gets current value of the register.
        /// </summary>
        public T GetValue()
        {
            var currentActor = this.Runtime.GetExecutingActor<Actor>();
            this.Runtime.SendEvent(this.RegisterActor, SharedRegisterEvent.GetEvent(currentActor.Id));
            var e = currentActor.Receive(typeof(SharedRegisterResponseEvent<T>)).Result as SharedRegisterResponseEvent<T>;
            return e.Value;
        }

        /// <summary>
        /// Sets current value of the register.
        /// </summary>
        public void SetValue(T value)
        {
            this.Runtime.SendEvent(this.RegisterActor, SharedRegisterEvent.SetEvent(value));
        }
    }
}
