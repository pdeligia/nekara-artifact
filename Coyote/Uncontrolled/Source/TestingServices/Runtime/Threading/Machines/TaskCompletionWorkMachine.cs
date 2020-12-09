// ------------------------------------------------------------------------------------------------
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
    /// Implements an actor that can complete a task asynchronously.
    /// </summary>
    internal sealed class TaskCompletionWorkActor : WorkActor
    {
        /// <summary>
        /// Task that provides access to the completed work.
        /// </summary>
        internal readonly Task AwaiterTask;

        /// <summary>
        /// The id of the task that provides access to the completed work.
        /// </summary>
        internal override int AwaiterTaskId => this.AwaiterTask.Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionWorkActor"/> class.
        /// </summary>
        internal TaskCompletionWorkActor(SystematicTestingRuntime runtime, Task task)
            : base(runtime)
        {
            this.AwaiterTask = task;
        }

        /// <summary>
        /// Executes the work asynchronously.
        /// </summary>
        internal override Task ExecuteAsync()
        {
            IO.Debug.WriteLine($"Actor '{this.Id}' completed task '{this.AwaiterTask.Id}' on task '{ActorTask.CurrentId}'");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Implements an actor that can complete a task asynchronously.
    /// </summary>
    internal sealed class TaskCompletionWorkActor<TResult> : WorkActor
    {
        /// <summary>
        /// Task that provides access to the completed work.
        /// </summary>
        internal readonly Task<TResult> AwaiterTask;

        /// <summary>
        /// The id of the task that provides access to the completed work.
        /// </summary>
        internal override int AwaiterTaskId => this.AwaiterTask.Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionWorkActor{TResult}"/> class.
        /// </summary>
        internal TaskCompletionWorkActor(SystematicTestingRuntime runtime, Task<TResult> task)
            : base(runtime)
        {
            this.AwaiterTask = task;
        }

        /// <summary>
        /// Executes the work asynchronously.
        /// </summary>
        internal override Task ExecuteAsync()
        {
            Console.WriteLine($"\n\nActor '{this.Id}' completed task '{this.AwaiterTask.Id}' on task '{ActorTask.CurrentId}'\n\n");
            return Task.CompletedTask;
        }
    }
}
