// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Coyote.Runtime;
using Microsoft.Coyote.TestingServices.Timers;
using Microsoft.Coyote.Threading;
using Microsoft.Coyote.Timers;

namespace Microsoft.Coyote.TestingServices.Runtime
{
    /// <summary>
    /// Runtime for testing an actor in isolation.
    /// </summary>
    internal sealed class ActorTestingRuntime : ActorRuntime
    {
        /// <summary>
        /// The actor being tested.
        /// </summary>
        internal readonly Actor Actor;

        /// <summary>
        /// The inbox of the actor being tested.
        /// </summary>
        internal readonly EventQueue ActorInbox;

        /// <summary>
        /// Task completion source that completes when the actor being tested reaches quiescence.
        /// </summary>
        private TaskCompletionSource<bool> QuiescenceCompletionSource;

        /// <summary>
        /// True if the actor is waiting to receive and event, else false.
        /// </summary>
        internal bool IsActorWaitingToReceiveEvent { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorTestingRuntime"/> class.
        /// </summary>
        internal ActorTestingRuntime(Type actorType, Configuration configuration)
            : base(configuration)
        {
            if (!actorType.IsSubclassOf(typeof(Actor)))
            {
                this.Assert(false, "Type '{0}' is not an actor.", actorType.FullName);
            }

            var mid = new ActorId(actorType, null, this);

            this.Actor = ActorFactory.Create(actorType);
            IActorStateManager stateManager = new ActorStateManager(this, this.Actor, Guid.Empty);
            this.ActorInbox = new EventQueue(stateManager);

            this.Actor.Initialize(this, mid, stateManager, this.ActorInbox);
            this.Actor.InitializeStateInformation();

            this.LogWriter.OnCreateActor(this.Actor.Id, null);

            this.ActorMap.TryAdd(mid, this.Actor);

            this.IsActorWaitingToReceiveEvent = false;
        }

        /// <summary>
        /// Starts executing the actor-under-test by transitioning it to its initial state
        /// and passing an optional initialization event.
        /// </summary>
        internal Task StartAsync(Event initialEvent)
        {
            this.RunActorEventHandler(this.Actor, initialEvent, true);
            return this.QuiescenceCompletionSource.Task;
        }

        /// <summary>
        /// Creates an actor id that is uniquely tied to the specified unique name. The
        /// returned actor id can either be a fresh id (not yet bound to any actor),
        /// or it can be bound to a previously created actor. In the second case, this
        /// actor id can be directly used to communicate with the corresponding actor.
        /// </summary>
        public override ActorId CreateActorIdFromName(Type type, string actorName) => new ActorId(type, actorName, this, true);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with
        /// the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        public override ActorId CreateActor(Type type, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and
        /// with the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        public override ActorId CreateActor(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new actor of the specified type, using the specified <see cref="ActorId"/>.
        /// This method optionally passes an <see cref="Event"/> to the new actor, which can only
        /// be used to access its payload, and cannot be handled.
        /// </summary>
        public override ActorId CreateActor(ActorId mid, Type type, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with the
        /// specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when
        /// the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(Type type, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and with
        /// the specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when the
        /// actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/>, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This
        /// event can only be used to access its payload, and cannot be handled. The method
        /// returns only when the actor is initialized and the <see cref="Event"/> (if any)
        /// is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with the
        /// specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when
        /// the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(Type type, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and with
        /// the specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when the
        /// actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/>, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This
        /// event can only be used to access its payload, and cannot be handled. The method
        /// returns only when the actor is initialized and the <see cref="Event"/> (if any)
        /// is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(ActorId mid, Type type, Event e = null, Guid opGroupId = default) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor.
        /// </summary>
        public override void SendEvent(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Sends an <see cref="Event"/> to an actor. Returns immediately if the target actor was already
        /// running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        public override Task<bool> SendEventAndExecuteAsync(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Sends an <see cref="Event"/> to an actor. Returns immediately if the target actor was already
        /// running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        public override Task<bool> SendEventAndExecute(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Returns the operation group id of the specified actor. Returns <see cref="Guid.Empty"/>
        /// if the id is not set, or if the <see cref="ActorId"/> is not associated with this runtime.
        /// During testing, the runtime asserts that the specified actor is currently executing.
        /// </summary>
        public override Guid GetCurrentOperationGroupId(ActorId currentActor) => Guid.Empty;

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>.
        /// </summary>
        internal override ActorId CreateActor(ActorId mid, Type type, string actorName, Event e,
            Actor creator, Guid opGroupId)
        {
            mid = mid ?? new ActorId(type, null, this);
            this.LogWriter.OnCreateActor(mid, creator?.Id);
            return mid;
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>. The
        /// method returns only when the created actor reaches quiescence.
        /// </summary>
        internal override Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, string actorName, Event e,
            Actor creator, Guid opGroupId)
        {
            mid = mid ?? new ActorId(type, null, this);
            this.LogWriter.OnCreateActor(mid, creator?.Id);
            return Task.FromResult(mid);
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor.
        /// </summary>
        internal override void SendEvent(ActorId target, Event e, AsyncActor sender, Guid opGroupId, SendOptions options)
        {
            this.Assert(sender is null || this.Actor.Id.Equals(sender.Id),
                string.Format("Only actor '{0}' can send an event during this test.", this.Actor.Id.ToString()));
            this.Assert(target != null, string.Format("Actor '{0}' is sending to a null actor.", this.Actor.Id.ToString()));
            this.Assert(e != null, string.Format("Actor '{0}' is sending a null event.", this.Actor.Id.ToString()));

            // The operation group id of this operation is set using the following precedence:
            // (1) To the specified send operation group id, if it is non-empty.
            // (2) To the operation group id of the sender actor, if it exists and is non-empty.
            // (3) To the empty operation group id.
            if (opGroupId == Guid.Empty && sender != null)
            {
                opGroupId = sender.OperationGroupId;
            }

            if (this.Actor.IsHalted)
            {
                this.LogWriter.OnSend(target, sender?.Id, (sender as Actor)?.CurrentStateName ?? string.Empty,
                    e.GetType().FullName, opGroupId, isTargetHalted: true);
                return;
            }

            this.LogWriter.OnSend(target, sender?.Id, (sender as Actor)?.CurrentStateName ?? string.Empty,
                e.GetType().FullName, opGroupId, isTargetHalted: false);

            if (!target.Equals(this.Actor.Id))
            {
                // Drop all events sent to an actor other than the actor-under-test.
                return;
            }

            EnqueueStatus enqueueStatus = this.Actor.Enqueue(e, opGroupId, null);
            if (enqueueStatus == EnqueueStatus.EventHandlerNotRunning)
            {
                this.RunActorEventHandler(this.Actor, null, false);
            }
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor. Returns immediately if the target actor was
        /// already running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        internal override Task<bool> SendEventAndExecuteAsync(ActorId target, Event e, AsyncActor sender,
            Guid opGroupId, SendOptions options)
        {
            this.SendEvent(target, e, sender, opGroupId, options);
            return this.QuiescenceCompletionSource.Task;
        }

        /// <summary>
        /// Runs a new asynchronous actor event handler.
        /// </summary>
        private Task RunActorEventHandler(Actor actor, Event initialEvent, bool isFresh)
        {
            this.QuiescenceCompletionSource = new TaskCompletionSource<bool>();

            return Task.Run(async () =>
            {
                try
                {
                    if (isFresh)
                    {
                        await actor.GotoStartState(initialEvent);
                    }

                    await actor.RunEventHandlerAsync();
                    this.QuiescenceCompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    this.IsRunning = false;
                    this.RaiseOnFailureEvent(ex);
                    this.QuiescenceCompletionSource.SetException(ex);
                }
            });
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask CreateActorTask(Action action, CancellationToken cancellationToken) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask CreateActorTask(Func<Task> function, CancellationToken cancellationToken) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new <see cref="ActorTask{TResult}"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask<TResult> CreateActorTask<TResult>(Func<TResult> function,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new <see cref="ActorTask{TResult}"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask<TResult> CreateActorTask<TResult>(Func<Task<TResult>> function,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous delay.
        /// </summary>
        internal override ActorTask CreateActorTask(int millisecondsDelay, CancellationToken cancellationToken) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to complete with the specified task.
        /// </summary>
        internal override ActorTask CreateCompletionActorTask(Task task) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to complete with the specified task.
        /// </summary>
        internal override ActorTask<TResult> CreateCompletionActorTask<TResult>(Task<TResult> task) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Asynchronously waits for the specified tasks to complete.
        /// </summary>
        internal override ActorTask WaitAllTasksAsync(IEnumerable<Task> tasks) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Asynchronously waits for all specified tasks to complete.
        /// </summary>
        internal override ActorTask<TResult[]> WaitAllTasksAsync<TResult>(IEnumerable<Task<TResult>> tasks) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Asynchronously waits for any of the specified tasks to complete.
        /// </summary>
        internal override ActorTask<Task> WaitAnyTaskAsync(IEnumerable<Task> tasks) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

        /// <summary>
        /// Asynchronously waits for any of the specified tasks to complete.
        /// </summary>
        internal override ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(IEnumerable<Task<TResult>> tasks) =>
            throw new NotSupportedException($"Invoking this method is not supported in actor unit testing mode.");

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
        /// Tries to create a new <see cref="Coyote.Monitor"/> of the specified <see cref="Type"/>.
        /// </summary>
        internal override void TryCreateMonitor(Type type)
        {
            // No-op in this runtime mode.
        }

        /// <summary>
        /// Invokes the specified <see cref="Coyote.Monitor"/> with the specified <see cref="Event"/>.
        /// </summary>
        internal override void Monitor(Type type, AsyncActor sender, Event e)
        {
            // No-op in this runtime mode.
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate)
        {
            if (!predicate)
            {
                throw new AssertionFailureException("Detected an assertion failure.");
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate, string s, object arg0)
        {
            if (!predicate)
            {
                throw new AssertionFailureException(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate, string s, object arg0, object arg1)
        {
            if (!predicate)
            {
                throw new AssertionFailureException(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString(), arg1.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate, string s, object arg0, object arg1, object arg2)
        {
            if (!predicate)
            {
                throw new AssertionFailureException(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString(), arg1.ToString(), arg2.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public override void Assert(bool predicate, string s, params object[] args)
        {
            if (!predicate)
            {
                throw new AssertionFailureException(string.Format(CultureInfo.InvariantCulture, s, args));
            }
        }

        /// <summary>
        /// Returns a nondeterministic boolean choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        internal override bool GetNondeterministicBooleanChoice(AsyncActor actor, int maxValue)
        {
            Random random = new Random(DateTime.Now.Millisecond);

            bool result = false;
            if (random.Next(maxValue) == 0)
            {
                result = true;
            }

            this.LogWriter.OnRandom(actor?.Id, result);

            return result;
        }

        /// <summary>
        /// Returns a fair nondeterministic boolean choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        internal override bool GetFairNondeterministicBooleanChoice(AsyncActor actor, string uniqueId)
        {
            return this.GetNondeterministicBooleanChoice(actor, 2);
        }

        /// <summary>
        /// Returns a nondeterministic integer choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        internal override int GetNondeterministicIntegerChoice(AsyncActor actor, int maxValue)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            var result = random.Next(maxValue);

            this.LogWriter.OnRandom(actor?.Id, result);

            return result;
        }

        /// <summary>
        /// Notifies that an actor entered a state.
        /// </summary>
        internal override void NotifyEnteredState(Actor actor)
        {
            if (this.Configuration.IsVerbose)
            {
                this.LogWriter.OnActorState(actor.Id, actor.CurrentStateName, isEntry: true);
            }
        }

        /// <summary>
        /// Notifies that a monitor entered a state.
        /// </summary>
        internal override void NotifyEnteredState(Monitor monitor)
        {
            if (this.Configuration.IsVerbose)
            {
                string monitorState = monitor.CurrentStateNameWithTemperature;
                this.LogWriter.OnMonitorState(monitor.GetType().FullName, monitor.Id, monitorState, true, monitor.GetHotState());
            }
        }

        /// <summary>
        /// Notifies that an actor exited a state.
        /// </summary>
        internal override void NotifyExitedState(Actor actor)
        {
            if (this.Configuration.IsVerbose)
            {
                this.LogWriter.OnActorState(actor.Id, actor.CurrentStateName, isEntry: false);
            }
        }

        /// <summary>
        /// Notifies that a monitor exited a state.
        /// </summary>
        internal override void NotifyExitedState(Monitor monitor)
        {
            if (this.Configuration.IsVerbose)
            {
                string monitorState = monitor.CurrentStateNameWithTemperature;
                this.LogWriter.OnMonitorState(monitor.GetType().FullName, monitor.Id, monitorState, false, monitor.GetHotState());
            }
        }

        /// <summary>
        /// Notifies that an actor invoked an action.
        /// </summary>
        internal override void NotifyInvokedAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
            if (this.Configuration.IsVerbose)
            {
                this.LogWriter.OnActorAction(actor.Id, actor.CurrentStateName, action.Name);
            }
        }

        /// <summary>
        /// Notifies that a monitor invoked an action.
        /// </summary>
        internal override void NotifyInvokedAction(Monitor monitor, MethodInfo action, Event receivedEvent)
        {
            if (this.Configuration.IsVerbose)
            {
                this.LogWriter.OnMonitorAction(monitor.GetType().FullName, monitor.Id, action.Name, monitor.CurrentStateName);
            }
        }

        /// <summary>
        /// Notifies that an actor raised an <see cref="Event"/>.
        /// </summary>
        internal override void NotifyRaisedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
            if (this.Configuration.IsVerbose)
            {
                this.LogWriter.OnActorEvent(actor.Id, actor.CurrentStateName, e.GetType().FullName);
            }
        }

        /// <summary>
        /// Notifies that a monitor raised an <see cref="Event"/>.
        /// </summary>
        internal override void NotifyRaisedEvent(Monitor monitor, Event e, EventInfo eventInfo)
        {
            if (this.Configuration.IsVerbose)
            {
                this.LogWriter.OnMonitorEvent(monitor.GetType().FullName, monitor.Id, monitor.CurrentStateName,
                    e.GetType().FullName, isProcessing: false);
            }
        }

        /// <summary>
        /// Notifies that an actor dequeued an <see cref="Event"/>.
        /// </summary>
        internal override void NotifyDequeuedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
            if (this.Configuration.IsVerbose)
            {
                this.LogWriter.OnDequeue(actor.Id, actor.CurrentStateName, e.GetType().FullName);
            }
        }

        /// <summary>
        /// Notifies that an actor is waiting to receive an event of one of the specified types.
        /// </summary>
        internal override void NotifyWaitEvent(Actor actor, IEnumerable<Type> eventTypes)
        {
            if (this.Configuration.IsVerbose)
            {
                var eventWaitTypesArray = eventTypes.ToArray();
                if (eventWaitTypesArray.Length == 1)
                {
                    this.LogWriter.OnWait(this.Actor.Id, this.Actor.CurrentStateName, eventWaitTypesArray[0]);
                }
                else
                {
                    this.LogWriter.OnWait(this.Actor.Id, this.Actor.CurrentStateName, eventWaitTypesArray);
                }
            }

            this.IsActorWaitingToReceiveEvent = true;
            this.QuiescenceCompletionSource.SetResult(true);
        }

        /// <summary>
        /// Notifies that an actor received an event that it was waiting for.
        /// </summary>
        internal override void NotifyReceivedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
            this.LogWriter.OnReceive(actor.Id, actor.CurrentStateName, e.GetType().FullName, wasBlocked: true);
            this.IsActorWaitingToReceiveEvent = false;
            this.QuiescenceCompletionSource = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Notifies that an actor received an event without waiting because the event
        /// was already in the inbox when the actor invoked the receive statement.
        /// </summary>
        internal override void NotifyReceivedEventWithoutWaiting(Actor actor, Event e, EventInfo eventInfo)
        {
            this.LogWriter.OnReceive(actor.Id, actor.CurrentStateName, e.GetType().FullName, wasBlocked: false);
        }

        /// <summary>
        /// Notifies that an actor has halted.
        /// </summary>
        internal override void NotifyHalted(Actor actor)
        {
            this.ActorMap.TryRemove(actor.Id, out AsyncActor _);
        }

        /// <summary>
        /// Disposes runtime resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ActorMap.Clear();
            }

            base.Dispose(disposing);
        }
    }
}
