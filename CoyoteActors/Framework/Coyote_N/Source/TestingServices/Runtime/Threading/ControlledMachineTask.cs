// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Coyote.Runtime;
using Microsoft.Coyote.TestingServices.Runtime;
using Microsoft.Coyote.TestingServices.Scheduling;
using Microsoft.Coyote.Threading;

namespace Microsoft.Coyote.TestingServices.Threading
{
    /// <summary>
    /// A <see cref="ActorTask"/> that is controlled by the runtime scheduler.
    /// </summary>
    internal sealed class ControlledActorTask : ActorTask
    {
        /// <summary>
        /// The testing runtime executing this task.
        /// </summary>
        private readonly SystematicTestingRuntime Runtime;

        /// <summary>
        /// The type of the task.
        /// </summary>
        private readonly ActorTaskType Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlledActorTask"/> class.
        /// </summary>
        internal ControlledActorTask(SystematicTestingRuntime runtime, Task task, ActorTaskType taskType)
            : base(task)
        {
            IO.Debug.WriteLine("<ActorTask> Creating task '{0}' from task '{1}' (option: {2}).",
                task.Id, Task.CurrentId, taskType);
            this.Runtime = runtime;
            this.Type = taskType;
        }

        /// <summary>
        /// Gets an awaiter for this awaitable.
        /// </summary>
        public override ActorTaskAwaiter GetAwaiter()
        {
            IO.Debug.WriteLine("<ActorTask> Awaiting task '{0}' from task '{1}'.", this.AwaiterTask.Id, Task.CurrentId);
            return new ActorTaskAwaiter(this, this.AwaiterTask);
        }

        /// <summary>
        /// Ends the wait for the completion of the task.
        /// </summary>
        internal override void GetResult(TaskAwaiter awaiter)
        {
            AsyncActor caller = this.Runtime.GetExecutingActor<AsyncActor>();
            IO.Debug.WriteLine("<ActorTask> Actor '{0}' is waiting task '{1}' to complete from task '{2}'.",
                caller.Id, this.Id, Task.CurrentId);
            ActorOperation callerOp = this.Runtime.GetAsynchronousOperation(caller.Id.Value);
            callerOp.OnWaitTask(this.AwaiterTask);
            this.Runtime.Scheduler.ScheduleNextOperation(AsyncOperationType.Join, AsyncOperationTarget.Task, caller.Id.Value);
            awaiter.GetResult();
        }

        /// <summary>
        /// Sets the action to perform when the task completes.
        /// </summary>
        internal override void OnCompleted(Action continuation, TaskAwaiter awaiter) =>
            this.DispatchWork(continuation);

        /// <summary>
        /// Schedules the continuation action that is invoked when the task completes.
        /// </summary>
        internal override void UnsafeOnCompleted(Action continuation, TaskAwaiter awaiter) =>
            this.DispatchWork(continuation);

        /// <summary>
        /// Dispatches the work.
        /// </summary>
        private void DispatchWork(Action continuation)
        {
            try
            {
                AsyncActor caller = this.Runtime.GetExecutingActor<AsyncActor>();
                this.Runtime.Assert(caller != null,
                    "Task with id '{0}' that is not controlled by the Coyote runtime is executing actor task '{1}'.",
                    Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString() : "<unknown>", this.Id);

                if (caller is Actor actor)
                {
                    this.Runtime.Assert((actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler,
                        "Actor '{0}' is executing actor task '{1}' inside a handler that does not return a 'ActorTask'.",
                        caller.Id, this.Id);
                }

                if (this.Type is ActorTaskType.CompletionSourceTask)
                {
                    IO.Debug.WriteLine("<ActorTask> Actor '{0}' is executing continuation of task '{1}' on task '{2}'.",
                        caller.Id, this.Id, Task.CurrentId);
                    continuation();
                    IO.Debug.WriteLine("<ActorTask> Actor '{0}' resumed after continuation of task '{1}' on task '{2}'.",
                        caller.Id, this.Id, Task.CurrentId);
                }
                else if (this.Type is ActorTaskType.ExplicitTask)
                {
                    IO.Debug.WriteLine("<ActorTask> Actor '{0}' is dispatching continuation of task '{1}'.", caller.Id, this.Id);
                    this.Runtime.DispatchWork(new ActionWorkActor(this.Runtime, continuation), this.AwaiterTask);
                    IO.Debug.WriteLine("<ActorTask> Actor '{0}' dispatched continuation of task '{1}'.", caller.Id, this.Id);
                }
            }
            catch (ExecutionCanceledException)
            {
                IO.Debug.WriteLine($"<Exception> ExecutionCanceledException was thrown from task '{Task.CurrentId}'.");
            }
        }
    }

    /// <summary>
    /// A <see cref="ActorTask{TResult}"/> that is controlled by the runtime scheduler.
    /// </summary>
    internal sealed class ControlledActorTask<TResult> : ActorTask<TResult>
    {
        /// <summary>
        /// The testing runtime executing this task.
        /// </summary>
        private readonly SystematicTestingRuntime Runtime;

        /// <summary>
        /// The type of the task.
        /// </summary>
        private readonly ActorTaskType Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlledActorTask{TResult}"/> class.
        /// </summary>
        internal ControlledActorTask(SystematicTestingRuntime runtime, Task<TResult> task, ActorTaskType taskType)
            : base(task)
        {
            IO.Debug.WriteLine("<ActorTask> Creating task '{0}' with result type '{1}' from task '{2}' (option: {3}).",
                task.Id, typeof(TResult), Task.CurrentId, taskType);
            this.Runtime = runtime;
            this.Type = taskType;
        }

        /// <summary>
        /// Gets an awaiter for this awaitable.
        /// </summary>
        public override ActorTaskAwaiter<TResult> GetAwaiter()
        {
            IO.Debug.WriteLine("<ActorTask> Awaiting task '{0}' with result type '{1}' and type '{2}' from task '{3}'.",
                this.AwaiterTask.Id, typeof(TResult), this.Type, Task.CurrentId);
            return new ActorTaskAwaiter<TResult>(this, this.AwaiterTask);
        }

        /// <summary>
        /// Ends the wait for the completion of the task.
        /// </summary>
        internal override void GetResult(TaskAwaiter awaiter)
        {
            AsyncActor caller = this.Runtime.GetExecutingActor<AsyncActor>();
            IO.Debug.WriteLine("<ActorTask> Actor '{0}' is waiting task '{1}' with result type '{2}' to complete from task '{3}'.",
                caller.Id, this.Id, typeof(TResult), Task.CurrentId);
            ActorOperation callerOp = this.Runtime.GetAsynchronousOperation(caller.Id.Value);
            callerOp.OnWaitTask(this.AwaiterTask);
            this.Runtime.Scheduler.ScheduleNextOperation(AsyncOperationType.Join, AsyncOperationTarget.Task, caller.Id.Value);
            awaiter.GetResult();
        }

        /// <summary>
        /// Sets the action to perform when the task completes.
        /// </summary>
        internal override void OnCompleted(Action continuation, TaskAwaiter awaiter) =>
            this.DispatchWork(continuation);

        /// <summary>
        /// Schedules the continuation action that is invoked when the task completes.
        /// </summary>
        internal override void UnsafeOnCompleted(Action continuation, TaskAwaiter awaiter) =>
            this.DispatchWork(continuation);

        /// <summary>
        /// Dispatches the work.
        /// </summary>
        private void DispatchWork(Action continuation)
        {
            try
            {
                AsyncActor caller = this.Runtime.GetExecutingActor<AsyncActor>();
                this.Runtime.Assert(caller != null,
                    "Task with id '{0}' that is not controlled by the Coyote runtime is executing actor task '{1}'.",
                    Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString() : "<unknown>", this.Id);

                if (caller is Actor actor)
                {
                    this.Runtime.Assert((actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler,
                        "Actor '{0}' is executing actor task '{1}' inside a handler that does not return a 'ActorTask'.",
                        caller.Id, this.Id);
                }

                if (this.Type is ActorTaskType.CompletionSourceTask)
                {
                    IO.Debug.WriteLine("<ActorTask> Actor '{0}' is executing continuation of task '{1}' with result type '{2}' on task '{3}'.",
                        caller.Id, this.Id, typeof(TResult), Task.CurrentId);
                    continuation();
                    IO.Debug.WriteLine("<ActorTask> Actor '{0}' resumed after continuation of task '{1}' with result type '{2}' on task '{3}'.",
                        caller.Id, this.Id, typeof(TResult), Task.CurrentId);
                }
                else if (this.Type is ActorTaskType.ExplicitTask)
                {
                    IO.Debug.WriteLine("<ActorTask> Actor '{0}' is dispatching continuation of task '{1}' with result type '{2}'.",
                        caller.Id, this.Id, typeof(TResult));
                    this.Runtime.DispatchWork(new ActionWorkActor(this.Runtime, continuation), this.AwaiterTask);
                    IO.Debug.WriteLine("<ActorTask> Actor '{0}' dispatched continuation of task '{1}' with result type '{2}'.",
                        caller.Id, this.Id, typeof(TResult));
                }
            }
            catch (ExecutionCanceledException)
            {
                IO.Debug.WriteLine($"<Exception> ExecutionCanceledException was thrown from task '{Task.CurrentId}'.");
            }
        }
    }
}
