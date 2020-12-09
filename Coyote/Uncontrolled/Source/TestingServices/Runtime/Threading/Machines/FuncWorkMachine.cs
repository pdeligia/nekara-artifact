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
    /// Implements an actor that can execute a <see cref="Func{ActorTask}"/> asynchronously.
    /// </summary>
    internal sealed class FuncWorkActor : WorkActor
    {
        /// <summary>
        /// Work to be executed asynchronously.
        /// </summary>
        private readonly Func<Task> Work;

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
        /// Initializes a new instance of the <see cref="FuncWorkActor"/> class.
        /// </summary>
        internal FuncWorkActor(SystematicTestingRuntime runtime, Func<Task> work)
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
            IO.Debug.WriteLine($"Actor '{this.Id}' is executing function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            Task task = this.Work();
            this.Runtime.NotifyWaitTask(this, task);
            IO.Debug.WriteLine($"Actor '{this.Id}' executed function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            this.Awaiter.SetResult(default);
            IO.Debug.WriteLine($"Actor '{this.Id}' completed function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Implements an actor that can execute a <see cref="Func{TResult}"/> asynchronously.
    /// </summary>
    internal sealed class FuncWorkActor<TResult> : WorkActor
    {
        /// <summary>
        /// Work to be executed asynchronously.
        /// </summary>
        private readonly Func<TResult> Work;

        /// <summary>
        /// Provides the capability to await for work completion.
        /// </summary>
        private readonly TaskCompletionSource<TResult> Awaiter;

        /// <summary>
        /// Task that provides access to the completed work.
        /// </summary>
        internal Task<TResult> AwaiterTask => this.Awaiter.Task;

        /// <summary>
        /// The id of the task that provides access to the completed work.
        /// </summary>
        internal override int AwaiterTaskId => this.AwaiterTask.Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncWorkActor{TResult}"/> class.
        /// </summary>
        internal FuncWorkActor(SystematicTestingRuntime runtime, Func<TResult> work)
            : base(runtime)
        {
            this.Work = work;
            this.Awaiter = new TaskCompletionSource<TResult>();
        }

        /// <summary>
        /// Executes the work asynchronously.
        /// </summary>
        internal override Task ExecuteAsync()
        {
            IO.Debug.WriteLine($"Actor '{this.Id}' is executing function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            TResult result = this.Work();
            IO.Debug.WriteLine($"Actor '{this.Id}' executed function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            this.Awaiter.SetResult(result);
            IO.Debug.WriteLine($"Actor '{this.Id}' completed function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Implements an actor that can execute a <see cref="Func{TResult}"/> asynchronously.
    /// </summary>
    internal sealed class FuncTaskWorkActor<TResult> : WorkActor
    {
        /// <summary>
        /// Work to be executed asynchronously.
        /// </summary>
        private readonly Func<Task<TResult>> Work;

        /// <summary>
        /// Provides the capability to await for work completion.
        /// </summary>
        private readonly TaskCompletionSource<TResult> Awaiter;

        /// <summary>
        /// Task that provides access to the completed work.
        /// </summary>
        internal Task<TResult> AwaiterTask => this.Awaiter.Task;

        /// <summary>
        /// The id of the task that provides access to the completed work.
        /// </summary>
        internal override int AwaiterTaskId => this.AwaiterTask.Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncTaskWorkActor{TResult}"/> class.
        /// </summary>
        internal FuncTaskWorkActor(SystematicTestingRuntime runtime, Func<Task<TResult>> work)
            : base(runtime)
        {
            this.Work = work;
            this.Awaiter = new TaskCompletionSource<TResult>();
        }

        /// <summary>
        /// Executes the work asynchronously.
        /// </summary>
        internal override Task ExecuteAsync()
        {
            IO.Debug.WriteLine($"Actor '{this.Id}' is executing function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            Task<TResult> task = this.Work();
            IO.Debug.WriteLine($"Actor '{this.Id}' is getting result on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            this.Runtime.NotifyWaitTask(this, task);
            IO.Debug.WriteLine($"Actor '{this.Id}' executed function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            this.Awaiter.SetResult(task.Result);
            IO.Debug.WriteLine($"Actor '{this.Id}' completed function on task '{ActorTask.CurrentId}' (tcs: {this.Awaiter.Task.Id})");
            return Task.CompletedTask;
        }
    }
}
