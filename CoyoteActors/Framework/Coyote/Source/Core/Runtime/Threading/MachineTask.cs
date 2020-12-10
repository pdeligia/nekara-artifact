// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Coyote.Runtime;

namespace Microsoft.Coyote.Threading
{
    /// <summary>
    /// Provides the capability to execute work asynchronously on a <see cref="TaskScheduler"/>.
    /// During testing, a <see cref="ActorTask"/> executes in the scope of a <see cref="Actor"/>,
    /// which enables systematic exploration for finding bugs.
    /// </summary>
    [AsyncMethodBuilder(typeof(AsyncActorTaskMethodBuilder))]
    public class ActorTask : IDisposable
    {
        /// <summary>
        /// Name of the task used for logging purposes.
        /// </summary>
        internal const string Name = "ActorTask";

        /// <summary>
        /// A <see cref="ActorTask"/> that has completed successfully.
        /// </summary>
        public static ActorTask CompletedTask { get; } = new ActorTask(Task.CompletedTask);

        /// <summary>
        /// Returns the id of the currently executing <see cref="ActorTask"/>.
        /// </summary>
        public static int? CurrentId => ActorRuntime.CurrentScheduler.CurrentTaskId;

        /// <summary>
        /// Internal task used to execute the work.
        /// </summary>
        private protected readonly Task InternalTask;

        /// <summary>
        /// The id of this task.
        /// </summary>
        public int Id => this.InternalTask.Id;

        /// <summary>
        /// Task that provides access to the completed work.
        /// </summary>
        internal Task AwaiterTask => this.InternalTask;

        /// <summary>
        /// Value that indicates whether the task has completed.
        /// </summary>
        public bool IsCompleted => this.InternalTask.IsCompleted;

        /// <summary>
        /// Value that indicates whether the task completed execution due to being canceled.
        /// </summary>
        public bool IsCanceled => this.InternalTask.IsCanceled;

        /// <summary>
        /// Value that indicates whether the task completed due to an unhandled exception.
        /// </summary>
        public bool IsFaulted => this.InternalTask.IsFaulted;

        /// <summary>
        /// Gets the <see cref="System.AggregateException"/> that caused the task
        /// to end prematurely. If the task completed successfully or has not yet
        /// thrown any exceptions, this will return null.
        /// </summary>
        public AggregateException Exception => this.InternalTask.Exception;

        /// <summary>
        /// The status of this task.
        /// </summary>
        public TaskStatus Status => this.InternalTask.Status;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorTask"/> class.
        /// </summary>
        internal ActorTask(Task task)
        {
            this.InternalTask = task ?? throw new ArgumentNullException(nameof(task));
        }

        /// <summary>
        /// Creates a <see cref="ActorTask{TResult}"/> that is completed successfully with the specified result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="result">The result to store into the completed task.</param>
        /// <returns>The successfully completed task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult> FromResult<TResult>(TResult result) =>
            new ActorTask<TResult>(Task.FromResult(result));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that is completed due to
        /// cancellation with a specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
        /// <returns>The canceled task.</returns>
        public static ActorTask FromCanceled(CancellationToken cancellationToken) =>
            new ActorTask(Task.FromCanceled(cancellationToken));

        /// <summary>
        /// Creates a <see cref="ActorTask{TResult}"/> that is completed due to
        /// cancellation with a specified cancellation token.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
        /// <returns>The canceled task.</returns>
        public static ActorTask<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) =>
            new ActorTask<TResult>(Task.FromCanceled<TResult>(cancellationToken));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that is completed with a specified exception.
        /// </summary>
        /// <param name="exception">The exception with which to complete the task.</param>
        /// <returns>The faulted task.</returns>
        public static ActorTask FromException(Exception exception) =>
            new ActorTask(Task.FromException(exception));

        /// <summary>
        /// Creates a <see cref="ActorTask{TResult}"/> that is completed with a specified exception.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="exception">The exception with which to complete the task.</param>
        /// <returns>The faulted task.</returns>
        public static ActorTask<TResult> FromException<TResult>(Exception exception) =>
            new ActorTask<TResult>(Task.FromException<TResult>(exception));

        /// <summary>
        /// Executes the specified work asynchronously on <see cref="TaskScheduler.Default"/>
        /// and returns the scheduled <see cref="ActorTask"/>.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <returns>Task that represents the work to run asynchronously.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask Run(Action action) => ActorRuntime.CurrentScheduler.RunAsync(action, default);

        /// <summary>
        /// Executes the specified work asynchronously on <see cref="TaskScheduler.Default"/>
        /// and returns the scheduled <see cref="ActorTask"/>.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to run asynchronously.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask Run(Action action, CancellationToken cancellationToken) =>
            ActorRuntime.CurrentScheduler.RunAsync(action, cancellationToken);

        /// <summary>
        /// Executes the specified work asynchronously on <see cref="TaskScheduler.Default"/>
        /// and returns the scheduled <see cref="ActorTask"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <returns>Task that represents the work to run asynchronously.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask Run(Func<Task> function) => ActorRuntime.CurrentScheduler.RunAsync(function, default);

        /// <summary>
        /// Executes the specified work asynchronously on <see cref="TaskScheduler.Default"/>
        /// and returns the scheduled <see cref="ActorTask"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to run asynchronously.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask Run(Func<Task> function, CancellationToken cancellationToken) =>
            ActorRuntime.CurrentScheduler.RunAsync(function, cancellationToken);

        /// <summary>
        /// Executes the specified work asynchronously on <see cref="TaskScheduler.Default"/>
        /// and returns the scheduled <see cref="ActorTask"/>.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <returns>Task that represents the work to run asynchronously.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult> Run<TResult>(Func<TResult> function) =>
            ActorRuntime.CurrentScheduler.RunAsync(function, default);

        /// <summary>
        /// Executes the specified work asynchronously on <see cref="TaskScheduler.Default"/>
        /// and returns the scheduled <see cref="ActorTask"/>.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to run asynchronously.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken) =>
            ActorRuntime.CurrentScheduler.RunAsync(function, cancellationToken);

        /// <summary>
        /// Executes the specified work asynchronously on <see cref="TaskScheduler.Default"/>
        /// and returns the scheduled <see cref="ActorTask"/>.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <returns>Task that represents the work to run asynchronously.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult> Run<TResult>(Func<Task<TResult>> function) =>
            ActorRuntime.CurrentScheduler.RunAsync(function, default);

        /// <summary>
        /// Executes the specified work asynchronously on <see cref="TaskScheduler.Default"/>
        /// and returns the scheduled <see cref="ActorTask"/>.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to run asynchronously.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken) =>
            ActorRuntime.CurrentScheduler.RunAsync(function, cancellationToken);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that completes after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">
        /// The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.
        /// </param>
        /// <returns>Task that represents the time delay.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask Delay(int millisecondsDelay) =>
            ActorRuntime.CurrentScheduler.DelayAsync(millisecondsDelay, default);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that completes after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">
        /// The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.
        /// </param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the delay.</param>
        /// <returns>Task that represents the time delay.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask Delay(int millisecondsDelay, CancellationToken cancellationToken) =>
            ActorRuntime.CurrentScheduler.DelayAsync(millisecondsDelay, cancellationToken);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask WhenAll(params ActorTask[] tasks) =>
            ActorRuntime.CurrentScheduler.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask WhenAll(params Task[] tasks) =>
            ActorRuntime.CurrentScheduler.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask WhenAll(IEnumerable<ActorTask> tasks) =>
            ActorRuntime.CurrentScheduler.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask WhenAll(IEnumerable<Task> tasks) =>
            ActorRuntime.CurrentScheduler.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult[]> WhenAll<TResult>(params ActorTask<TResult>[] tasks) =>
            ActorRuntime.CurrentScheduler.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks) =>
            ActorRuntime.CurrentScheduler.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult[]> WhenAll<TResult>(IEnumerable<ActorTask<TResult>> tasks) =>
            ActorRuntime.CurrentScheduler.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks) =>
            ActorRuntime.CurrentScheduler.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<Task> WhenAny(params ActorTask[] tasks) =>
            ActorRuntime.CurrentScheduler.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<Task> WhenAny(params Task[] tasks) =>
            ActorRuntime.CurrentScheduler.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<Task> WhenAny(IEnumerable<ActorTask> tasks) =>
            ActorRuntime.CurrentScheduler.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<Task> WhenAny(IEnumerable<Task> tasks) =>
            ActorRuntime.CurrentScheduler.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<Task<TResult>> WhenAny<TResult>(params ActorTask<TResult>[] tasks) =>
            ActorRuntime.CurrentScheduler.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks) =>
            ActorRuntime.CurrentScheduler.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<Task<TResult>> WhenAny<TResult>(IEnumerable<ActorTask<TResult>> tasks) =>
            ActorRuntime.CurrentScheduler.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActorTask<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks) =>
            ActorRuntime.CurrentScheduler.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates an awaitable that asynchronously yields back to the current context when awaited.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YieldAwaitable Yield() => new YieldAwaitable(ActorRuntime.CurrentScheduler);

        /// <summary>
        /// Converts the specified <see cref="ActorTask"/> into a <see cref="Task"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ToTask() => this.InternalTask;

        /// <summary>
        /// Gets an awaiter for this awaitable.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ActorTaskAwaiter GetAwaiter() => new ActorTaskAwaiter(this, this.InternalTask);

        /// <summary>
        /// Ends the wait for the completion of the task.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void GetResult(TaskAwaiter awaiter) => awaiter.GetResult();

        /// <summary>
        /// Sets the action to perform when the task completes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnCompleted(Action continuation, TaskAwaiter awaiter) => awaiter.OnCompleted(continuation);

        /// <summary>
        /// Schedules the continuation action that is invoked when the task completes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void UnsafeOnCompleted(Action continuation, TaskAwaiter awaiter) =>
            awaiter.UnsafeOnCompleted(continuation);

        /// <summary>
        /// Disposes the <see cref="ActorTask"/>, releasing all of its unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Disposes the <see cref="ActorTask"/>, releasing all of its unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Unlike most of the members of <see cref="ActorTask"/>, this method is not thread-safe.
        /// </remarks>
        public void Dispose()
        {
            this.InternalTask.Dispose();
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Provides the capability to execute work asynchronously on a <see cref="TaskScheduler"/> and produce
    /// a result at some time in the future. During testing, a <see cref="ActorTask"/> executes in the
    /// scope of a <see cref="Actor"/>, which enables systematic exploration for finding bugs.
    /// </summary>
    /// <typeparam name="TResult">The type of the produced result.</typeparam>
    [AsyncMethodBuilder(typeof(AsyncActorTaskMethodBuilder<>))]
    public class ActorTask<TResult> : ActorTask
    {
        /// <summary>
        /// Task that provides access to the completed work.
        /// </summary>
        internal new Task<TResult> AwaiterTask => this.InternalTask as Task<TResult>;

        /// <summary>
        /// Gets the result value of this task.
        /// </summary>
        public TResult Result => this.AwaiterTask.Result;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorTask{TResult}"/> class.
        /// </summary>
        internal ActorTask(Task<TResult> task)
            : base(task)
        {
        }

        /// <summary>
        /// Gets an awaiter for this awaitable.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new virtual ActorTaskAwaiter<TResult> GetAwaiter() =>
            new ActorTaskAwaiter<TResult>(this, this.InternalTask as Task<TResult>);

        /// <summary>
        /// Ends the wait for the completion of the task.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual TResult GetResult(TaskAwaiter<TResult> awaiter) => awaiter.GetResult();

        /// <summary>
        /// Sets the action to perform when the task completes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnCompleted(Action continuation, TaskAwaiter<TResult> awaiter) =>
            awaiter.OnCompleted(continuation);

        /// <summary>
        /// Schedules the continuation action that is invoked when the task completes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void UnsafeOnCompleted(Action continuation, TaskAwaiter<TResult> awaiter) =>
            awaiter.UnsafeOnCompleted(continuation);
    }
}
