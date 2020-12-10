// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Coyote.Runtime;
using Microsoft.Coyote.TestingServices.Coverage;
using Microsoft.Coyote.TestingServices.Scheduling;
using Microsoft.Coyote.TestingServices.Scheduling.Strategies;
using Microsoft.Coyote.TestingServices.StateCaching;
using Microsoft.Coyote.TestingServices.Threading;
using Microsoft.Coyote.TestingServices.Timers;
using Microsoft.Coyote.TestingServices.Tracing.Error;
using Microsoft.Coyote.TestingServices.Tracing.Schedule;
using Microsoft.Coyote.Threading;
using Microsoft.Coyote.Timers;
using Microsoft.Coyote.Utilities;

namespace Microsoft.Coyote.TestingServices.Runtime
{
    /// <summary>
    /// Runtime for systematically testing actors by controlling the scheduler.
    /// </summary>
    internal sealed class SystematicTestingRuntime : ActorRuntime
    {
        /// <summary>
        /// Stores the runtime that executes each operation in a given asynchronous context.
        /// </summary>
        private static readonly AsyncLocal<SystematicTestingRuntime> AsyncLocalRuntime =
            new AsyncLocal<SystematicTestingRuntime>();

        /// <summary>
        /// The asynchronous operation scheduler.
        /// </summary>
        internal OperationScheduler Scheduler;

        /// <summary>
        /// The controlled task scheduler.
        /// </summary>
        private readonly ControlledTaskScheduler TaskScheduler;

        /// <summary>
        /// The bug trace.
        /// </summary>
        internal BugTrace BugTrace;

        /// <summary>
        /// Data structure containing information
        /// regarding testing coverage.
        /// </summary>
        internal CoverageInfo CoverageInfo;

        /// <summary>
        /// Interface for registering runtime operations.
        /// </summary>
        internal IRegisterRuntimeOperation Reporter;

        /// <summary>
        /// The Coyote program state cache.
        /// </summary>
        internal StateCache StateCache;

        /// <summary>
        /// List of monitors in the program.
        /// </summary>
        private readonly List<Monitor> Monitors;

        /// <summary>
        /// Map from unique ids to operations.
        /// </summary>
        private readonly ConcurrentDictionary<ulong, ActorOperation> ActorOperations;

        /// <summary>
        /// Map that stores all unique names and their corresponding actor ids.
        /// </summary>
        internal readonly ConcurrentDictionary<string, ActorId> NameValueToActorId;

        /// <summary>
        /// Set of all actor Ids created by this runtime.
        /// </summary>
        internal HashSet<ActorId> CreatedActorIds;

        /// <summary>
        /// The root task id.
        /// </summary>
        internal readonly int? RootTaskId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystematicTestingRuntime"/> class.
        /// </summary>
        internal SystematicTestingRuntime(Configuration configuration, ISchedulingStrategy strategy,
            IRegisterRuntimeOperation reporter)
            : base(configuration)
        {
            this.Monitors = new List<Monitor>();
            this.ActorOperations = new ConcurrentDictionary<ulong, ActorOperation>();
            this.RootTaskId = Task.CurrentId;
            this.CreatedActorIds = new HashSet<ActorId>();
            this.NameValueToActorId = new ConcurrentDictionary<string, ActorId>();

            this.BugTrace = new BugTrace();
            this.StateCache = new StateCache(this);
            this.CoverageInfo = new CoverageInfo();
            this.Reporter = reporter;

            var scheduleTrace = new ScheduleTrace();
            if (configuration.EnableLivenessChecking && configuration.EnableCycleDetection)
            {
                strategy = new CycleDetectionStrategy(configuration, this.StateCache,
                    scheduleTrace, this.Monitors, strategy);
            }
            else if (configuration.EnableLivenessChecking)
            {
                strategy = new TemperatureCheckingStrategy(configuration, this.Monitors, strategy);
            }

            this.Scheduler = new OperationScheduler(this, strategy, scheduleTrace, this.Configuration);
            this.TaskScheduler = new ControlledTaskScheduler(this, this.Scheduler.ControlledTaskMap);
            // this.SyncContext = new ControlledSynchronizationContext(this);
        }

        /// <summary>
        /// Creates an actor id that is uniquely tied to the specified unique name. The
        /// returned actor id can either be a fresh id (not yet bound to any actor),
        /// or it can be bound to a previously created actor. In the second case, this
        /// actor id can be directly used to communicate with the corresponding actor.
        /// </summary>
        public override ActorId CreateActorIdFromName(Type type, string actorName)
        {
            // It is important that all actor ids use the monotonically incrementing
            // value as the id during testing, and not the unique name.
            var mid = new ActorId(type, actorName, this);
            return this.NameValueToActorId.GetOrAdd(actorName, mid);
        }

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with
        /// the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        public override ActorId CreateActor(Type type, Event e = null, Guid opGroupId = default) =>
            this.CreateActor(null, type, null, e, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and
        /// with the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        public override ActorId CreateActor(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            this.CreateActor(null, type, actorName, e, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified type, using the specified <see cref="ActorId"/>.
        /// This method optionally passes an <see cref="Event"/> to the new actor, which can only
        /// be used to access its payload, and cannot be handled.
        /// </summary>
        public override ActorId CreateActor(ActorId mid, Type type, Event e = null, Guid opGroupId = default)
        {
            this.Assert(mid != null, "Cannot create an actor using a null actor id.");
            return this.CreateActor(mid, type, null, e, opGroupId);
        }

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with the
        /// specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when
        /// the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(Type type, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(null, type, null, e, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and with
        /// the specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when the
        /// actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(null, type, actorName, e, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/>, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This
        /// event can only be used to access its payload, and cannot be handled. The method
        /// returns only when the actor is initialized and the <see cref="Event"/> (if any)
        /// is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, Event e = null, Guid opGroupId = default)
        {
            this.Assert(mid != null, "Cannot create an actor using a null actor id.");
            return this.CreateActorAndExecuteAsync(mid, type, null, e, opGroupId);
        }

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with the
        /// specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when
        /// the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(Type type, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(null, type, null, e, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and with
        /// the specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when the
        /// actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(null, type, actorName, e, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/>, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This
        /// event can only be used to access its payload, and cannot be handled. The method
        /// returns only when the actor is initialized and the <see cref="Event"/> (if any)
        /// is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(ActorId mid, Type type, Event e = null, Guid opGroupId = default)
        {
            this.Assert(mid != null, "Cannot create an actor using a null actor id.");
            return this.CreateActorAndExecuteAsync(mid, type, null, e, opGroupId);
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor.
        /// </summary>
        public override void SendEvent(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null)
        {
            this.SendEvent(target, e, this.GetExecutingActor<Actor>(), opGroupId, options);
        }

        /// <summary>
        /// Sends an <see cref="Event"/> to an actor. Returns immediately if the target actor was already
        /// running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        public override Task<bool> SendEventAndExecuteAsync(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null) =>
            this.SendEventAndExecuteAsync(target, e, this.GetExecutingActor<Actor>(), opGroupId, options);

        /// <summary>
        /// Sends an <see cref="Event"/> to an actor. Returns immediately if the target actor was already
        /// running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        public override Task<bool> SendEventAndExecute(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null) =>
            this.SendEventAndExecuteAsync(target, e, opGroupId, options);

        /// <summary>
        /// Returns the operation group id of the specified actor. Returns <see cref="Guid.Empty"/>
        /// if the id is not set, or if the <see cref="ActorId"/> is not associated with this runtime.
        /// During testing, the runtime asserts that the specified actor is currently executing.
        /// </summary>
        public override Guid GetCurrentOperationGroupId(ActorId currentActor)
        {
            this.Assert(currentActor == this.GetCurrentActorId(),
                "Trying to access the operation group id of '{0}', which is not the currently executing actor.",
                currentActor);

            Actor actor = this.GetActorFromId<Actor>(currentActor);
            return actor is null ? Guid.Empty : actor.OperationGroupId;
        }

        /// <summary>
        /// Runs the specified test inside a synchronous test harness actor.
        /// </summary>
        internal void RunTestHarness(Delegate test, string testName)
        {
            this.Assert(Task.CurrentId != null, "The test harness actor must execute inside a task.");
            this.Assert(test != null, "The test harness actor cannot execute a null test.");

            testName = string.IsNullOrEmpty(testName) ? "anonymous" : testName;
            this.Logger.WriteLine($"<TestLog> Running test '{testName}'.");

            var actor = new TestEntryPointWorkActor(this, test);
            this.DispatchWork(actor, null);
        }

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This event
        /// can only be used to access its payload, and cannot be handled.
        /// </summary>
        internal ActorId CreateActor(ActorId mid, Type type, string actorName, Event e = null, Guid opGroupId = default)
        {
            Actor creator = this.GetExecutingActor<Actor>();
            return this.CreateActor(mid, type, actorName, e, creator, opGroupId);
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>.
        /// </summary>
        internal override ActorId CreateActor(ActorId mid, Type type, string actorName, Event e,
            Actor creator, Guid opGroupId)
        {
            this.AssertCorrectCallerActor(creator, "CreateActor");
            if (creator != null)
            {
                this.AssertNoPendingTransitionStatement(creator, "create an actor");
            }

            Actor actor = this.CreateActor(mid, type, actorName, creator, opGroupId);

            this.BugTrace.AddCreateActorStep(creator, actor.Id, e is null ? null : new EventInfo(e));
            this.RunActorEventHandler(actor, e, true, null, null);

            return actor.Id;
        }

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This event
        /// can only be used to access its payload, and cannot be handled. The method returns only
        /// when the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        internal Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, string actorName, Event e = null,
            Guid opGroupId = default)
        {
            Actor creator = this.GetExecutingActor<Actor>();
            return this.CreateActorAndExecuteAsync(mid, type, actorName, e, creator, opGroupId);
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>. The
        /// method returns only when the actor is initialized and the <see cref="Event"/>
        /// (if any) is handled.
        /// </summary>
        internal override async Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, string actorName, Event e,
            Actor creator, Guid opGroupId)
        {
            this.AssertCorrectCallerActor(creator, "CreateActorAndExecute");
            this.Assert(creator != null,
                "Only an actor can call 'CreateActorAndExecute': avoid calling it directly from the 'Test' method; instead call it through a 'harness' actor.");
            this.AssertNoPendingTransitionStatement(creator, "create an actor");

            Actor actor = this.CreateActor(mid, type, actorName, creator, opGroupId);

            this.BugTrace.AddCreateActorStep(creator, actor.Id, e is null ? null : new EventInfo(e));
            this.RunActorEventHandler(actor, e, true, creator, null);

            // Wait until the actor reaches quiescence.
            await creator.Receive(typeof(QuiescentEvent), rev => (rev as QuiescentEvent).ActorId == actor.Id);

            return await Task.FromResult(actor.Id);
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>.
        /// </summary>
        private Actor CreateActor(ActorId mid, Type type, string actorName, Actor creator, Guid opGroupId)
        {
            this.Assert(type.IsSubclassOf(typeof(Actor)), "Type '{0}' is not an actor.", type.FullName);

            // Using ulong.MaxValue because a 'Create' operation cannot specify
            // the id of its target, because the id does not exist yet.
            this.Scheduler.ScheduleNextOperation(AsyncOperationType.Create, AsyncOperationTarget.Task, ulong.MaxValue);
            ResetProgramCounter(creator);

            if (mid is null)
            {
                mid = new ActorId(type, actorName, this);
            }
            else
            {
                this.Assert(mid.Runtime is null || mid.Runtime == this, "Unbound actor id '{0}' was created by another runtime.", mid.Value);
                this.Assert(mid.Type == type.FullName, "Cannot bind actor id '{0}' of type '{1}' to an actor of type '{2}'.",
                    mid.Value, mid.Type, type.FullName);
                mid.Bind(this);
            }

            // The operation group id of the actor is set using the following precedence:
            // (1) To the specified actor creation operation group id, if it is non-empty.
            // (2) To the operation group id of the creator actor, if it exists and is non-empty.
            // (3) To the empty operation group id.
            if (opGroupId == Guid.Empty && creator != null)
            {
                opGroupId = creator.OperationGroupId;
            }

            Actor actor = ActorFactory.Create(type);
            IActorStateManager stateManager = new SerializedActorStateManager(this, actor, opGroupId);
            IEventQueue eventQueue = new SerializedActorEventQueue(stateManager, actor);
            actor.Initialize(this, mid, stateManager, eventQueue);
            actor.InitializeStateInformation();

            if (this.Configuration.ReportActivityCoverage)
            {
                this.ReportActivityCoverageOfActor(actor);
            }

            bool result = this.ActorMap.TryAdd(mid, actor);
            this.Assert(result, "Actor id '{0}' is used by an existing actor.", mid.Value);

            this.Assert(!this.CreatedActorIds.Contains(mid),
                "Actor id '{0}' of a previously halted actor cannot be reused to create a new actor of type '{1}'",
                mid.Value, type.FullName);
            this.CreatedActorIds.Add(mid);
            this.ActorOperations.GetOrAdd(mid.Value, new ActorOperation(actor));

            this.LogWriter.OnCreateActor(mid, creator?.Id);

            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.RegisterCreateActor(creator?.Id, mid);
            }

            return actor;
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor.
        /// </summary>
        internal override void SendEvent(ActorId target, Event e, AsyncActor sender, Guid opGroupId, SendOptions options)
        {
            if (sender != null)
            {
                this.Assert(target != null, "Actor '{0}' is sending to a null actor.", sender.Id);
                this.Assert(e != null, "Actor '{0}' is sending a null event.", sender.Id);
            }
            else
            {
                this.Assert(target != null, "Cannot send to a null actor.");
                this.Assert(e != null, "Cannot send a null event.");
            }

            this.AssertCorrectCallerActor(sender as Actor, "SendEvent");

            EnqueueStatus enqueueStatus = this.EnqueueEvent(target, e, sender, opGroupId, options,
                out Actor targetActor, out EventInfo eventInfo);
            if (enqueueStatus is EnqueueStatus.EventHandlerNotRunning)
            {
                this.RunActorEventHandler(targetActor, null, false, null, eventInfo);
            }
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor. Returns immediately if the target actor was
        /// already running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        internal override async Task<bool> SendEventAndExecuteAsync(ActorId target, Event e, AsyncActor sender,
            Guid opGroupId, SendOptions options)
        {
            this.Assert(sender is Actor,
                "Only an actor can call 'SendEventAndExecute': avoid calling it directly from the 'Test' method; instead call it through a 'harness' actor.");
            this.Assert(target != null, "Actor '{0}' is sending to a null actor.", sender.Id);
            this.Assert(e != null, "Actor '{0}' is sending a null event.", sender.Id);
            this.AssertCorrectCallerActor(sender as Actor, "SendEventAndExecute");

            EnqueueStatus enqueueStatus = this.EnqueueEvent(target, e, sender, opGroupId, options,
                out Actor targetActor, out EventInfo eventInfo);
            if (enqueueStatus is EnqueueStatus.EventHandlerNotRunning)
            {
                this.RunActorEventHandler(targetActor, null, false, sender as Actor, eventInfo);

                // Wait until the actor reaches quiescence.
                await (sender as Actor).Receive(typeof(QuiescentEvent), rev => (rev as QuiescentEvent).ActorId == target);
                return true;
            }

            // 'EnqueueStatus.EventHandlerNotRunning' is not returned by 'EnqueueEvent' (even when
            // the actor was previously inactive) when the event 'e' requires no action by the
            // actor (i.e., it implicitly handles the event).
            return enqueueStatus is EnqueueStatus.Dropped || enqueueStatus is EnqueueStatus.NextEventUnavailable;
        }

        /// <summary>
        /// Enqueues an event to the actor with the specified id.
        /// </summary>
        private EnqueueStatus EnqueueEvent(ActorId target, Event e, AsyncActor sender, Guid opGroupId,
            SendOptions options, out Actor targetActor, out EventInfo eventInfo)
        {
            this.Assert(this.CreatedActorIds.Contains(target),
                "Cannot send event '{0}' to actor id '{1}' that was never previously bound to an actor of type '{2}'",
                e.GetType().FullName, target.Value, target.Type);

            AsyncOperationType opType = AsyncOperationType.Send;

            if (e.GetType().Name == "eTerminateRequest")
            {
                // Hardcoded for now, can convert to a generic attribute for production use.
                opType = AsyncOperationType.InjectFailure;
                this.Logger.WriteLine("<FailureInjectionLog> Injecting a failure.");
            }

            if (e.GetType().Name == "eFailure")
            {
                // Hardcoded for now, can convert to a generic attribute for production use.
                opType = AsyncOperationType.InjectFailure;
                this.Logger.WriteLine("<FailureInjectionLog> Injecting a failure.");
            }

            this.Scheduler.ScheduleNextOperation(opType, AsyncOperationTarget.Inbox, target.Value);
            ResetProgramCounter(sender as Actor);

            // The operation group id of this operation is set using the following precedence:
            // (1) To the specified send operation group id, if it is non-empty.
            // (2) To the operation group id of the sender actor, if it exists and is non-empty.
            // (3) To the empty operation group id.
            if (opGroupId == Guid.Empty && sender != null)
            {
                opGroupId = sender.OperationGroupId;
            }

            targetActor = this.GetActorFromId<Actor>(target);
            if (targetActor is null)
            {
                this.LogWriter.OnSend(target, sender?.Id, (sender as Actor)?.CurrentStateName ?? string.Empty,
                    e.GetType().FullName, opGroupId, isTargetHalted: true);
                this.Assert(options is null || !options.MustHandle,
                    "A must-handle event '{0}' was sent to the halted actor '{1}'.", e.GetType().FullName, target);
                this.TryHandleDroppedEvent(e, target);
                eventInfo = null;
                return EnqueueStatus.Dropped;
            }

            if (sender is Actor)
            {
                this.AssertNoPendingTransitionStatement(sender as Actor, "send an event");
            }

            EnqueueStatus enqueueStatus = this.EnqueueEvent(targetActor, e, sender, opGroupId, options, out eventInfo);
            if (enqueueStatus == EnqueueStatus.Dropped)
            {
                this.TryHandleDroppedEvent(e, target);
            }

            return enqueueStatus;
        }

        /// <summary>
        /// Enqueues an event to the actor with the specified id.
        /// </summary>
        private EnqueueStatus EnqueueEvent(Actor actor, Event e, AsyncActor sender, Guid opGroupId,
            SendOptions options, out EventInfo eventInfo)
        {
            EventOriginInfo originInfo;
            if (sender is Actor senderActor)
            {
                originInfo = new EventOriginInfo(sender.Id, senderActor.GetType().FullName,
                    NameResolver.GetStateNameForLogging(senderActor.CurrentState));
            }
            else
            {
                // Message comes from the environment.
                originInfo = new EventOriginInfo(null, "Env", "Env");
            }

            eventInfo = new EventInfo(e, originInfo)
            {
                MustHandle = options?.MustHandle ?? false,
                Assert = options?.Assert ?? -1,
                Assume = options?.Assume ?? -1,
                SendStep = this.Scheduler.ScheduledSteps
            };

            this.LogWriter.OnSend(actor.Id, sender?.Id, (sender as Actor)?.CurrentStateName ?? string.Empty,
                e.GetType().FullName, opGroupId, isTargetHalted: false);

            if (sender != null)
            {
                var stateName = sender is Actor ? (sender as Actor).CurrentStateName : string.Empty;
                this.BugTrace.AddSendEventStep(sender.Id, stateName, eventInfo, actor.Id);
                if (this.Configuration.EnableDataRaceDetection)
                {
                    this.Reporter.RegisterEnqueue(sender.Id, actor.Id, e, (ulong)this.Scheduler.ScheduledSteps);
                }
            }

            return actor.Enqueue(e, opGroupId, eventInfo);
        }

        /// <summary>
        /// Runs a new asynchronous event handler for the specified actor.
        /// This is a fire and forget invocation.
        /// </summary>
        /// <param name="actor">Actor that executes this event handler.</param>
        /// <param name="initialEvent">Event for initializing the actor.</param>
        /// <param name="isFresh">If true, then this is a new actor.</param>
        /// <param name="syncCaller">Caller actor that is blocked for quiscence.</param>
        /// <param name="enablingEvent">If non-null, the event info of the sent event that caused the event handler to be restarted.</param>
        private void RunActorEventHandler(Actor actor, Event initialEvent, bool isFresh, Actor syncCaller, EventInfo enablingEvent)
        {
            ActorOperation op = this.GetAsynchronousOperation(actor.Id.Value);

            Task task = new Task(async () =>
            {
                // Set the executing runtime in the local asynchronous context,
                // allowing future retrieval in the same asynchronous call stack.
                AsyncLocalRuntime.Value = this;

                try
                {
                    OperationScheduler.NotifyOperationStarted(op);

                    if (isFresh)
                    {
                        await actor.GotoStartState(initialEvent);
                    }

                    await actor.RunEventHandlerAsync();

                    if (syncCaller != null)
                    {
                        this.EnqueueEvent(syncCaller, new QuiescentEvent(actor.Id), actor, actor.OperationGroupId, null, out EventInfo _);
                    }

                    IO.Debug.WriteLine($"<ScheduleDebug> Completed event handler of '{actor.Id}' on task '{Task.CurrentId}'.");
                    op.OnCompleted();

                    if (actor.IsHalted)
                    {
                        this.Scheduler.ScheduleNextOperation(AsyncOperationType.Stop, AsyncOperationTarget.Task, actor.Id.Value);
                    }
                    else
                    {
                        this.Scheduler.ScheduleNextOperation(AsyncOperationType.Receive, AsyncOperationTarget.Inbox, actor.Id.Value);
                    }

                    IO.Debug.WriteLine($"<ScheduleDebug> Terminated event handler of '{actor.Id}' on task '{Task.CurrentId}'.");
                    ResetProgramCounter(actor);
                }
                catch (Exception ex)
                {
                    Exception innerException = ex;
                    while (innerException is TargetInvocationException)
                    {
                        innerException = innerException.InnerException;
                    }

                    if (innerException is AggregateException)
                    {
                        innerException = innerException.InnerException;
                    }

                    if (innerException is ExecutionCanceledException)
                    {
                        IO.Debug.WriteLine($"<Exception> ExecutionCanceledException was thrown from actor '{actor.Id}'.");
                    }
                    else if (innerException is TaskSchedulerException)
                    {
                        IO.Debug.WriteLine($"<Exception> TaskSchedulerException was thrown from actor '{actor.Id}'.");
                    }
                    else if (innerException is ObjectDisposedException)
                    {
                        IO.Debug.WriteLine($"<Exception> ObjectDisposedException was thrown from actor '{actor.Id}' with reason '{ex.Message}'.");
                    }
                    else
                    {
                        // Reports the unhandled exception.
                        string message = string.Format(CultureInfo.InvariantCulture,
                            $"Exception '{ex.GetType()}' was thrown in actor '{actor.Id}', " +
                            $"'{ex.Source}':\n" +
                            $"   {ex.Message}\n" +
                            $"The stack trace is:\n{ex.StackTrace}");
                        this.Scheduler.NotifyAssertionFailure(message, killTasks: true, cancelExecution: false);
                    }
                }
                finally
                {
                    if (actor.IsHalted)
                    {
                        this.ActorMap.TryRemove(actor.Id, out AsyncActor _);
                    }
                }
            });

            op.OnCreated(enablingEvent?.SendStep ?? 0);
            this.Scheduler.NotifyOperationCreated(op, task);

            task.Start(this.TaskScheduler);
            this.Scheduler.WaitForOperationToStart(op);
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask CreateActorTask(Action action, CancellationToken cancellationToken)
        {
            this.Assert(action != null, "The task cannot execute a null action.");
            var actor = new ActionWorkActor(this, action);
            this.DispatchWork(actor, null);
            return new ControlledActorTask(this, actor.AwaiterTask, ActorTaskType.ExplicitTask);
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask CreateActorTask(Func<Task> function, CancellationToken cancellationToken)
        {
            this.Assert(function != null, "The task cannot execute a null function.");
            var actor = new FuncWorkActor(this, function);
            this.DispatchWork(actor, null);
            return new ControlledActorTask(this, actor.AwaiterTask, ActorTaskType.ExplicitTask);
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask{TResult}"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask<TResult> CreateActorTask<TResult>(Func<TResult> function,
            CancellationToken cancellationToken)
        {
            this.Assert(function != null, "The task cannot execute a null function.");
            var actor = new FuncWorkActor<TResult>(this, function);
            this.DispatchWork(actor, null);
            return new ControlledActorTask<TResult>(this, actor.AwaiterTask, ActorTaskType.ExplicitTask);
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask{TResult}"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask<TResult> CreateActorTask<TResult>(Func<Task<TResult>> function,
            CancellationToken cancellationToken)
        {
            this.Assert(function != null, "The task cannot execute a null function.");
            var actor = new FuncTaskWorkActor<TResult>(this, function);
            this.DispatchWork(actor, null);
            return new ControlledActorTask<TResult>(this, actor.AwaiterTask, ActorTaskType.ExplicitTask);
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous delay.
        /// </summary>
        internal override ActorTask CreateActorTask(int millisecondsDelay, CancellationToken cancellationToken)
        {
            if (millisecondsDelay == 0)
            {
                // If the delay is 0, then complete synchronously.
                return ActorTask.CompletedTask;
            }

            var actor = new DelayWorkActor(this);
            this.DispatchWork(actor, null);
            return new ControlledActorTask(this, actor.AwaiterTask, ActorTaskType.ExplicitTask);
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to complete with the specified task.
        /// </summary>
        internal override ActorTask CreateCompletionActorTask(Task task)
        {
            if (AsyncLocalRuntime.Value != this)
            {
                throw new ExecutionCanceledException();
            }

            this.Scheduler.CheckNoExternalConcurrencyUsed();
            // var actor = new TaskCompletionWorkActor(this, task);
            // this.DispatchWork(actor, task);
            return new ControlledActorTask(this, task, ActorTaskType.CompletionSourceTask);
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to complete with the specified task.
        /// </summary>
        internal override ActorTask<TResult> CreateCompletionActorTask<TResult>(Task<TResult> task)
        {
            if (AsyncLocalRuntime.Value != this)
            {
                throw new ExecutionCanceledException();
            }

            this.Scheduler.CheckNoExternalConcurrencyUsed();
            // var actor = new TaskCompletionWorkActor<TResult>(this, task);
            // this.DispatchWork(actor, task);
            return new ControlledActorTask<TResult>(this, /*actor.AwaiterTask*/task,
                ActorTaskType.CompletionSourceTask);
        }

        /// <summary>
        /// Asynchronously waits for the specified tasks to complete.
        /// </summary>
        internal override ActorTask WaitAllTasksAsync(IEnumerable<Task> tasks)
        {
            this.Assert(tasks != null, "Cannot wait for a null array of tasks to complete.");
            this.Assert(tasks.Count() > 0, "Cannot wait for zero tasks to complete.");

            AsyncActor caller = this.GetExecutingActor<AsyncActor>();
            if (caller is null)
            {
                // TODO: throw an error, as a non-controlled task is awaiting?
                return new ActorTask(Task.WhenAll(tasks));
            }

            ActorOperation callerOp = this.GetAsynchronousOperation(caller.Id.Value);
            foreach (var task in tasks)
            {
                if (!task.IsCompleted)
                {
                    callerOp.OnWaitTask(task, true);
                }
            }

            if (callerOp.Status == AsyncOperationStatus.BlockedOnWaitAll)
            {
                // Only schedule if the task is not already completed.
                this.Scheduler.ScheduleNextOperation(AsyncOperationType.Join, AsyncOperationTarget.Task, caller.Id.Value);
            }

            return ActorTask.CompletedTask;
        }

        /// <summary>
        /// Asynchronously waits for all specified tasks to complete.
        /// </summary>
        internal override ActorTask<TResult[]> WaitAllTasksAsync<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            this.Assert(tasks != null, "Cannot wait for a null array of tasks to complete.");
            this.Assert(tasks.Count() > 0, "Cannot wait for zero tasks to complete.");

            AsyncActor caller = this.GetExecutingActor<AsyncActor>();
            if (caller is null)
            {
                // TODO: throw an error, as a non-controlled task is awaiting?
                return new ActorTask<TResult[]>(Task.WhenAll(tasks));
            }

            int size = 0;
            ActorOperation callerOp = this.GetAsynchronousOperation(caller.Id.Value);
            foreach (var task in tasks)
            {
                size++;
                if (!task.IsCompleted)
                {
                    callerOp.OnWaitTask(task, true);
                }
            }

            if (callerOp.Status == AsyncOperationStatus.BlockedOnWaitAll)
            {
                // Only schedule if the task is not already completed.
                this.Scheduler.ScheduleNextOperation(AsyncOperationType.Join, AsyncOperationTarget.Task, caller.Id.Value);
            }

            int idx = 0;
            TResult[] result = new TResult[size];
            foreach (var task in tasks)
            {
                result[idx] = task.Result;
                idx++;
            }

            return ActorTask.FromResult(result);
        }

        /// <summary>
        /// Asynchronously waits for any of the specified tasks to complete.
        /// </summary>
        internal override ActorTask<Task> WaitAnyTaskAsync(IEnumerable<Task> tasks)
        {
            this.Assert(tasks != null, "Cannot wait for a null array of tasks to complete.");
            this.Assert(tasks.Count() > 0, "Cannot wait for zero tasks to complete.");

            AsyncActor caller = this.GetExecutingActor<AsyncActor>();
            if (caller is null)
            {
                // TODO: throw an error, as a non-controlled task is awaiting?
                return new ActorTask<Task>(Task.WhenAny(tasks));
            }

            ActorOperation callerOp = this.GetAsynchronousOperation(caller.Id.Value);
            foreach (var task in tasks)
            {
                if (!task.IsCompleted)
                {
                    callerOp.OnWaitTask(task, false);
                }
            }

            if (callerOp.Status == AsyncOperationStatus.BlockedOnWaitAny)
            {
                // Only schedule if the task is not already completed.
                this.Scheduler.ScheduleNextOperation(AsyncOperationType.Join, AsyncOperationTarget.Task, caller.Id.Value);
            }

            Task result = null;
            foreach (var task in tasks)
            {
                if (task.IsCompleted)
                {
                    result = task;
                    break;
                }
            }

            return ActorTask.FromResult(result);
        }

        /// <summary>
        /// Asynchronously waits for any of the specified tasks to complete.
        /// </summary>
        internal override ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            this.Assert(tasks != null, "Cannot wait for a null array of tasks to complete.");
            this.Assert(tasks.Count() > 0, "Cannot wait for zero tasks to complete.");

            AsyncActor caller = this.GetExecutingActor<AsyncActor>();
            if (caller is null)
            {
                // TODO: throw an error, as a non-controlled task is awaiting?
                return new ActorTask<Task<TResult>>(Task.WhenAny(tasks));
            }

            int size = 0;
            ActorOperation callerOp = this.GetAsynchronousOperation(caller.Id.Value);
            foreach (var task in tasks)
            {
                size++;
                if (!task.IsCompleted)
                {
                    callerOp.OnWaitTask(task, false);
                }
            }

            if (callerOp.Status == AsyncOperationStatus.BlockedOnWaitAny)
            {
                // Only schedule if the task is not already completed.
                this.Scheduler.ScheduleNextOperation(AsyncOperationType.Join, AsyncOperationTarget.Task, caller.Id.Value);
            }

            Task<TResult> result = null;
            foreach (var task in tasks)
            {
                if (task.IsCompleted)
                {
                    result = task;
                    break;
                }
            }

            return ActorTask.FromResult(result);
        }

        /// <summary>
        /// Schedules the specified work actor to be executed asynchronously.
        /// This is a fire and forget invocation.
        /// </summary>
        internal void DispatchWork(WorkActor actor, Task parentTask)
        {
            // this.Scheduler.ScheduleNextOperation(AsyncOperationType.Create, AsyncOperationTarget.Task, ulong.MaxValue);

            ActorOperation op = new ActorOperation(actor);

            this.ActorOperations.GetOrAdd(actor.Id.Value, op);
            this.ActorMap.TryAdd(actor.Id, actor);
            this.CreatedActorIds.Add(actor.Id);

            // TODO: change to custom log.
            this.LogWriter.OnCreateActor(actor.Id, null);

            Task task = new Task(async () =>
            {
                // Set the executing runtime in the local asynchronous context,
                // allowing future retrieval in the same asynchronous call stack.
                AsyncLocalRuntime.Value = this;

                // SynchronizationContext.SetSynchronizationContext(this.SyncContext);

                try
                {
                    OperationScheduler.NotifyOperationStarted(op);

                    await actor.ExecuteAsync();

                    IO.Debug.WriteLine($"<ScheduleDebug> Completed '{actor.Id}' on task '{Task.CurrentId}'.");
                    op.OnCompleted();

                    this.Scheduler.ScheduleNextOperation(AsyncOperationType.Stop, AsyncOperationTarget.Task, actor.Id.Value);

                    IO.Debug.WriteLine($"<ScheduleDebug> Terminated '{actor.Id}' on task '{Task.CurrentId}'.");
                }
                catch (Exception ex)
                {
                    Exception innerException = ex;
                    while (innerException is TargetInvocationException)
                    {
                        innerException = innerException.InnerException;
                    }

                    if (innerException is AggregateException)
                    {
                        innerException = innerException.InnerException;
                    }

                    if (innerException is ExecutionCanceledException)
                    {
                        IO.Debug.WriteLine($"<Exception> ExecutionCanceledException was thrown from actor '{actor.Id}'.");
                    }
                    else if (innerException is TaskSchedulerException)
                    {
                        IO.Debug.WriteLine($"<Exception> TaskSchedulerException was thrown from actor '{actor.Id}'.");
                    }
                    else
                    {
                        // Reports the unhandled exception.
                        string message = string.Format(CultureInfo.InvariantCulture,
                            $"Exception '{ex.GetType()}' was thrown in actor {actor.Id}, " +
                            $"'{ex.Source}':\n" +
                            $"   {ex.Message}\n" +
                            $"The stack trace is:\n{ex.StackTrace}");
                        this.Scheduler.NotifyAssertionFailure(message, killTasks: true, cancelExecution: false);
                    }
                }
                finally
                {
                    // TODO: properly cleanup actor tasks.
                    this.ActorMap.TryRemove(actor.Id, out AsyncActor _);
                }
            });

            op.OnCreated(0);
            if (parentTask != null)
            {
                op.OnWaitTask(parentTask);
            }

            this.Scheduler.NotifyOperationCreated(op, task);

            task.Start();
            // task.Start(this.TaskScheduler);
            this.Scheduler.WaitForOperationToStart(op);

            this.Scheduler.ScheduleNextOperation(AsyncOperationType.Yield, AsyncOperationTarget.Task, actor.Id.Value);
        }

        /// <summary>
        /// Waits until all actors have finished execution.
        /// </summary>
        internal async Task WaitAsync()
        {
            await this.Scheduler.WaitAsync();
            this.IsRunning = false;
        }

        /// <summary>
        /// Creates a new timer that sends a <see cref="TimerElapsedEvent"/> to its owner actor.
        /// </summary>
        internal override IActorTimer CreateActorTimer(TimerInfo info, Actor owner)
        {
            var mid = this.CreateActorId(typeof(MockActorTimer));
            this.CreateActor(mid, typeof(MockActorTimer), new TimerSetupEvent(info, owner, this.Configuration.TimeoutDelay));
            return this.GetActorFromId<MockActorTimer>(mid);
        }

        /// <summary>
        /// Tries to create a new monitor of the given type.
        /// </summary>
        internal override void TryCreateMonitor(Type type)
        {
            if (this.Monitors.Any(m => m.GetType() == type))
            {
                // Idempotence: only one monitor per type can exist.
                return;
            }

            this.Assert(type.IsSubclassOf(typeof(Monitor)), "Type '{0}' is not a subclass of Monitor.", type.FullName);

            ActorId mid = new ActorId(type, null, this);

            // ActorOperation op = this.ActorOperations.GetOrAdd(mid.Value, new ActorOperation(mid));
            // this.Scheduler.NotifyMonitorRegistered(op);

            Monitor monitor = Activator.CreateInstance(type) as Monitor;
            monitor.Initialize(this, mid);
            monitor.InitializeStateInformation();

            this.LogWriter.OnCreateMonitor(type.FullName, monitor.Id);

            this.ReportActivityCoverageOfMonitor(monitor);
            this.BugTrace.AddCreateMonitorStep(mid);

            this.Monitors.Add(monitor);

            monitor.GotoStartState();
        }

        /// <summary>
        /// Invokes the specified monitor with the given event.
        /// </summary>
        internal override void Monitor(Type type, AsyncActor sender, Event e)
        {
            this.AssertCorrectCallerActor(sender as Actor, "Monitor");
            foreach (var m in this.Monitors)
            {
                if (m.GetType() == type)
                {
                    if (this.Configuration.ReportActivityCoverage)
                    {
                        this.ReportActivityCoverageOfMonitorEvent(sender, m, e);
                        this.ReportActivityCoverageOfMonitorTransition(m, e);
                    }

                    if (this.Configuration.EnableDataRaceDetection)
                    {
                        this.Reporter.InMonitor = (long)m.Id.Value;
                    }

                    m.MonitorEvent(e);
                    if (this.Configuration.EnableDataRaceDetection)
                    {
                        this.Reporter.InMonitor = -1;
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate)
        {
            if (!predicate)
            {
                this.Scheduler.NotifyAssertionFailure("Detected an assertion failure.");
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate, string s, object arg0)
        {
            if (!predicate)
            {
                this.Scheduler.NotifyAssertionFailure(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate, string s, object arg0, object arg1)
        {
            if (!predicate)
            {
                this.Scheduler.NotifyAssertionFailure(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString(), arg1.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate, string s, object arg0, object arg1, object arg2)
        {
            if (!predicate)
            {
                this.Scheduler.NotifyAssertionFailure(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString(), arg1.ToString(), arg2.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate, string s, params object[] args)
        {
            if (!predicate)
            {
                this.Scheduler.NotifyAssertionFailure(string.Format(CultureInfo.InvariantCulture, s, args));
            }
        }

        /// <summary>
        /// Asserts that a transition statement (raise, goto or pop) has not
        /// already been called. Records that RGP has been called.
        /// </summary>
        internal void AssertTransitionStatement(Actor actor)
        {
            var stateManager = actor.StateManager as SerializedActorStateManager;
            this.Assert(!stateManager.IsInsideOnExit,
                "Actor '{0}' has called raise, goto, push or pop inside an OnExit method.",
                actor.Id.Name);
            this.Assert(!stateManager.IsTransitionStatementCalledInCurrentAction,
                "Actor '{0}' has called multiple raise, goto, push or pop in the same action.",
                actor.Id.Name);
            stateManager.IsTransitionStatementCalledInCurrentAction = true;
        }

        /// <summary>
        /// Asserts that a transition statement (raise, goto or pop) has not already been called.
        /// </summary>
        private void AssertNoPendingTransitionStatement(Actor actor, string action)
        {
            if (!this.Configuration.EnableNoApiCallAfterTransitionStmtAssertion)
            {
                // The check is disabled.
                return;
            }

            var stateManager = actor.StateManager as SerializedActorStateManager;
            this.Assert(!stateManager.IsTransitionStatementCalledInCurrentAction,
                "Actor '{0}' cannot {1} after calling raise, goto, push or pop in the same action.",
                actor.Id.Name, action);
        }

        /// <summary>
        /// Asserts that the actor calling a Coyote actor method is also
        /// the actor that is currently executing.
        /// </summary>
        private void AssertCorrectCallerActor(Actor callerActor, string calledAPI)
        {
            if (callerActor is null)
            {
                return;
            }

            var executingActor = this.GetExecutingActor<Actor>();
            if (executingActor is null)
            {
                return;
            }

            this.Assert(executingActor.Equals(callerActor), "Actor '{0}' invoked {1} on behalf of actor '{2}'.",
                executingActor.Id, calledAPI, callerActor.Id);
        }

        /// <summary>
        /// Checks that no monitor is in a hot state upon program termination.
        /// If the program is still running, then this method returns without
        /// performing a check.
        /// </summary>
        internal void CheckNoMonitorInHotStateAtTermination()
        {
            if (!this.Scheduler.HasFullyExploredSchedule)
            {
                return;
            }

            foreach (var monitor in this.Monitors)
            {
                if (monitor.IsInHotState(out string stateName))
                {
                    string message = string.Format(CultureInfo.InvariantCulture,
                        "Monitor '{0}' detected liveness bug in hot state '{1}' at the end of program execution.",
                        monitor.GetType().FullName, stateName);
                    this.Scheduler.NotifyAssertionFailure(message, killTasks: false, cancelExecution: false);
                }
            }
        }

        /// <summary>
        /// Returns a nondeterministic boolean choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        internal override bool GetNondeterministicBooleanChoice(AsyncActor caller, int maxValue)
        {
            caller = caller ?? this.GetExecutingActor<Actor>();
            this.AssertCorrectCallerActor(caller as Actor, "Random");
            if (caller is Actor actor)
            {
                this.AssertNoPendingTransitionStatement(caller as Actor, "invoke 'Random'");
                (actor.StateManager as SerializedActorStateManager).ProgramCounter++;
            }

            var choice = this.Scheduler.GetNextNondeterministicBooleanChoice(maxValue);
            this.LogWriter.OnRandom(caller?.Id, choice);

            var stateName = caller is Actor ? (caller as Actor).CurrentStateName : string.Empty;
            this.BugTrace.AddRandomChoiceStep(caller?.Id, stateName, choice);

            return choice;
        }

        /// <summary>
        /// Returns a fair nondeterministic boolean choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        internal override bool GetFairNondeterministicBooleanChoice(AsyncActor caller, string uniqueId)
        {
            caller = caller ?? this.GetExecutingActor<Actor>();
            this.AssertCorrectCallerActor(caller as Actor, "FairRandom");
            if (caller is Actor actor)
            {
                this.AssertNoPendingTransitionStatement(caller as Actor, "invoke 'FairRandom'");
                (actor.StateManager as SerializedActorStateManager).ProgramCounter++;
            }

            var choice = this.Scheduler.GetNextNondeterministicBooleanChoice(2, uniqueId);
            this.LogWriter.OnRandom(caller?.Id, choice);

            var stateName = caller is Actor ? (caller as Actor).CurrentStateName : string.Empty;
            this.BugTrace.AddRandomChoiceStep(caller?.Id, stateName, choice);

            return choice;
        }

        /// <summary>
        /// Returns a nondeterministic integer, that can be
        /// controlled during analysis or testing.
        /// </summary>
        internal override int GetNondeterministicIntegerChoice(AsyncActor caller, int maxValue)
        {
            caller = caller ?? this.GetExecutingActor<Actor>();
            this.AssertCorrectCallerActor(caller as Actor, "RandomInteger");
            if (caller is Actor)
            {
                this.AssertNoPendingTransitionStatement(caller as Actor, "invoke 'RandomInteger'");
            }

            var choice = this.Scheduler.GetNextNondeterministicIntegerChoice(maxValue);
            this.LogWriter.OnRandom(caller?.Id, choice);

            var stateName = caller is Actor ? (caller as Actor).CurrentStateName : string.Empty;
            this.BugTrace.AddRandomChoiceStep(caller?.Id, stateName, choice);

            return choice;
        }

        /// <summary>
        /// Notifies that an actor entered a state.
        /// </summary>
        internal override void NotifyEnteredState(Actor actor)
        {
            string actorState = actor.CurrentStateName;
            this.BugTrace.AddGotoStateStep(actor.Id, actorState);

            this.LogWriter.OnActorState(actor.Id, actorState, isEntry: true);
        }

        /// <summary>
        /// Notifies that a monitor entered a state.
        /// </summary>
        internal override void NotifyEnteredState(Monitor monitor)
        {
            string monitorState = monitor.CurrentStateNameWithTemperature;
            this.BugTrace.AddGotoStateStep(monitor.Id, monitorState);

            this.LogWriter.OnMonitorState(monitor.GetType().FullName, monitor.Id, monitorState, true, monitor.GetHotState());
        }

        /// <summary>
        /// Notifies that an actor exited a state.
        /// </summary>
        internal override void NotifyExitedState(Actor actor)
        {
            this.LogWriter.OnActorState(actor.Id, actor.CurrentStateName, isEntry: false);
        }

        /// <summary>
        /// Notifies that a monitor exited a state.
        /// </summary>
        internal override void NotifyExitedState(Monitor monitor)
        {
            string monitorState = monitor.CurrentStateNameWithTemperature;
            this.LogWriter.OnMonitorState(monitor.GetType().FullName, monitor.Id, monitorState, false, monitor.GetHotState());
        }

        /// <summary>
        /// Notifies that an actor invoked an action.
        /// </summary>
        internal override void NotifyInvokedAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
            (actor.StateManager as SerializedActorStateManager).IsTransitionStatementCalledInCurrentAction = false;
            if (action.ReturnType == typeof(ActorTask))
            {
                (actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler = true;
            }

            string actorState = actor.CurrentStateName;
            this.BugTrace.AddInvokeActionStep(actor.Id, actorState, action);

            this.LogWriter.OnActorAction(actor.Id, actorState, action.Name);
            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.InAction[actor.Id.Value] = true;
            }
        }

        /// <summary>
        /// Notifies that an actor completed an action.
        /// </summary>
        internal override void NotifyCompletedAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
            (actor.StateManager as SerializedActorStateManager).IsTransitionStatementCalledInCurrentAction = false;
            (actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler = false;
            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.InAction[actor.Id.Value] = false;
            }
        }

        /// <summary>
        /// Notifies that an actor invoked an action.
        /// </summary>
        internal override void NotifyInvokedOnEntryAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
            (actor.StateManager as SerializedActorStateManager).IsTransitionStatementCalledInCurrentAction = false;
            if (action.ReturnType == typeof(ActorTask))
            {
                (actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler = true;
            }

            string actorState = actor.CurrentStateName;
            this.BugTrace.AddInvokeActionStep(actor.Id, actorState, action);

            this.LogWriter.OnActorAction(actor.Id, actorState, action.Name);
            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.InAction[actor.Id.Value] = true;
            }
        }

        /// <summary>
        /// Notifies that an actor completed invoking an action.
        /// </summary>
        internal override void NotifyCompletedOnEntryAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
            (actor.StateManager as SerializedActorStateManager).IsTransitionStatementCalledInCurrentAction = false;
            (actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler = false;
            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.InAction[actor.Id.Value] = false;
            }
        }

        /// <summary>
        /// Notifies that an actor invoked an action.
        /// </summary>
        internal override void NotifyInvokedOnExitAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
            (actor.StateManager as SerializedActorStateManager).IsInsideOnExit = true;
            (actor.StateManager as SerializedActorStateManager).IsTransitionStatementCalledInCurrentAction = false;
            if (action.ReturnType == typeof(ActorTask))
            {
                (actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler = true;
            }

            string actorState = actor.CurrentStateName;
            this.BugTrace.AddInvokeActionStep(actor.Id, actorState, action);

            this.LogWriter.OnActorAction(actor.Id, actorState, action.Name);
            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.InAction[actor.Id.Value] = true;
            }
        }

        /// <summary>
        /// Notifies that an actor completed invoking an action.
        /// </summary>
        internal override void NotifyCompletedOnExitAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
            (actor.StateManager as SerializedActorStateManager).IsInsideOnExit = false;
            (actor.StateManager as SerializedActorStateManager).IsTransitionStatementCalledInCurrentAction = false;
            (actor.StateManager as SerializedActorStateManager).IsInsideActorTaskHandler = false;
            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.InAction[actor.Id.Value] = false;
            }
        }

        /// <summary>
        /// Notifies that a monitor invoked an action.
        /// </summary>
        internal override void NotifyInvokedAction(Monitor monitor, MethodInfo action, Event receivedEvent)
        {
            string monitorState = monitor.CurrentStateName;
            this.BugTrace.AddInvokeActionStep(monitor.Id, monitorState, action);

            this.LogWriter.OnMonitorAction(monitor.GetType().FullName, monitor.Id, action.Name, monitorState);
        }

        /// <summary>
        /// Notifies that an actor raised an <see cref="Event"/>.
        /// </summary>
        internal override void NotifyRaisedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
            this.AssertTransitionStatement(actor);

            string actorState = actor.CurrentStateName;
            this.BugTrace.AddRaiseEventStep(actor.Id, actorState, eventInfo);

            this.LogWriter.OnActorEvent(actor.Id, actorState, eventInfo.EventName);
        }

        /// <summary>
        /// Notifies that a monitor raised an <see cref="Event"/>.
        /// </summary>
        internal override void NotifyRaisedEvent(Monitor monitor, Event e, EventInfo eventInfo)
        {
            string monitorState = monitor.CurrentStateName;
            this.BugTrace.AddRaiseEventStep(monitor.Id, monitorState, eventInfo);

            this.LogWriter.OnMonitorEvent(monitor.GetType().FullName, monitor.Id, monitor.CurrentStateName,
                eventInfo.EventName, isProcessing: false);
        }

        /// <summary>
        /// Notifies that an actor dequeued an <see cref="Event"/>.
        /// </summary>
        internal override void NotifyDequeuedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
            ActorOperation op = this.GetAsynchronousOperation(actor.Id.Value);

            // Skip `Receive` if the last operation exited the previous event handler,
            // to avoid scheduling duplicate `Receive` operations.
            if (op.SkipNextReceiveSchedulingPoint)
            {
                op.SkipNextReceiveSchedulingPoint = false;
            }
            else
            {
                op.MatchingSendIndex = (ulong)eventInfo.SendStep;
                this.Scheduler.ScheduleNextOperation(AsyncOperationType.Receive, AsyncOperationTarget.Inbox, actor.Id.Value);
                ResetProgramCounter(actor);
            }

            this.LogWriter.OnDequeue(actor.Id, actor.CurrentStateName, eventInfo.EventName);

            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.RegisterDequeue(eventInfo.OriginInfo?.SenderActorId, actor.Id, e, (ulong)eventInfo.SendStep);
            }

            this.BugTrace.AddDequeueEventStep(actor.Id, actor.CurrentStateName, eventInfo);

            if (this.Configuration.ReportActivityCoverage)
            {
                this.ReportActivityCoverageOfReceivedEvent(actor, eventInfo);
                this.ReportActivityCoverageOfStateTransition(actor, e);
            }
        }

        /// <summary>
        /// Notifies that an actor invoked pop.
        /// </summary>
        internal override void NotifyPop(Actor actor)
        {
            this.AssertCorrectCallerActor(actor, "Pop");
            this.AssertTransitionStatement(actor);

            this.LogWriter.OnPop(actor.Id, string.Empty, actor.CurrentStateName);

            if (this.Configuration.ReportActivityCoverage)
            {
                this.ReportActivityCoverageOfPopTransition(actor, actor.CurrentState, actor.GetStateTypeAtStackIndex(1));
            }
        }

        /// <summary>
        /// Notifies that an actor called Receive.
        /// </summary>
        internal override void NotifyReceiveCalled(Actor actor)
        {
            this.AssertCorrectCallerActor(actor, "Receive");
            this.AssertNoPendingTransitionStatement(actor, "invoke 'Receive'");
        }

        /// <summary>
        /// Notifies that an actor is handling a raised event.
        /// </summary>
        internal override void NotifyHandleRaisedEvent(Actor actor, Event e)
        {
            if (this.Configuration.ReportActivityCoverage)
            {
                this.ReportActivityCoverageOfStateTransition(actor, e);
            }
        }

        /// <summary>
        /// Notifies that an actor is waiting for the specified task to complete.
        /// </summary>
        internal override void NotifyWaitTask(AsyncActor actor, Task task)
        {
            this.Assert(task != null, "Cannot wait for a null task to complete.");
            ActorOperation callerOp = this.GetAsynchronousOperation(actor.Id.Value);
            if (callerOp == null)
            {
                return;
            }

            if (!task.IsCompleted)
            {
                callerOp.OnWaitTask(task);
            }

            if (callerOp.Status == AsyncOperationStatus.BlockedOnWaitAll)
            {
                // Only schedule if the task is not already completed.
                this.Scheduler.ScheduleNextOperation(AsyncOperationType.Join, AsyncOperationTarget.Task, actor.Id.Value);
            }
        }

        /// <summary>
        /// Notifies that an actor is waiting to receive an event of one of the specified types.
        /// </summary>
        internal override void NotifyWaitEvent(Actor actor, IEnumerable<Type> eventTypes)
        {
            ActorOperation op = this.GetAsynchronousOperation(actor.Id.Value);
            op.OnWaitEvent(eventTypes);

            string eventNames;
            var eventWaitTypesArray = eventTypes.ToArray();
            if (eventWaitTypesArray.Length == 1)
            {
                this.LogWriter.OnWait(actor.Id, actor.CurrentStateName, eventWaitTypesArray[0]);
                eventNames = eventWaitTypesArray[0].FullName;
            }
            else
            {
                this.LogWriter.OnWait(actor.Id, actor.CurrentStateName, eventWaitTypesArray);
                if (eventWaitTypesArray.Length > 0)
                {
                    string[] eventNameArray = new string[eventWaitTypesArray.Length - 1];
                    for (int i = 0; i < eventWaitTypesArray.Length - 2; i++)
                    {
                        eventNameArray[i] = eventWaitTypesArray[i].FullName;
                    }

                    eventNames = string.Join(", ", eventNameArray) + " or " + eventWaitTypesArray[eventWaitTypesArray.Length - 1].FullName;
                }
                else
                {
                    eventNames = string.Empty;
                }
            }

            this.BugTrace.AddWaitToReceiveStep(actor.Id, actor.CurrentStateName, eventNames);
            this.Scheduler.ScheduleNextOperation(AsyncOperationType.Receive, AsyncOperationTarget.Inbox, actor.Id.Value);
            ResetProgramCounter(actor);
        }

        /// <summary>
        /// Notifies that an actor enqueued an event that it was waiting to receive.
        /// </summary>
        internal override void NotifyReceivedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
            this.LogWriter.OnReceive(actor.Id, actor.CurrentStateName, e.GetType().FullName, wasBlocked: true);
            this.BugTrace.AddReceivedEventStep(actor.Id, actor.CurrentStateName, eventInfo);

            // A subsequent enqueue unblocked the receive action of actor.
            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.RegisterDequeue(eventInfo.OriginInfo?.SenderActorId, actor.Id, e, (ulong)eventInfo.SendStep);
            }

            ActorOperation op = this.GetAsynchronousOperation(actor.Id.Value);
            op.OnReceivedEvent((ulong)eventInfo.SendStep);

            if (this.Configuration.ReportActivityCoverage)
            {
                this.ReportActivityCoverageOfReceivedEvent(actor, eventInfo);
            }
        }

        /// <summary>
        /// Notifies that an actor received an event without waiting because the event
        /// was already in the inbox when the actor invoked the receive statement.
        /// </summary>
        internal override void NotifyReceivedEventWithoutWaiting(Actor actor, Event e, EventInfo eventInfo)
        {
            this.LogWriter.OnReceive(actor.Id, actor.CurrentStateName, e.GetType().FullName, wasBlocked: false);

            ActorOperation op = this.GetAsynchronousOperation(actor.Id.Value);
            op.MatchingSendIndex = (ulong)eventInfo.SendStep;

            if (this.Configuration.EnableDataRaceDetection)
            {
                this.Reporter.RegisterDequeue(eventInfo.OriginInfo?.SenderActorId, actor.Id, e, (ulong)eventInfo.SendStep);
            }

            this.Scheduler.ScheduleNextOperation(AsyncOperationType.Receive, AsyncOperationTarget.Inbox, actor.Id.Value);
            ResetProgramCounter(actor);
        }

        /// <summary>
        /// Notifies that an actor has halted.
        /// </summary>
        internal override void NotifyHalted(Actor actor)
        {
            this.BugTrace.AddHaltStep(actor.Id, null);
        }

        /// <summary>
        /// Notifies that the inbox of the specified actor is about to be
        /// checked to see if the default event handler should fire.
        /// </summary>
        internal override void NotifyDefaultEventHandlerCheck(Actor actor)
        {
            this.Scheduler.ScheduleNextOperation(AsyncOperationType.Send, AsyncOperationTarget.Inbox, actor.Id.Value);

            // If the default event handler fires, the next receive in NotifyDefaultHandlerFired
            // will use this as its MatchingSendIndex.
            // If it does not fire, MatchingSendIndex will be overwritten.
            this.GetAsynchronousOperation(actor.Id.Value).MatchingSendIndex = (ulong)this.Scheduler.ScheduledSteps;
        }

        /// <summary>
        /// Notifies that the default handler of the specified actor has been fired.
        /// </summary>
        internal override void NotifyDefaultHandlerFired(Actor actor)
        {
            // MatchingSendIndex is set in NotifyDefaultEventHandlerCheck.
            this.Scheduler.ScheduleNextOperation(AsyncOperationType.Receive, AsyncOperationTarget.Inbox, actor.Id.Value);
            ResetProgramCounter(actor);
        }

        /// <summary>
        /// Reports coverage for the specified received event.
        /// </summary>
        private void ReportActivityCoverageOfReceivedEvent(Actor actor, EventInfo eventInfo)
        {
            string originActor = eventInfo.OriginInfo.SenderActorName;
            string originState = eventInfo.OriginInfo.SenderStateName;
            string edgeLabel = eventInfo.EventName;
            string destActor = actor.GetType().FullName;
            string destState = NameResolver.GetStateNameForLogging(actor.CurrentState);

            this.CoverageInfo.AddTransition(originActor, originState, edgeLabel, destActor, destState);
        }

        /// <summary>
        /// Reports coverage for the specified monitor event.
        /// </summary>
        private void ReportActivityCoverageOfMonitorEvent(AsyncActor sender, Monitor monitor, Event e)
        {
            string originActor = sender is null ? "Env" : sender.GetType().FullName;
            string originState = sender is null ? "Env" :
                (sender is Actor) ? NameResolver.GetStateNameForLogging((sender as Actor).CurrentState) : "Env";

            string edgeLabel = e.GetType().FullName;
            string destActor = monitor.GetType().FullName;
            string destState = NameResolver.GetStateNameForLogging(monitor.CurrentState);

            this.CoverageInfo.AddTransition(originActor, originState, edgeLabel, destActor, destState);
        }

        /// <summary>
        /// Reports coverage for the specified actor.
        /// </summary>
        private void ReportActivityCoverageOfActor(Actor actor)
        {
            var actorName = actor.GetType().FullName;
            if (this.CoverageInfo.IsActorDeclared(actorName))
            {
                return;
            }

            // Fetch states.
            var states = actor.GetAllStates();
            foreach (var state in states)
            {
                this.CoverageInfo.DeclareActorState(actorName, state);
            }

            // Fetch registered events.
            var pairs = actor.GetAllStateEventPairs();
            foreach (var tup in pairs)
            {
                this.CoverageInfo.DeclareStateEvent(actorName, tup.Item1, tup.Item2);
            }
        }

        /// <summary>
        /// Reports coverage for the specified monitor.
        /// </summary>
        private void ReportActivityCoverageOfMonitor(Monitor monitor)
        {
            var monitorName = monitor.GetType().FullName;

            // Fetch states.
            var states = monitor.GetAllStates();

            foreach (var state in states)
            {
                this.CoverageInfo.DeclareActorState(monitorName, state);
            }

            // Fetch registered events.
            var pairs = monitor.GetAllStateEventPairs();

            foreach (var tup in pairs)
            {
                this.CoverageInfo.DeclareStateEvent(monitorName, tup.Item1, tup.Item2);
            }
        }

        /// <summary>
        /// Reports coverage for the specified state transition.
        /// </summary>
        private void ReportActivityCoverageOfStateTransition(Actor actor, Event e)
        {
            string originActor = actor.GetType().FullName;
            string originState = NameResolver.GetStateNameForLogging(actor.CurrentState);
            string destActor = actor.GetType().FullName;

            string edgeLabel;
            string destState;
            if (e is GotoStateEvent gotoStateEvent)
            {
                edgeLabel = "goto";
                destState = NameResolver.GetStateNameForLogging(gotoStateEvent.State);
            }
            else if (e is PushStateEvent pushStateEvent)
            {
                edgeLabel = "push";
                destState = NameResolver.GetStateNameForLogging(pushStateEvent.State);
            }
            else if (actor.GotoTransitions.ContainsKey(e.GetType()))
            {
                edgeLabel = e.GetType().FullName;
                destState = NameResolver.GetStateNameForLogging(
                    actor.GotoTransitions[e.GetType()].TargetState);
            }
            else if (actor.PushTransitions.ContainsKey(e.GetType()))
            {
                edgeLabel = e.GetType().FullName;
                destState = NameResolver.GetStateNameForLogging(
                    actor.PushTransitions[e.GetType()].TargetState);
            }
            else
            {
                return;
            }

            this.CoverageInfo.AddTransition(originActor, originState, edgeLabel, destActor, destState);
        }

        /// <summary>
        /// Reports coverage for a pop transition.
        /// </summary>
        private void ReportActivityCoverageOfPopTransition(Actor actor, Type fromState, Type toState)
        {
            string originActor = actor.GetType().FullName;
            string originState = NameResolver.GetStateNameForLogging(fromState);
            string destActor = actor.GetType().FullName;
            string edgeLabel = "pop";
            string destState = NameResolver.GetStateNameForLogging(toState);

            this.CoverageInfo.AddTransition(originActor, originState, edgeLabel, destActor, destState);
        }

        /// <summary>
        /// Reports coverage for the specified state transition.
        /// </summary>
        private void ReportActivityCoverageOfMonitorTransition(Monitor monitor, Event e)
        {
            string originActor = monitor.GetType().FullName;
            string originState = NameResolver.GetStateNameForLogging(monitor.CurrentState);
            string destActor = originActor;

            string edgeLabel;
            string destState;
            if (e is GotoStateEvent)
            {
                edgeLabel = "goto";
                destState = NameResolver.GetStateNameForLogging((e as GotoStateEvent).State);
            }
            else if (monitor.GotoTransitions.ContainsKey(e.GetType()))
            {
                edgeLabel = e.GetType().FullName;
                destState = NameResolver.GetStateNameForLogging(
                    monitor.GotoTransitions[e.GetType()].TargetState);
            }
            else
            {
                return;
            }

            this.CoverageInfo.AddTransition(originActor, originState, edgeLabel, destActor, destState);
        }

        /// <summary>
        /// Resets the program counter of the specified actor.
        /// </summary>
        private static void ResetProgramCounter(Actor actor)
        {
            if (actor != null)
            {
                (actor.StateManager as SerializedActorStateManager).ProgramCounter = 0;
            }
        }

        /// <summary>
        /// Gets the currently executing actor of type <typeparamref name="TActor"/>,
        /// or null if no such actor is currently executing.
        /// </summary>
        internal TActor GetExecutingActor<TActor>()
            where TActor : AsyncActor
        {
            if (Task.CurrentId.HasValue &&
                this.Scheduler.ControlledTaskMap.TryGetValue(Task.CurrentId.Value, out ActorOperation op) &&
                op?.Actor is TActor actor)
            {
                return actor;
            }

            return null;
        }

        /// <summary>
        /// Gets the id of the currently executing actor.
        /// </summary>
        internal ActorId GetCurrentActorId() => this.GetExecutingActor<AsyncActor>()?.Id;

        /// <summary>
        /// Gets the asynchronous operation associated with the specified id.
        /// </summary>
        internal ActorOperation GetAsynchronousOperation(ulong id)
        {
            if (!this.IsRunning)
            {
                throw new ExecutionCanceledException();
            }

            this.ActorOperations.TryGetValue(id, out ActorOperation op);
            return op;
        }

        /// <summary>
        /// Returns the current hashed state of the execution using the specified
        /// level of abstraction. The hash is updated in each execution step.
        /// </summary>
        internal int GetHashedExecutionState(AbstractionLevel abstractionLevel)
        {
            unchecked
            {
                int hash = 14689;

                if (abstractionLevel is AbstractionLevel.Default ||
                    abstractionLevel is AbstractionLevel.Full)
                {
                    foreach (var actor in this.ActorMap.Values)
                    {
                        int actorHash = 37;
                        actorHash = (actorHash * 397) + actor.GetHashedState(abstractionLevel);
                        actorHash = (actorHash * 397) + this.GetAsynchronousOperation(actor.Id.Value).Type.GetHashCode();
                        hash *= actorHash;
                    }

                    foreach (var monitor in this.Monitors)
                    {
                        hash = (hash * 397) + monitor.GetHashedState(abstractionLevel);
                    }
                }
                else if (abstractionLevel is AbstractionLevel.InboxOnly ||
                    abstractionLevel is AbstractionLevel.Custom)
                {
                    foreach (var actor in this.ActorMap.Values)
                    {
                        int actorHash = 37;
                        actorHash = (actorHash * 397) + actor.GetHashedState(abstractionLevel);
                        hash *= actorHash;
                    }

                    foreach (var monitor in this.Monitors)
                    {
                        hash = (hash * 397) + monitor.GetHashedState(abstractionLevel);
                    }
                }

                return hash;
            }
        }

        /// <summary>
        /// Throws an <see cref="AssertionFailureException"/> exception
        /// containing the specified exception.
        /// </summary>
        internal override void WrapAndThrowException(Exception exception, string s, params object[] args)
        {
            string message = string.Format(CultureInfo.InvariantCulture, s, args);
            this.Scheduler.NotifyAssertionFailure(message);
        }

        /// <summary>
        /// Disposes runtime resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Monitors.Clear();
                this.ActorMap.Clear();
                this.ActorOperations.Clear();
            }

            base.Dispose(disposing);
        }
    }
}
