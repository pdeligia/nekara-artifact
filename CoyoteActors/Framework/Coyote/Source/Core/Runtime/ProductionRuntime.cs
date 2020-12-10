// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CoyoteActors.Threading;
using Microsoft.CoyoteActors.Timers;

namespace Microsoft.CoyoteActors.Runtime
{
    /// <summary>
    /// Runtime for executing actors in production.
    /// </summary>
    internal sealed class ProductionRuntime : ActorRuntime
    {
        /// <summary>
        /// List of monitors in the program.
        /// </summary>
        private readonly List<Monitor> Monitors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductionRuntime"/> class.
        /// </summary>
        internal ProductionRuntime()
            : this(Configuration.Create())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductionRuntime"/> class.
        /// </summary>
        internal ProductionRuntime(Configuration configuration)
            : base(configuration)
        {
            this.Monitors = new List<Monitor>();
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
            this.CreateActor(null, type, null, e, null, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and
        /// with the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        public override ActorId CreateActor(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            this.CreateActor(null, type, actorName, e, null, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified type, using the specified <see cref="ActorId"/>.
        /// This method optionally passes an <see cref="Event"/> to the new actor, which can only
        /// be used to access its payload, and cannot be handled.
        /// </summary>
        public override ActorId CreateActor(ActorId mid, Type type, Event e = null, Guid opGroupId = default) =>
            this.CreateActor(mid, type, null, e, null, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with the
        /// specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when
        /// the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(Type type, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(null, type, null, e, null, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and with
        /// the specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when the
        /// actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(null, type, actorName, e, null, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/>, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This
        /// event can only be used to access its payload, and cannot be handled. The method
        /// returns only when the actor is initialized and the <see cref="Event"/> (if any)
        /// is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(mid, type, null, e, null, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with the
        /// specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when
        /// the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(Type type, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(null, type, null, e, null, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and with
        /// the specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when the
        /// actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(Type type, string actorName, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(null, type, actorName, e, null, opGroupId);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/>, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This
        /// event can only be used to access its payload, and cannot be handled. The method
        /// returns only when the actor is initialized and the <see cref="Event"/> (if any)
        /// is handled.
        /// </summary>
        public override Task<ActorId> CreateActorAndExecute(ActorId mid, Type type, Event e = null, Guid opGroupId = default) =>
            this.CreateActorAndExecuteAsync(mid, type, null, e, null, opGroupId);

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor.
        /// </summary>
        public override void SendEvent(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null) =>
            this.SendEvent(target, e, null, opGroupId, options);

        /// <summary>
        /// Sends an <see cref="Event"/> to an actor. Returns immediately if the target actor was already
        /// running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        public override Task<bool> SendEventAndExecuteAsync(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null) =>
            this.SendEventAndExecuteAsync(target, e, null, opGroupId, options);

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
            Actor actor = this.GetActorFromId<Actor>(currentActor);
            return actor is null ? Guid.Empty : actor.OperationGroupId;
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>.
        /// </summary>
        internal override ActorId CreateActor(ActorId mid, Type type, string actorName, Event e,
            Actor creator, Guid opGroupId)
        {
            Actor actor = this.CreateActor(mid, type, actorName, creator, opGroupId);
            this.LogWriter.OnCreateActor(actor.Id, creator?.Id);
            this.RunActorEventHandler(actor, e, true);
            return actor.Id;
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>. The
        /// method returns only when the created actor reaches quiescence.
        /// </summary>
        internal override async Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, string actorName, Event e,
            Actor creator, Guid opGroupId)
        {
            Actor actor = this.CreateActor(mid, type, actorName, creator, opGroupId);
            this.LogWriter.OnCreateActor(actor.Id, creator?.Id);
            await this.RunActorEventHandlerAsync(actor, e, true);
            return actor.Id;
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>.
        /// </summary>
        private Actor CreateActor(ActorId mid, Type type, string actorName, Actor creator, Guid opGroupId)
        {
            if (!type.IsSubclassOf(typeof(Actor)))
            {
                this.Assert(false, "Type '{0}' is not an actor.", type.FullName);
            }

            if (mid is null)
            {
                mid = new ActorId(type, actorName, this);
            }
            else if (mid.Runtime != null && mid.Runtime != this)
            {
                this.Assert(false, "Unbound actor id '{0}' was created by another runtime.", mid.Value);
            }
            else if (mid.Type != type.FullName)
            {
                this.Assert(false, "Cannot bind actor id '{0}' of type '{1}' to an actor of type '{2}'.",
                    mid.Value, mid.Type, type.FullName);
            }
            else
            {
                mid.Bind(this);
            }

            // The operation group id of the actor is set using the following precedence:
            // (1) To the specified actor creation operation group id, if it is non-empty.
            // (2) To the operation group id of the creator actor, if it exists.
            // (3) To the empty operation group id.
            if (opGroupId == Guid.Empty && creator != null)
            {
                opGroupId = creator.OperationGroupId;
            }

            Actor actor = ActorFactory.Create(type);
            IActorStateManager stateManager = new ActorStateManager(this, actor, opGroupId);
            IEventQueue eventQueue = new EventQueue(stateManager);

            actor.Initialize(this, mid, stateManager, eventQueue);
            actor.InitializeStateInformation();

            if (!this.ActorMap.TryAdd(mid, actor))
            {
                string info = "This typically occurs if either the actor id was created by another runtime instance, " +
                    "or if an actor id from a previous runtime generation was deserialized, but the current runtime " +
                    "has not increased its generation value.";
                this.Assert(false, "Actor with id '{0}' was already created in generation '{1}'. {2}", mid.Value, mid.Generation, info);
            }

            return actor;
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor.
        /// </summary>
        internal override void SendEvent(ActorId target, Event e, AsyncActor sender, Guid opGroupId, SendOptions options)
        {
            EnqueueStatus enqueueStatus = this.EnqueueEvent(target, e, sender, opGroupId, out Actor targetActor);
            if (enqueueStatus is EnqueueStatus.EventHandlerNotRunning)
            {
                this.RunActorEventHandler(targetActor, null, false);
            }
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor. Returns immediately if the target actor was
        /// already running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        internal override async Task<bool> SendEventAndExecuteAsync(ActorId target, Event e, AsyncActor sender,
            Guid opGroupId, SendOptions options)
        {
            EnqueueStatus enqueueStatus = this.EnqueueEvent(target, e, sender, opGroupId, out Actor targetActor);
            if (enqueueStatus is EnqueueStatus.EventHandlerNotRunning)
            {
                await this.RunActorEventHandlerAsync(targetActor, null, false);
                return true;
            }

            return enqueueStatus is EnqueueStatus.Dropped;
        }

        /// <summary>
        /// Enqueues an event to the actor with the specified id.
        /// </summary>
        private EnqueueStatus EnqueueEvent(ActorId target, Event e, AsyncActor sender, Guid opGroupId, out Actor targetActor)
        {
            if (target is null)
            {
                string message = sender != null ?
                    string.Format("Actor '{0}' is sending to a null actor.", sender.Id.ToString()) :
                    "Cannot send to a null actor.";
                this.Assert(false, message);
            }

            if (e is null)
            {
                string message = sender != null ?
                    string.Format("Actor '{0}' is sending a null event.", sender.Id.ToString()) :
                    "Cannot send a null event.";
                this.Assert(false, message);
            }

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
                this.TryHandleDroppedEvent(e, target);
                return EnqueueStatus.Dropped;
            }

            this.LogWriter.OnSend(target, sender?.Id, (sender as Actor)?.CurrentStateName ?? string.Empty,
                e.GetType().FullName, opGroupId, isTargetHalted: false);

            EnqueueStatus enqueueStatus = targetActor.Enqueue(e, opGroupId, null);
            if (enqueueStatus == EnqueueStatus.Dropped)
            {
                this.TryHandleDroppedEvent(e, target);
            }

            return enqueueStatus;
        }

        /// <summary>
        /// Runs a new asynchronous actor event handler.
        /// This is a fire and forget invocation.
        /// </summary>
        private void RunActorEventHandler(Actor actor, Event initialEvent, bool isFresh)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (isFresh)
                    {
                        await actor.GotoStartState(initialEvent);
                    }

                    await actor.RunEventHandlerAsync();
                }
                catch (Exception ex)
                {
                    this.IsRunning = false;
                    this.RaiseOnFailureEvent(ex);
                }
                finally
                {
                    if (actor.IsHalted)
                    {
                        this.ActorMap.TryRemove(actor.Id, out AsyncActor _);
                    }
                }
            });
        }

        /// <summary>
        /// Runs a new asynchronous actor event handler.
        /// </summary>
        private async Task RunActorEventHandlerAsync(Actor actor, Event initialEvent, bool isFresh)
        {
            try
            {
                if (isFresh)
                {
                    await actor.GotoStartState(initialEvent);
                }

                await actor.RunEventHandlerAsync();
            }
            catch (Exception ex)
            {
                this.IsRunning = false;
                this.RaiseOnFailureEvent(ex);
                return;
            }
        }

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask CreateActorTask(Action action, CancellationToken cancellationToken) =>
            new ActorTask(Task.Run(action, cancellationToken));

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask CreateActorTask(Func<Task> function, CancellationToken cancellationToken) =>
            new ActorTask(Task.Run(function, cancellationToken));

        /// <summary>
        /// Creates a new <see cref="ActorTask{TResult}"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask<TResult> CreateActorTask<TResult>(Func<TResult> function,
            CancellationToken cancellationToken) =>
            new ActorTask<TResult>(Task.Run(function, cancellationToken));

        /// <summary>
        /// Creates a new <see cref="ActorTask{TResult}"/> to execute the specified asynchronous work.
        /// </summary>
        internal override ActorTask<TResult> CreateActorTask<TResult>(Func<Task<TResult>> function,
            CancellationToken cancellationToken) =>
            new ActorTask<TResult>(Task.Run(function, cancellationToken));

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to execute the specified asynchronous delay.
        /// </summary>
        internal override ActorTask CreateActorTask(int millisecondsDelay, CancellationToken cancellationToken) =>
            new ActorTask(Task.Delay(millisecondsDelay, cancellationToken));

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to complete with the specified task.
        /// </summary>
        internal override ActorTask CreateCompletionActorTask(Task task) => new ActorTask(task);

        /// <summary>
        /// Creates a new <see cref="ActorTask"/> to complete with the specified task.
        /// </summary>
        internal override ActorTask<TResult> CreateCompletionActorTask<TResult>(Task<TResult> task) =>
            new ActorTask<TResult>(task);

        /// <summary>
        /// Asynchronously waits for the specified tasks to complete.
        /// </summary>
        internal override ActorTask WaitAllTasksAsync(IEnumerable<Task> tasks) =>
            new ActorTask(Task.WhenAll(tasks));

        /// <summary>
        /// Asynchronously waits for all specified tasks to complete.
        /// </summary>
        internal override ActorTask<TResult[]> WaitAllTasksAsync<TResult>(IEnumerable<Task<TResult>> tasks) =>
            new ActorTask<TResult[]>(Task.WhenAll(tasks));

        /// <summary>
        /// Asynchronously waits for any of the specified tasks to complete.
        /// </summary>
        internal override ActorTask<Task> WaitAnyTaskAsync(IEnumerable<Task> tasks) =>
            new ActorTask<Task>(Task.WhenAny(tasks));

        /// <summary>
        /// Asynchronously waits for any of the specified tasks to complete.
        /// </summary>
        internal override ActorTask<Task<TResult>> WaitAnyTaskAsync<TResult>(IEnumerable<Task<TResult>> tasks) =>
            new ActorTask<Task<TResult>>(Task.WhenAny(tasks));

        /// <summary>
        /// Creates a new timer that sends a <see cref="TimerElapsedEvent"/> to its owner actor.
        /// </summary>
        internal override IActorTimer CreateActorTimer(TimerInfo info, Actor owner) => new ActorTimer(info, owner);

        /// <summary>
        /// Tries to create a new <see cref="CoyoteActors.Monitor"/> of the specified <see cref="Type"/>.
        /// </summary>
        internal override void TryCreateMonitor(Type type)
        {
            // Check if monitors are enabled in production.
            if (!this.Configuration.EnableMonitorsInProduction)
            {
                return;
            }

            lock (this.Monitors)
            {
                if (this.Monitors.Any(m => m.GetType() == type))
                {
                    // Idempotence: only one monitor per type can exist.
                    return;
                }
            }

            this.Assert(type.IsSubclassOf(typeof(Monitor)), "Type '{0}' is not a subclass of Monitor.", type.FullName);

            ActorId mid = new ActorId(type, null, this);
            Monitor monitor = (Monitor)Activator.CreateInstance(type);

            monitor.Initialize(this, mid);
            monitor.InitializeStateInformation();

            lock (this.Monitors)
            {
                this.Monitors.Add(monitor);
            }

            this.LogWriter.OnCreateMonitor(type.FullName, monitor.Id);

            monitor.GotoStartState();
        }

        /// <summary>
        /// Invokes the specified <see cref="CoyoteActors.Monitor"/> with the specified <see cref="Event"/>.
        /// </summary>
        internal override void Monitor(Type type, AsyncActor sender, Event e)
        {
            // Check if monitors are enabled in production.
            if (!this.Configuration.EnableMonitorsInProduction)
            {
                return;
            }

            Monitor monitor = null;

            lock (this.Monitors)
            {
                foreach (var m in this.Monitors)
                {
                    if (m.GetType() == type)
                    {
                        monitor = m;
                        break;
                    }
                }
            }

            if (monitor != null)
            {
                lock (monitor)
                {
                    monitor.MonitorEvent(e);
                }
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
        /// Notifies that an actor invoked an action.
        /// </summary>
        internal override void NotifyInvokedOnEntryAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
            if (this.Configuration.IsVerbose)
            {
                this.LogWriter.OnActorAction(actor.Id, actor.CurrentStateName, action.Name);
            }
        }

        /// <summary>
        /// Notifies that an actor invoked an action.
        /// </summary>
        internal override void NotifyInvokedOnExitAction(Actor actor, MethodInfo action, Event receivedEvent)
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
                    this.LogWriter.OnWait(actor.Id, actor.CurrentStateName, eventWaitTypesArray[0]);
                }
                else
                {
                    this.LogWriter.OnWait(actor.Id, actor.CurrentStateName, eventWaitTypesArray);
                }
            }
        }

        /// <summary>
        /// Notifies that an actor enqueued an event that it was waiting to receive.
        /// </summary>
        internal override void NotifyReceivedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
            this.LogWriter.OnReceive(actor.Id, actor.CurrentStateName, e.GetType().FullName, wasBlocked: true);
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
        /// Disposes runtime resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Monitors.Clear();
                this.ActorMap.Clear();
            }

            base.Dispose(disposing);
        }
    }
}
