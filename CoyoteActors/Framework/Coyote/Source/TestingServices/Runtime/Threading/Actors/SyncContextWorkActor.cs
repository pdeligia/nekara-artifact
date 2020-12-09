// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CoyoteActors.TestingServices.Runtime;
using Microsoft.CoyoteActors.Threading;

namespace Microsoft.CoyoteActors.TestingServices.Threading
{
    /// <summary>
    /// Implements an actor that can execute a <see cref="SynchronizationContext"/> callback asynchronously.
    /// </summary>
    internal sealed class SyncContextWorkActor : WorkActor
    {
        /// <summary>
        /// Callback to be executed asynchronously.
        /// </summary>
        private readonly SendOrPostCallback Callback;

        /// <summary>
        /// State of the callback.
        /// </summary>
        private readonly object State;

        /// <summary>
        /// Provides the capability to await for work completion.
        /// </summary>
        private readonly TaskCompletionSource<object> Awaiter;

        /// <summary>
        /// Task that provides access to the completed work.
        /// </summary>
        internal Task AwaiterTask => this.Awaiter.Task;

        /// <summary>
        /// The id of the task that provides access to the completed work.
        /// </summary>
        internal override int AwaiterTaskId => this.AwaiterTask.Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncContextWorkActor"/> class.
        /// </summary>
        internal SyncContextWorkActor(SystematicTestingRuntime runtime, SendOrPostCallback callback, object state)
            : base(runtime)
        {
            this.Callback = callback;
            this.State = state;
            this.Awaiter = new TaskCompletionSource<object>();
        }

        /// <summary>
        /// Executes the work asynchronously.
        /// </summary>
        internal override Task ExecuteAsync()
        {
            IO.Debug.WriteLine($"Actor '{this.Id}' is executing sync context callback on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            this.Callback(this.State);
            IO.Debug.WriteLine($"Actor '{this.Id}' executed sync context callback on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            this.Awaiter.SetResult(default);
            IO.Debug.WriteLine($"Actor '{this.Id}' completed sync context callback on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            return Task.CompletedTask;
        }
    }
}
