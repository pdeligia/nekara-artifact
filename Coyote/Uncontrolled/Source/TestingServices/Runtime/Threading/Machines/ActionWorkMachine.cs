﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Microsoft.Coyote.TestingServices.Runtime;
using Microsoft.Coyote.Threading;

namespace Microsoft.Coyote.TestingServices.Threading
{
    /// <summary>
    /// Implements a machine that can execute an <see cref="Action"/> asynchronously.
    /// </summary>
    internal sealed class ActionWorkMachine : WorkMachine
    {
        /// <summary>
        /// Work to be executed asynchronously.
        /// </summary>
        private readonly Action Work;

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
        /// Initializes a new instance of the <see cref="ActionWorkMachine"/> class.
        /// </summary>
        internal ActionWorkMachine(SystematicTestingRuntime runtime, Action work)
            : base(runtime)
        {
            this.Work = work;
            this.Awaiter = new TaskCompletionSource<object>();
        }

        /// <summary>
        /// Executes the work asynchronously.
        /// </summary>
        internal override Task ExecuteAsync()
        {
            IO.Debug.WriteLine($"Machine '{this.Id}' is executing action on task '{MachineTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            this.Work();
            IO.Debug.WriteLine($"Machine '{this.Id}' executed action on task '{MachineTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            this.Awaiter.SetResult(default);
            IO.Debug.WriteLine($"Machine '{this.Id}' completed action on task '{MachineTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            return Task.CompletedTask;
        }
    }
}
