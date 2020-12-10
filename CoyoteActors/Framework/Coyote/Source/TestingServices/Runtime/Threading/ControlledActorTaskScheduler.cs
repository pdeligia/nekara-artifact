// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Coyote.Runtime;
using Microsoft.Coyote.TestingServices.Runtime;
using Microsoft.Coyote.TestingServices.Scheduling;
using Microsoft.Coyote.Threading;

using static System.Runtime.CompilerServices.YieldAwaitable;

namespace Microsoft.Coyote.TestingServices.Threading
{
    /// <summary>
    /// Implements a <see cref="ActorTaskScheduler"/> that is controlled by the runtime during testing.
    /// </summary>
    internal sealed class ControlledActorTaskScheduler : ActorTaskScheduler
    {
        /// <summary>
        /// The testing runtime that is controlling this scheduler.
        /// </summary>
        internal SystematicTestingRuntime Runtime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlledActorTaskScheduler"/> class.
        /// </summary>
        internal ControlledActorTaskScheduler(SystematicTestingRuntime runtime)
            : base()
        {
            this.Runtime = runtime;
        }

        /// <summary>
        /// Schedules the specified work to execute asynchronously and returns a task handle for the work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to be scheduled.</returns>
        internal override ActorTask RunAsync(Action action, CancellationToken cancellationToken) =>
            this.Runtime.CreateActorTask(action, cancellationToken);

        /// <summary>
        /// Schedules the specified work to execute asynchronously and returns a task handle for the work.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to be scheduled.</returns>
        internal override ActorTask RunAsync(Func<Task> function, CancellationToken cancellationToken) =>
            this.Runtime.CreateActorTask(function, cancellationToken);

        /// <summary>
        /// Schedules the specified work to execute asynchronously and returns a task handle for the work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to be scheduled.</returns>
        internal override ActorTask<TResult> RunAsync<TResult>(Func<TResult> function, CancellationToken cancellationToken) =>
            this.Runtime.CreateActorTask(function, cancellationToken);

        /// <summary>
        /// Schedules the specified work to execute asynchronously and returns a task handle for the work.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the work.</param>
        /// <returns>Task that represents the work to be scheduled.</returns>
        internal override ActorTask<TResult> RunAsync<TResult>(Func<Task<TResult>> function,
            CancellationToken cancellationToken) =>
            this.Runtime.CreateActorTask(function, cancellationToken);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that completes after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">
        /// The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.
        /// </param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the delay.</param>
        /// <returns>Task that represents the time delay.</returns>
        internal override ActorTask DelayAsync(int millisecondsDelay, CancellationToken cancellationToken) =>
            this.Runtime.CreateActorTask(millisecondsDelay, cancellationToken);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        internal override ActorTask WaitAllTasksAsync(params Task[] tasks) =>
            this.Runtime.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        internal override ActorTask WaitAllTasksAsync(IEnumerable<Task> tasks) =>
            this.Runtime.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified array have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        internal override ActorTask<TResult[]> WaitAllTasksAsync<TResult>(params Task<TResult>[] tasks) =>
            this.Runtime.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when all tasks
        /// in the specified enumerable collection have completed.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="tasks">The tasks to wait for completion.</param>
        /// <returns>Task that represents the completion of all of the specified tasks.</returns>
        internal override ActorTask<TResult[]> WaitAllTasksAsync<TResult>(IEnumerable<Task<TResult>> tasks) =>
            this.Runtime.WaitAllTasksAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        internal override ActorTask<Task> WaitAnyTaskAsync(params ActorTask[] tasks) =>
            this.Runtime.WaitAnyTaskAsync(tasks.Select(t => t.AwaiterTask));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        internal override ActorTask<Task> WaitAnyTaskAsync(params Task[] tasks) =>
            this.Runtime.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        internal override ActorTask<Task> WaitAnyTaskAsync(IEnumerable<ActorTask> tasks) =>
            this.Runtime.WaitAnyTaskAsync(tasks.Select(t => t.AwaiterTask));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        internal override ActorTask<Task> WaitAnyTaskAsync(IEnumerable<Task> tasks) =>
            this.Runtime.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        internal override ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(params ActorTask<TResult>[] tasks) =>
            this.Runtime.WaitAnyTaskAsync(tasks.Select(t => t.AwaiterTask));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified array have completed.
        /// </summary>
        internal override ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(params Task<TResult>[] tasks) =>
            this.Runtime.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        internal override ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(IEnumerable<ActorTask<TResult>> tasks) =>
            this.Runtime.WaitAnyTaskAsync(tasks.Select(t => t.AwaiterTask));

        /// <summary>
        /// Creates a <see cref="ActorTask"/> that will complete when any task
        /// in the specified enumerable collection have completed.
        /// </summary>
        internal override ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(IEnumerable<Task<TResult>> tasks) =>
            this.Runtime.WaitAnyTaskAsync(tasks);

        /// <summary>
        /// Creates a <see cref="ActorTask"/> associated with a completion source.
        /// </summary>
        internal override ActorTask CreateCompletionSourceActorTask(Task task) =>
            this.Runtime.CreateCompletionActorTask(task);

        /// <summary>
        /// Creates a <see cref="ActorTask{TResult}"/> associated with a completion source.
        /// </summary>
        internal override ActorTask<TResult> CreateCompletionSourceActorTask<TResult>(Task<TResult> task) =>
            this.Runtime.CreateCompletionActorTask(task);

        /// <summary>
        /// Creates a mutual exclusion lock that is compatible with <see cref="ActorTask"/> objects.
        /// </summary>
        internal override ActorLock CreateLock()
        {
            var id = (ulong)Interlocked.Increment(ref this.ActorLockIdCounter) - 1;
            return new ControlledActorLock(this.Runtime, id);
        }

        /// <summary>
        /// Ends the wait for the completion of the yield operation.
        /// </summary>
        internal override void GetYieldResult(YieldAwaiter awaiter)
        {
            AsyncActor caller = this.Runtime.GetExecutingActor<AsyncActor>();
            this.Runtime.Scheduler.ScheduleNextOperation(AsyncOperationType.Yield, AsyncOperationTarget.Task, caller.Id.Value);
            awaiter.GetResult();
        }

        /// <summary>
        /// Sets the action to perform when the yield operation completes.
        /// </summary>
        internal override void OnYieldCompleted(Action continuation, YieldAwaiter awaiter) =>
            this.DispatchYield(continuation);

        /// <summary>
        /// Schedules the continuation action that is invoked when the yield operation completes.
        /// </summary>
        internal override void UnsafeOnYieldCompleted(Action continuation, YieldAwaiter awaiter) =>
            this.DispatchYield(continuation);

        /// <summary>
        /// Dispatches the work.
        /// </summary>
        private void DispatchYield(Action continuation)
        {
            try
            {
                AsyncActor caller = this.Runtime.GetExecutingActor<AsyncActor>();
                this.Runtime.Assert(caller != null,
                    "Task with id '{0}' that is not controlled by the Coyote runtime invoked yield operation.",
                    Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString() : "<unknown>");

                if (caller is Actor actor)
                {
                    this.Runtime.Assert((actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler,
                        "Actor '{0}' is executing a yield operation inside a handler that does not return a 'ActorTask'.", caller.Id);
                }

                IO.Debug.WriteLine("<ActorTask> Actor '{0}' is executing a yield operation.", caller.Id);
                this.Runtime.DispatchWork(new ActionWorkActor(this.Runtime, continuation), null);
                IO.Debug.WriteLine("<ActorTask> Actor '{0}' is executing a yield operation.", caller.Id);
            }
            catch (ExecutionCanceledException)
            {
                IO.Debug.WriteLine($"<Exception> ExecutionCanceledException was thrown from task '{Task.CurrentId}'.");
            }
        }
    }
}
