// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Microsoft.Coyote.Threading
{
    /// <summary>
    /// Implements a <see cref="ActorTask"/> awaiter.
    /// </summary>
    public readonly struct ActorTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
    {
        // WARNING: The layout must remain the same, as the struct is used to access
        // the generic ActorTaskAwaiter<> as ActorTaskAwaiter.

        /// <summary>
        /// The actor task being awaited.
        /// </summary>
        private readonly ActorTask ActorTask;

        /// <summary>
        /// The task awaiter.
        /// </summary>
        private readonly TaskAwaiter Awaiter;

        /// <summary>
        /// Gets a value that indicates whether the asynchronous task has completed.
        /// </summary>
        public bool IsCompleted => this.ActorTask.IsCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorTaskAwaiter"/> struct.
        /// </summary>
        internal ActorTaskAwaiter(ActorTask task, Task awaiterTask)
        {
            this.ActorTask = task;
            this.Awaiter = awaiterTask.GetAwaiter();
        }

        /// <summary>
        /// Ends the wait for the completion of the asynchronous task.
        /// </summary>
        public void GetResult()
        {
            this.ActorTask.GetResult(this.Awaiter);
        }

        /// <summary>
        /// Sets the action to perform when the asynchronous task completes.
        /// </summary>
        public void OnCompleted(Action continuation)
        {
            this.ActorTask.OnCompleted(continuation, this.Awaiter);
        }

        /// <summary>
        /// Schedules the continuation action that is invoked when the asynchronous task completes.
        /// </summary>
        public void UnsafeOnCompleted(Action continuation)
        {
            this.ActorTask.UnsafeOnCompleted(continuation, this.Awaiter);
        }
    }

    /// <summary>
    /// Implements a <see cref="ActorTask"/> awaiter.
    /// </summary>
    /// <typeparam name="TResult">The type of the produced result.</typeparam>
    public readonly struct ActorTaskAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
    {
        // WARNING: The layout must remain the same, as the struct is used to access
        // the generic ActorTaskAwaiter<> as ActorTaskAwaiter.

        /// <summary>
        /// The actor task being awaited.
        /// </summary>
        private readonly ActorTask<TResult> ActorTask;

        /// <summary>
        /// The task awaiter.
        /// </summary>
        private readonly TaskAwaiter<TResult> Awaiter;

        /// <summary>
        /// Gets a value that indicates whether the asynchronous task has completed.
        /// </summary>
        public bool IsCompleted => this.ActorTask.IsCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorTaskAwaiter{TResult}"/> struct.
        /// </summary>
        internal ActorTaskAwaiter(ActorTask<TResult> task, Task<TResult> awaiterTask)
        {
            this.ActorTask = task;
            this.Awaiter = awaiterTask.GetAwaiter();
        }

        /// <summary>
        /// Ends the wait for the completion of the asynchronous task.
        /// </summary>
        public TResult GetResult()
        {
            return this.ActorTask.GetResult(this.Awaiter);
        }

        /// <summary>
        /// Sets the action to perform when the asynchronous task completes.
        /// </summary>
        public void OnCompleted(Action continuation)
        {
            this.ActorTask.OnCompleted(continuation, this.Awaiter);
        }

        /// <summary>
        /// Schedules the continuation action that is invoked when the asynchronous task completes.
        /// </summary>
        public void UnsafeOnCompleted(Action continuation)
        {
            this.ActorTask.UnsafeOnCompleted(continuation, this.Awaiter);
        }
    }
}
