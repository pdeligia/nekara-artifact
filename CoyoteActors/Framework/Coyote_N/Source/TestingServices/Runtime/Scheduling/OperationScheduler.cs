// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.CoyoteActors.IO;
using Microsoft.CoyoteActors.Runtime;
using Microsoft.CoyoteActors.TestingServices.Runtime;
using Microsoft.CoyoteActors.TestingServices.Scheduling.Strategies;
using Microsoft.CoyoteActors.TestingServices.Tracing.Schedule;
using Microsoft.CoyoteActors.Utilities;

#pragma warning disable CA1822
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable SA1005 // Single line comments must begin with single space
namespace Microsoft.CoyoteActors.TestingServices.Scheduling
{
    /// <summary>
    /// Provides methods for controlling the schedule of asynchronous operations.
    /// </summary>
    internal sealed class OperationScheduler : IDisposable
    {
        /// <summary>
        /// The native scheduler.
        /// </summary>
        private static IntPtr SchedulerPtr;

        /// <summary>
        /// The configuration used by the scheduler.
        /// </summary>
        internal readonly Configuration Configuration;

        /// <summary>
        /// The testing runtime.
        /// </summary>
        private readonly SystematicTestingRuntime Runtime;

        /// <summary>
        /// The scheduling strategy to be used for state-space exploration.
        /// </summary>
        private readonly ISchedulingStrategy Strategy;

        /// <summary>
        /// Map from unique ids to asynchronous operations.
        /// </summary>
        private readonly Dictionary<ulong, ActorOperation> OperationMap;

        /// <summary>
        /// Map from ids of tasks that are controlled by the runtime to operations.
        /// </summary>
        internal readonly ConcurrentDictionary<int, ActorOperation> ControlledTaskMap;

        /// <summary>
        /// The program schedule trace.
        /// </summary>
        internal ScheduleTrace ScheduleTrace;

        //private readonly ScheduleTrace Mirror;

        /// <summary>
        /// Checks if the scheduler is running.
        /// </summary>
        private bool IsSchedulerRunning;

        /// <summary>
        /// The currently scheduled asynchronous operation.
        /// </summary>
        internal ActorOperation ScheduledOperation { get; private set; }

        /// <summary>
        /// The set of hashed states.
        /// </summary>
        private static readonly HashSet<int> HashedStates = new HashSet<int>();

        /// <summary>
        /// Number of scheduled steps.
        /// </summary>
        internal int ScheduledSteps { get; private set; }

        /// <summary>
        /// Checks if the schedule has been fully explored.
        /// </summary>
        internal bool HasFullyExploredSchedule { get; private set; }

        /// <summary>
        /// True if a bug was found.
        /// </summary>
        internal bool BugFound { get; private set; }

        /// <summary>
        /// Bug report.
        /// </summary>
        internal string BugReport { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationScheduler"/> class.
        /// </summary>
        internal OperationScheduler(SystematicTestingRuntime runtime, ISchedulingStrategy strategy,
            ScheduleTrace trace, Configuration configuration)
        {
            this.Configuration = configuration;
            this.Runtime = runtime;

            ulong seed = (ulong)this.Configuration.SchedulingSeed;
            if (SchedulerPtr == IntPtr.Zero)
            {
                if (this.Configuration.SchedulingStrategy == SchedulingStrategy.PCT)
                {
                    Console.WriteLine($"PrioritySwitchBound: {this.Configuration.PrioritySwitchBound}");
                    SchedulerPtr = create_scheduler_with_pct_strategy(seed, (ulong)this.Configuration.PrioritySwitchBound);
                }
                else
                {
                    SchedulerPtr = create_scheduler_with_random_strategy(seed);
                }
            }

            this.Strategy = strategy;
            this.OperationMap = new Dictionary<ulong, ActorOperation>();
            this.ControlledTaskMap = new ConcurrentDictionary<int, ActorOperation>();
            this.ScheduleTrace = trace;
            this.IsSchedulerRunning = true;
            this.BugFound = false;
            this.HasFullyExploredSchedule = false;
            this.ScheduledSteps = 0;

            //string[] scheduleDump = System.IO.File.ReadAllLines(@".\bin\netcoreapp3.1\Output\Benchmarks.Protocols.dll\CoyoteTesterOutput\Benchmarks.Protocols_0_2.schedule");
            //this.Mirror = new ScheduleTrace(scheduleDump);
        }

        [DllImport("libcoyote.so")]
        private static extern IntPtr create_scheduler();

        [DllImport("libcoyote.so")]
        private static extern IntPtr create_scheduler_with_random_strategy(ulong seed);

        [DllImport("libcoyote.so")]
        private static extern IntPtr create_scheduler_with_pct_strategy(ulong seed, ulong bound);

        [DllImport("libcoyote.so")]
        private static extern int attach(IntPtr scheduler);

        [DllImport("libcoyote.so")]
        private static extern int detach(IntPtr scheduler);

        [DllImport("libcoyote.so")]
        private static extern int create_operation(IntPtr scheduler, ulong operation_id);

        [DllImport("libcoyote.so")]
        private static extern int start_operation(IntPtr scheduler, ulong operation_id);

        [DllImport("libcoyote.so")]
        private static extern int join_operation(IntPtr scheduler, ulong operation_id);

        [DllImport("libcoyote.so")]
        private static extern int complete_operation(IntPtr scheduler, ulong operation_id);

        [DllImport("libcoyote.so")]
        private static extern int create_resource(IntPtr scheduler, ulong resource_id);

        [DllImport("libcoyote.so")]
        private static extern int wait_resource(IntPtr scheduler, ulong resource_id);

        [DllImport("libcoyote.so")]
        private static extern int signal_resource(IntPtr scheduler, ulong resource_id);

        [DllImport("libcoyote.so")]
        private static extern int delete_resource(IntPtr scheduler, ulong resource_id);

        [DllImport("libcoyote.so")]
        private static extern int schedule_next(IntPtr scheduler);

        [DllImport("libcoyote.so")]
        private static extern int next_boolean(IntPtr scheduler);

        [DllImport("libcoyote.so")]
        private static extern int next_integer(IntPtr scheduler);

        [DllImport("libcoyote.so")]
        private static extern int next_integer(IntPtr scheduler, ulong max_value);

        [DllImport("libcoyote.so")]
        private static extern int scheduled_operation_id(IntPtr scheduler);

        [DllImport("libcoyote.so")]
        private static extern int random_seed(IntPtr scheduler);

        [DllImport("libcoyote.so")]
        private static extern int dispose_scheduler(IntPtr scheduler);

        internal int CreateOperation(ActorOperation op) => create_operation(SchedulerPtr, op.Actor.Id.Value);

        internal int StartOperation(ActorOperation op) => start_operation(SchedulerPtr, op.Actor.Id.Value);

        internal int JoinOperation(ActorOperation op) => join_operation(SchedulerPtr, op.Actor.Id.Value);

        internal int CompleteOperation(ActorOperation op) => complete_operation(SchedulerPtr, op.Actor.Id.Value);

        internal int CreateResource(ulong id) => create_operation(SchedulerPtr, id);

        internal int AcquireResource(ulong id) => wait_resource(SchedulerPtr, id);

        internal int ReleaseResource(ulong id) => signal_resource(SchedulerPtr, id);

        internal int ScheduleNextOperation() => schedule_next(SchedulerPtr);

        internal void Attach() => _ = attach(SchedulerPtr);

        internal void Detach() => _ = detach(SchedulerPtr);

        /// <summary>
        /// Schedules the next asynchronous operation.
        /// </summary>
        internal void ScheduleNextOperation(AsyncOperationType type, AsyncOperationTarget target, ulong targetId)
        {
            int? taskId = Task.CurrentId;

            //// If the caller is the root task, then return.
            //if (taskId != null && taskId == this.Runtime.RootTaskId)
            //{
            //    return;
            //}

            if (!this.IsSchedulerRunning)
            {
                this.Stop();
                throw new ExecutionCanceledException();
            }

            // If the caller is the root task, then return.
            if (taskId != null && taskId != this.Runtime.RootTaskId)
            {
                // Checks if concurrency not controlled by the runtime was used.
                this.CheckNoExternalConcurrencyUsed();

                ActorOperation current = this.ScheduledOperation;
                current.SetNextOperation(type, target, targetId);

                this.ControlledTaskMap.GetOrAdd(Task.CurrentId.Value, current);
            }

            // Checks if the scheduling steps bound has been reached.
            this.CheckIfSchedulingStepsBoundIsReached();

            // Update the current execution state.
            var states = this.Runtime.GetHashedExecutionState(AbstractionLevel.Custom);
            ((this.Strategy as TemperatureCheckingStrategy).SchedulingStrategy as RandomStrategy).
                CaptureExecutionStep(this.Runtime.GetHashedExecutionState(AbstractionLevel.Default),
                this.Runtime.GetHashedExecutionState(AbstractionLevel.InboxOnly),
                states,
                this.Runtime.GetHashedExecutionState(AbstractionLevel.Full));
            HashedStates.Add(states);

            if (!this.OperationMap.Values.Any(op => op.Status == AsyncOperationStatus.Enabled))
            {
                // Checks if the program has livelocked.
                //this.CheckIfProgramHasLivelocked(ops.Select(op => op as ActorOperation));

                Debug.WriteLine("<ScheduleDebug> Schedule explored.");
                this.HasFullyExploredSchedule = true;
                this.Stop();
                return;
                //throw new ExecutionCanceledException();
            }

            int result = this.ScheduleNextOperation();

            var id = (ulong)scheduled_operation_id(SchedulerPtr);
            if (id > 0)
            {
                this.ScheduledOperation = this.OperationMap[id];
            }

            //Console.WriteLine($"next_operation {id}");
            //Console.WriteLine($"mirror_operation [{this.ScheduledSteps}]: {this.Mirror[this.ScheduledSteps].Type}");
            //Console.WriteLine($"mirror_operation [{this.ScheduledSteps}]: {this.Mirror[this.ScheduledSteps].ScheduledOperationId + 1}");
            //Console.ReadLine();

            this.ScheduledSteps++;

            if (!this.IsSchedulerRunning)
            {
                throw new ExecutionCanceledException();
            }
        }

        /// <summary>
        /// Returns the next nondeterministic boolean choice.
        /// </summary>
        internal bool GetNextNondeterministicBooleanChoice(int maxValue, string uniqueId = null)
        {
            // Checks if concurrency not controlled by the runtime was used.
            this.CheckNoExternalConcurrencyUsed();

            // Checks if the scheduling steps bound has been reached.
            this.CheckIfSchedulingStepsBoundIsReached();

            // Update the current execution state.
            var states = this.Runtime.GetHashedExecutionState(AbstractionLevel.Custom);
            ((this.Strategy as TemperatureCheckingStrategy).SchedulingStrategy as RandomStrategy).
                CaptureExecutionStep(this.Runtime.GetHashedExecutionState(AbstractionLevel.Default),
                this.Runtime.GetHashedExecutionState(AbstractionLevel.InboxOnly),
                states,
                this.Runtime.GetHashedExecutionState(AbstractionLevel.Full));
            HashedStates.Add(states);

            maxValue = 2;
            bool choice = Convert.ToBoolean(next_boolean(SchedulerPtr));
            //Console.WriteLine($"next_boolean {choice}");
            //Console.WriteLine($"mirror_boolean [{this.ScheduledSteps}]: {this.Mirror[this.ScheduledSteps].Type}");
            //Console.WriteLine($"mirror_boolean [{this.ScheduledSteps}]: {this.Mirror[this.ScheduledSteps].BooleanChoice}");
            //Console.ReadLine();

            this.ScheduledSteps++;

            if (uniqueId is null)
            {
                this.ScheduleTrace.AddNondeterministicBooleanChoice(choice);
            }
            else
            {
                this.ScheduleTrace.AddFairNondeterministicBooleanChoice(uniqueId, choice);
            }

            return choice;
        }

        /// <summary>
        /// Returns the next nondeterministic integer choice.
        /// </summary>
        internal int GetNextNondeterministicIntegerChoice(int maxValue)
        {
            // Checks if concurrency not controlled by the runtime was used.
            this.CheckNoExternalConcurrencyUsed();

            // Checks if the scheduling steps bound has been reached.
            this.CheckIfSchedulingStepsBoundIsReached();

            // Update the current execution state.
            var states = this.Runtime.GetHashedExecutionState(AbstractionLevel.Custom);
            ((this.Strategy as TemperatureCheckingStrategy).SchedulingStrategy as RandomStrategy).
                CaptureExecutionStep(this.Runtime.GetHashedExecutionState(AbstractionLevel.Default),
                this.Runtime.GetHashedExecutionState(AbstractionLevel.InboxOnly),
                states,
                this.Runtime.GetHashedExecutionState(AbstractionLevel.Full));
            HashedStates.Add(states);

            int choice = next_integer(SchedulerPtr, (ulong)maxValue);
            //Console.WriteLine($"next_integer {choice}");
            //Console.WriteLine($"mirror_integer [{this.ScheduledSteps}]: {this.Mirror[this.ScheduledSteps].Type}");
            //Console.WriteLine($"mirror_integer [{this.ScheduledSteps}]: {this.Mirror[this.ScheduledSteps].IntegerChoice}");
            //Console.ReadLine();

            this.ScheduledSteps++;

            this.ScheduleTrace.AddNondeterministicIntegerChoice(choice);

            return choice;
        }

        /// <summary>
        /// Notify that the specified asynchronous operation has been created
        /// and will start executing on the specified task.
        /// </summary>
        internal void NotifyOperationCreated(ActorOperation op, Task task)
        {
            this.ControlledTaskMap.TryAdd(task.Id, op);
            if (!this.OperationMap.ContainsKey(op.SourceId))
            {
                if (this.OperationMap.Count == 0)
                {
                    this.ScheduledOperation = op;
                }

                this.OperationMap.Add(op.SourceId, op);
            }

            Debug.WriteLine($"<ScheduleDebug> Registering the current operation of '{op.SourceName}'.");
        }

        /// <summary>
        /// Notify that an assertion has failed.
        /// </summary>
        internal void NotifyAssertionFailure(string text, bool killTasks = true, bool cancelExecution = true)
        {
            if (!this.BugFound)
            {
                this.BugReport = text;

                this.Runtime.LogWriter.OnError($"<ErrorLog> {text}");
                this.Runtime.LogWriter.OnStrategyError(this.Configuration.SchedulingStrategy, this.Strategy.GetDescription());

                this.BugFound = true;

                if (this.Configuration.AttachDebugger)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

            if (killTasks)
            {
                //this.Stop();
            }

            if (cancelExecution)
            {
                //throw new ExecutionCanceledException();
            }
        }

        /// <summary>
        /// Returns the enabled schedulable ids.
        /// </summary>
        internal HashSet<ulong> GetEnabledSchedulableIds()
        {
            var enabledSchedulableIds = new HashSet<ulong>();
            foreach (var actorInfo in this.OperationMap.Values)
            {
                if (actorInfo.Status is AsyncOperationStatus.Enabled)
                {
                    enabledSchedulableIds.Add(actorInfo.SourceId);
                }
            }

            return enabledSchedulableIds;
        }

        /// <summary>
        /// Returns a test report with the scheduling statistics.
        /// </summary>
        internal TestReport GetReport()
        {
            TestReport report = new TestReport(this.Configuration);

            report.NumOfStates = HashedStates.Count;
            if (this.BugFound)
            {
                report.NumOfFoundBugs++;
                report.BugReports.Add(this.BugReport);
            }

            if (this.Strategy.IsFair())
            {
                report.NumOfExploredFairSchedules++;
                report.TotalExploredFairSteps += this.ScheduledSteps;

                if (report.MinExploredFairSteps < 0 ||
                    report.MinExploredFairSteps > this.ScheduledSteps)
                {
                    report.MinExploredFairSteps = this.ScheduledSteps;
                }

                if (report.MaxExploredFairSteps < this.ScheduledSteps)
                {
                    report.MaxExploredFairSteps = this.ScheduledSteps;
                }

                if (this.Strategy.HasReachedMaxSchedulingSteps())
                {
                    report.MaxFairStepsHitInFairTests++;
                }

                if (this.ScheduledSteps >= report.Configuration.MaxUnfairSchedulingSteps)
                {
                    report.MaxUnfairStepsHitInFairTests++;
                }
            }
            else
            {
                report.NumOfExploredUnfairSchedules++;

                if (this.Strategy.HasReachedMaxSchedulingSteps())
                {
                    report.MaxUnfairStepsHitInUnfairTests++;
                }
            }

            return report;
        }

        /// <summary>
        /// Checks that no task that is not controlled by the runtime is currently executing.
        /// </summary>
        internal void CheckNoExternalConcurrencyUsed()
        {
            if (!this.IsSchedulerRunning)
            {
                throw new ExecutionCanceledException();
            }

            if (!Task.CurrentId.HasValue || !this.ControlledTaskMap.ContainsKey(Task.CurrentId.Value))
            {
                this.NotifyAssertionFailure(string.Format(CultureInfo.InvariantCulture,
                    "Task with id '{0}' that is not controlled by the Coyote runtime invoked a runtime method.",
                    Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString() : "<unknown>"));
            }
        }

        /// <summary>
        /// Checks for a livelock. This happens when there are no more enabled operations,
        /// but there is one or more blocked operations that are waiting to receive an event
        /// or for a task to complete.
        /// </summary>
        private void CheckIfProgramHasLivelocked(IEnumerable<ActorOperation> ops)
        {
            var blockedOnReceiveOperations = ops.Where(op => op.Status is AsyncOperationStatus.BlockedOnReceive).ToList();
            var blockedOnWaitOperations = ops.Where(op => op.Status is AsyncOperationStatus.BlockedOnWaitAll ||
                op.Status is AsyncOperationStatus.BlockedOnWaitAny).ToList();
            var blockedOnResourceSynchronization = ops.Where(op => op.Status is AsyncOperationStatus.BlockedOnResource).ToList();
            if (blockedOnReceiveOperations.Count == 0 &&
                blockedOnWaitOperations.Count == 0 &&
                blockedOnResourceSynchronization.Count == 0)
            {
                return;
            }

            string message = "Livelock detected.";
            if (blockedOnReceiveOperations.Count > 0)
            {
                for (int i = 0; i < blockedOnReceiveOperations.Count; i++)
                {
                    message += string.Format(CultureInfo.InvariantCulture, " '{0}'", blockedOnReceiveOperations[i].SourceName);
                    if (i == blockedOnReceiveOperations.Count - 2)
                    {
                        message += " and";
                    }
                    else if (i < blockedOnReceiveOperations.Count - 1)
                    {
                        message += ",";
                    }
                }

                message += blockedOnReceiveOperations.Count == 1 ? " is " : " are ";
                message += "waiting to receive an event, but no other controlled tasks are enabled.";
            }

            if (blockedOnWaitOperations.Count > 0)
            {
                for (int i = 0; i < blockedOnWaitOperations.Count; i++)
                {
                    message += string.Format(CultureInfo.InvariantCulture, " '{0}'", blockedOnWaitOperations[i].SourceName);
                    if (i == blockedOnWaitOperations.Count - 2)
                    {
                        message += " and";
                    }
                    else if (i < blockedOnWaitOperations.Count - 1)
                    {
                        message += ",";
                    }
                }

                message += blockedOnWaitOperations.Count == 1 ? " is " : " are ";
                message += "waiting for a task to complete, but no other controlled tasks are enabled.";
            }

            if (blockedOnResourceSynchronization.Count > 0)
            {
                for (int i = 0; i < blockedOnResourceSynchronization.Count; i++)
                {
                    message += string.Format(CultureInfo.InvariantCulture, " '{0}'", blockedOnResourceSynchronization[i].SourceName);
                    if (i == blockedOnResourceSynchronization.Count - 2)
                    {
                        message += " and";
                    }
                    else if (i < blockedOnResourceSynchronization.Count - 1)
                    {
                        message += ",";
                    }
                }

                message += blockedOnResourceSynchronization.Count == 1 ? " is " : " are ";
                message += "waiting to access a concurrent resource that is acquired by another task, ";
                message += "but no other controlled tasks are enabled.";
            }

            this.NotifyAssertionFailure(message);
        }

        /// <summary>
        /// Checks if the scheduling steps bound has been reached. If yes,
        /// it stops the scheduler and kills all enabled actors.
        /// </summary>
        private void CheckIfSchedulingStepsBoundIsReached()
        {
            if (this.Configuration.MaxFairSchedulingSteps > 0 &&
                this.Configuration.MaxFairSchedulingSteps == this.ScheduledSteps)
            {
                int bound = this.Strategy.IsFair() ? this.Configuration.MaxFairSchedulingSteps :
                    this.Configuration.MaxUnfairSchedulingSteps;
                string message = $"Scheduling steps bound of {bound} reached.";

                if (this.Configuration.ConsiderDepthBoundHitAsBug)
                {
                    this.NotifyAssertionFailure(message);
                }
                else
                {
                    Debug.WriteLine("<ScheduleDebug> {0}", message);
                    this.Stop();
                    throw new ExecutionCanceledException();
                }
            }

            // this.ScheduledSteps++;
        }

        /// <summary>
        /// Stops the scheduler.
        /// </summary>
        private void Stop()
        {
            Debug.WriteLine("STOP");
            this.IsSchedulerRunning = false;
            this.KillRemainingOperations();
            //this.Detach();

            this.CompleteOperation(this.ScheduledOperation);
            throw new ExecutionCanceledException();
        }

        /// <summary>
        /// Kills any remaining operations at the end of the schedule.
        /// </summary>
        private void KillRemainingOperations()
        {
            foreach (var op in this.OperationMap.Values)
            {
                op.IsActive = true;
                op.Status = AsyncOperationStatus.Completed;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing && SchedulerPtr != IntPtr.Zero)
            {
                //_ = dispose_scheduler(SchedulerPtr);
                //SchedulerPtr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Disposes scheduling resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OperationScheduler() => this.Dispose(false);
    }
}
