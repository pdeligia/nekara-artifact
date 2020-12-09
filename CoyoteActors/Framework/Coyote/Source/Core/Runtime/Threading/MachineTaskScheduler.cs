// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using static System.Runtime.CompilerServices.YieldAwaitable;

namespace Microsoft.CoyoteActors.Threading
{
    /// <summary>
    /// Schedules the execution of actor tasks on a <see cref="TaskScheduler"/>.
    /// </summary>
    internal class ActorTaskScheduler
    {
        /// <summary>
        /// The default <see cref="ActorTask"/> scheduler. It schedules the execution
        /// of tasks on <see cref="TaskScheduler.Default"/>.
        /// </summary>
        internal static ActorTaskScheduler Default { get; } = new ActorTaskScheduler();

        /// <summary>
        /// Map from task ids to <see cref="ActorTask"/> objects.
        /// </summary>
        protected readonly ConcurrentDictionary<int, ActorTask> TaskMap;

        /// <summary>
        /// Returns the id of the currently executing <see cref="ActorTask"/>.
        /// </summary>
        internal virtual int? CurrentTaskId => Task.CurrentId;

        /// <summary>
        /// Monotonically increasing actor lock id counter.
        /// </summary>
        internal long ActorLockIdCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorTaskScheduler"/> class.
        /// </summary>
        internal ActorTaskScheduler()
        {
            this.TaskMap = new ConcurrentDictionary<int, ActorTask>();
            this.ActorLockIdCounter = 0;
        }

        /// <summary>
        /// Schedules the specified work to execute asynchronously and returns a task handle for the work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to be scheduled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask RunAsync(Action action, CancellationToken cancellationToken) =>
            new ActorTask(Task.Run(action, cancellationToken));

        /// <summary>
        /// Schedules the specified work to execute asynchronously and returns a task handle for the work.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to be scheduled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask RunAsync(Func<Task> function, CancellationToken cancellationToken) =>
            new ActorTask(Task.Run(function, cancellationToken));

        /// <summary>
        /// Schedules the specified work to execute asynchronously and returns a task handle for the work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to be scheduled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<TResult> RunAsync<TResult>(Func<TResult> function, CancellationToken cancellationToken) =>
            new ActorTask<TResult>(Task.Run(function, cancellationToken));

        /// <summary>
        /// Schedules the specified work to execute asynchronously and returns a task handle for the work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to be scheduled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<TResult> RunAsync<TResult>(Func<Task<TResult>> function,
            CancellationToken cancellationToken) =>
            new ActorTask<TResult>(Task.Run(function, cancellationToken));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that completes after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">
        /// The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.
        /// </param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the delay.</param>
        /// <returns>Task that represents the time delay.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask DelayAsync(int millisecondsDelay, CancellationToken cancellationToken) =>
            new ActorTask(Task.Delay(millisecondsDelay, cancellationToken));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask WaitAllTasksAsync(params ActorTask[] tasks) =>
            new ActorTask(Task.WhenAll(tasks.Select(t => t.AwaiterTask)));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask WaitAllTasksAsync(params Task[] tasks) =>
            new ActorTask(Task.WhenAll(tasks));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask WaitAllTasksAsync(IEnumerable<ActorTask> tasks) =>
            new ActorTask(Task.WhenAll(tasks.Select(t => t.AwaiterTask)));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask WaitAllTasksAsync(IEnumerable<Task> tasks) =>
            new ActorTask(Task.WhenAll(tasks));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<TResult[]> WaitAllTasksAsync<TResult>(params ActorTask<TResult>[] tasks) =>
            new ActorTask<TResult[]>(Task.WhenAll(tasks.Select(t => t.AwaiterTask)));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<TResult[]> WaitAllTasksAsync<TResult>(params Task<TResult>[] tasks) =>
            new ActorTask<TResult[]>(Task.WhenAll(tasks));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<TResult[]> WaitAllTasksAsync<TResult>(IEnumerable<ActorTask<TResult>> tasks) =>
            new ActorTask<TResult[]>(Task.WhenAll(tasks.Select(t => t.AwaiterTask)));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<TResult[]> WaitAllTasksAsync<TResult>(IEnumerable<Task<TResult>> tasks) =>
            new ActorTask<TResult[]>(Task.WhenAll(tasks));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<Task> WaitAnyTaskAsync(params ActorTask[] tasks) =>
            new ActorTask<Task>(Task.WhenAny(tasks.Select(t => t.AwaiterTask)));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<Task> WaitAnyTaskAsync(params Task[] tasks) =>
            new ActorTask<Task>(Task.WhenAny(tasks));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<Task> WaitAnyTaskAsync(IEnumerable<ActorTask> tasks) =>
            new ActorTask<Task>(Task.WhenAny(tasks.Select(t => t.AwaiterTask)));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<Task> WaitAnyTaskAsync(IEnumerable<Task> tasks) =>
            new ActorTask<Task>(Task.WhenAny(tasks));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(params ActorTask<TResult>[] tasks) =>
            new ActorTask<Task<TResult>>(Task.WhenAny(tasks.Select(t => t.AwaiterTask)));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(params Task<TResult>[] tasks) =>
            new ActorTask<Task<TResult>>(Task.WhenAny(tasks));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(IEnumerable<ActorTask<TResult>> tasks) =>
            new ActorTask<Task<TResult>>(Task.WhenAny(tasks.Select(t => t.AwaiterTask)));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(IEnumerable<Task<TResult>> tasks) =>
            new ActorTask<Task<TResult>>(Task.WhenAny(tasks));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> associated with a completion source.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask CreateCompletionSourceActorTask(Task task) => new ActorTask(task);

        /// <summary>
        /// Creates a <see cref="ActorTask{TResult}"/> associated with a completion source.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorTask<TResult> CreateCompletionSourceActorTask<TResult>(Task<TResult> task) =>
            new ActorTask<TResult>(task);

        /// <summary>
        /// Creates a mutual exclusion lock that is compatible with <see cref="ActorTask"/> objects.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ActorLock CreateLock()
        {
            var id = (ulong)Interlocked.Increment(ref this.ActorLockIdCounter) - 1;
            return new ActorLock(id);
        }

        /// <summary>
        /// Creates an awaiter that asynchronously yields back to the current context when awaited.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal YieldAwaitable.YieldAwaiter CreateYieldAwaiter() => new YieldAwaitable.YieldAwaiter(this);

        /// <summary>
        /// Ends the wait for the completion of the yield operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void GetYieldResult(YieldAwaiter awaiter) => awaiter.GetResult();

        /// <summary>
        /// Sets the action to perform when the yield operation completes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnYieldCompleted(Action continuation, YieldAwaiter awaiter) => awaiter.OnCompleted(continuation);

        /// <summary>
        /// Schedules the continuation action that is invoked when the yield operation completes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void UnsafeOnYieldCompleted(Action continuation, YieldAwaiter awaiter) =>
            awaiter.UnsafeOnCompleted(continuation);
    }
}
