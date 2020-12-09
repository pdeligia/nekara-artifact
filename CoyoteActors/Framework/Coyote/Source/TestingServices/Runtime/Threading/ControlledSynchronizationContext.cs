// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Threading;

using Microsoft.CoyoteActors.TestingServices.Runtime;

namespace Microsoft.CoyoteActors.TestingServices.Threading
{
    /// <summary>
    /// A <see cref="SynchronizationContext"/> that can be controlled during testing.
    /// </summary>
    internal sealed class ControlledSynchronizationContext : SynchronizationContext
    {
        /// <summary>
        /// The runtime that is executing this synchronization context.
        /// </summary>
        private readonly SystematicTestingRuntime Runtime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlledSynchronizationContext"/> class.
        /// </summary>
        public ControlledSynchronizationContext(SystematicTestingRuntime runtime)
        {
            this.Runtime = runtime;
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            this.Runtime.DispatchWork(new SyncContextWorkActor(this.Runtime, d, state), null);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            this.Runtime.DispatchWork(new SyncContextWorkActor(this.Runtime, d, state), null);
        }
    }
}
