// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CoyoteActors.IO;
using Microsoft.CoyoteActors.Timers;

namespace Microsoft.CoyoteActors.Runtime
{
    /// <summary>
    /// Runtime for executing actors asynchronously.
    /// </summary>
    internal abstract class ActorRuntime : IActorRuntime
    {
        /// <summary>
        /// The configuration used by the runtime.
        /// </summary>
        internal readonly Configuration Configuration;

        /// <summary>
        /// Monotonically increasing actor id counter.
        /// </summary>
        internal long ActorIdCounter;

        /// <summary>
        /// Records if the runtime is running.
        /// </summary>
        internal volatile bool IsRunning;

        /// <summary>
        /// Map from unique actor ids to actors.
        /// </summary>
        protected readonly ConcurrentDictionary<ActorId, AsyncActor> ActorMap;

        /// <summary>
        /// The log writer.
        /// </summary>
        protected internal RuntimeLogWriter LogWriter { get; private set; }

        /// <summary>
        /// The installed logger.
        /// </summary>
        public ILogger Logger => this.LogWriter.Logger;

        /// <summary>
        /// Callback that is fired when the Coyote program throws an exception.
        /// </summary>
        public event OnFailureHandler OnFailure;

        /// <summary>
        /// Callback that is fired when a Coyote event is dropped.
        /// </summary>
        public event OnEventDroppedHandler OnEventDropped;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorRuntime"/> class.
        /// </summary>
        protected ActorRuntime(Configuration configuration)
        {
            this.Configuration = configuration;
            this.ActorMap = new ConcurrentDictionary<ActorId, AsyncActor>();
            this.ActorIdCounter = 0;
            this.LogWriter = new RuntimeLogWriter
            {
                Logger = configuration.IsVerbose ? (ILogger)new ConsoleLogger() : new NulLogger()
            };

            this.IsRunning = true;
        }

        /// <summary>
        /// Creates a fresh actor id that has not yet been bound to any actor.
        /// </summary>
        public ActorId CreateActorId(Type type, string actorName = null) => new ActorId(type, actorName, this);

        /// <summary>
        /// Creates an actor id that is uniquely tied to the specified unique name. The
        /// returned actor id can either be a fresh id (not yet bound to any actor),
        /// or it can be bound to a previously created actor. In the second case, this
        /// actor id can be directly used to communicate with the corresponding actor.
        /// </summary>
        public abstract ActorId CreateActorIdFromName(Type type, string actorName);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with
        /// the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        public abstract ActorId CreateActor(Type type, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and
        /// with the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        public abstract ActorId CreateActor(Type type, string actorName, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Creates a new actor of the specified type, using the specified <see cref="ActorId"/>.
        /// This method optionally passes an <see cref="Event"/> to the new actor, which can only
        /// be used to access its payload, and cannot be handled.
        /// </summary>
        public abstract ActorId CreateActor(ActorId mid, Type type, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with the
        /// specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when
        /// the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public abstract Task<ActorId> CreateActorAndExecuteAsync(Type type, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and with
        /// the specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when the
        /// actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public abstract Task<ActorId> CreateActorAndExecuteAsync(Type type, string actorName, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/>, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This
        /// event can only be used to access its payload, and cannot be handled. The method
        /// returns only when the actor is initialized and the <see cref="Event"/> (if any)
        /// is handled.
        /// </summary>
        public abstract Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and with the
        /// specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when
        /// the actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public abstract Task<ActorId> CreateActorAndExecute(Type type, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/> and name, and with
        /// the specified optional <see cref="Event"/>. This event can only be used to
        /// access its payload, and cannot be handled. The method returns only when the
        /// actor is initialized and the <see cref="Event"/> (if any) is handled.
        /// </summary>
        public abstract Task<ActorId> CreateActorAndExecute(Type type, string actorName, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Creates a new actor of the specified <see cref="Type"/>, using the specified
        /// unbound actor id, and passes the specified optional <see cref="Event"/>. This
        /// event can only be used to access its payload, and cannot be handled. The method
        /// returns only when the actor is initialized and the <see cref="Event"/> (if any)
        /// is handled.
        /// </summary>
        public abstract Task<ActorId> CreateActorAndExecute(ActorId mid, Type type, Event e = null, Guid opGroupId = default);

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor.
        /// </summary>
        public abstract void SendEvent(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null);

        /// <summary>
        /// Sends an <see cref="Event"/> to an actor. Returns immediately if the target actor was already
        /// running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        public abstract Task<bool> SendEventAndExecuteAsync(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null);

        /// <summary>
        /// Sends an <see cref="Event"/> to an actor. Returns immediately if the target actor was already
        /// running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        public abstract Task<bool> SendEventAndExecute(ActorId target, Event e, Guid opGroupId = default, SendOptions options = null);

        /// <summary>
        /// Registers a new specification monitor of the specified <see cref="Type"/>.
        /// </summary>
        public void RegisterMonitor(Type type)
        {
            this.TryCreateMonitor(type);
        }

        /// <summary>
        /// Invokes the specified monitor with the specified <see cref="Event"/>.
        /// </summary>
        public void InvokeMonitor<T>(Event e)
        {
            this.InvokeMonitor(typeof(T), e);
        }

        /// <summary>
        /// Invokes the specified monitor with the specified <see cref="Event"/>.
        /// </summary>
        public void InvokeMonitor(Type type, Event e)
        {
            // If the event is null then report an error and exit.
            this.Assert(e != null, "Cannot monitor a null event.");
            this.Monitor(type, null, e);
        }

        /// <summary>
        /// Returns a nondeterministic boolean choice, that can be controlled
        /// during analysis or testing.
        /// </summary>
        public bool Random()
        {
            return this.GetNondeterministicBooleanChoice(null, 2);
        }

        /// <summary>
        /// Returns a fair nondeterministic boolean choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        public bool FairRandom(
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var havocId = string.Format("Runtime_{0}_{1}_{2}",
                callerMemberName, callerFilePath, callerLineNumber.ToString());
            return this.GetFairNondeterministicBooleanChoice(null, havocId);
        }

        /// <summary>
        /// Returns a nondeterministic boolean choice, that can be controlled
        /// during analysis or testing. The value is used to generate a number
        /// in the range [0..maxValue), where 0 triggers true.
        /// </summary>
        public bool Random(int maxValue)
        {
            return this.GetNondeterministicBooleanChoice(null, maxValue);
        }

        /// <summary>
        /// Returns a nondeterministic integer, that can be controlled during
        /// analysis or testing. The value is used to generate an integer in
        /// the range [0..maxValue).
        /// </summary>
        public int RandomInteger(int maxValue)
        {
            return this.GetNondeterministicIntegerChoice(null, maxValue);
        }

        /// <summary>
        /// Returns the operation group id of the specified actor. During testing,
        /// the runtime asserts that the specified actor is currently executing.
        /// </summary>
        public abstract Guid GetCurrentOperationGroupId(ActorId currentActor);

        /// <summary>
        /// Terminates the runtime and notifies each active actor to halt execution.
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <returns>ActorId</returns>
        internal abstract ActorId CreateActor(ActorId mid, Type type, string actorName, Event e,
            Actor creator, Guid opGroupId);

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified <see cref="Type"/>. The
        /// method returns only when the actor is initialized and the <see cref="Event"/>
        /// (if any) is handled.
        /// </summary>
        internal abstract Task<ActorId> CreateActorAndExecuteAsync(ActorId mid, Type type, string actorName, Event e,
            Actor creator, Guid opGroupId);

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor.
        /// </summary>
        internal abstract void SendEvent(ActorId target, Event e, AsyncActor sender, Guid opGroupId, SendOptions options);

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to an actor. Returns immediately if the target actor was
        /// already running. Otherwise blocks until the actor handles the event and reaches quiescense.
        /// </summary>
        internal abstract Task<bool> SendEventAndExecuteAsync(ActorId target, Event e, AsyncActor sender,
            Guid opGroupId, SendOptions options);

        /// <summary>
        /// Creates a new timer that sends a <see cref="TimerElapsedEvent"/> to its owner actor.
        /// </summary>
        internal abstract IActorTimer CreateActorTimer(TimerInfo info, Actor owner);

        /// <summary>
        /// Tries to create a new <see cref="CoyoteActors.Monitor"/> of the specified <see cref="Type"/>.
        /// </summary>
        internal abstract void TryCreateMonitor(Type type);

        /// <summary>
        /// Invokes the specified <see cref="CoyoteActors.Monitor"/> with the specified <see cref="Event"/>.
        /// </summary>
        internal abstract void Monitor(Type type, AsyncActor sender, Event e);

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public virtual void Assert(bool predicate)
        {
            if (!predicate)
            {
                throw new AssertionFailureException("Detected an assertion failure.");
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public virtual void Assert(bool predicate, string s, object arg0)
        {
            if (!predicate)
            {
                throw new AssertionFailureException(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public virtual void Assert(bool predicate, string s, object arg0, object arg1)
        {
            if (!predicate)
            {
                throw new AssertionFailureException(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString(), arg1.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public virtual void Assert(bool predicate, string s, object arg0, object arg1, object arg2)
        {
            if (!predicate)
            {
                throw new AssertionFailureException(string.Format(CultureInfo.InvariantCulture, s, arg0.ToString(), arg1.ToString(), arg2.ToString()));
            }
        }

        /// <summary>
        /// Checks if the assertion holds, and if not, throws an <see cref="AssertionFailureException"/> exception.
        /// </summary>
        public virtual void Assert(bool predicate, string s, params object[] args)
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
        internal abstract bool GetNondeterministicBooleanChoice(AsyncActor actor, int maxValue);

        /// <summary>
        /// Returns a fair nondeterministic boolean choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        internal abstract bool GetFairNondeterministicBooleanChoice(AsyncActor actor, string uniqueId);

        /// <summary>
        /// Returns a nondeterministic integer choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        internal abstract int GetNondeterministicIntegerChoice(AsyncActor actor, int maxValue);

        /// <summary>
        /// Gets the actor of type <typeparamref name="TActor"/> with the specified id,
        /// or null if no such actor exists.
        /// </summary>
        internal TActor GetActorFromId<TActor>(ActorId id)
            where TActor : AsyncActor =>
            id != null && this.ActorMap.TryGetValue(id, out AsyncActor value) &&
            value is TActor actor ? actor : null;

        /// <summary>
        /// Notifies that an actor entered a state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyEnteredState(Actor actor)
        {
        }

        /// <summary>
        /// Notifies that a monitor entered a state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyEnteredState(Monitor monitor)
        {
        }

        /// <summary>
        /// Notifies that an actor exited a state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyExitedState(Actor actor)
        {
        }

        /// <summary>
        /// Notifies that a monitor exited a state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyExitedState(Monitor monitor)
        {
        }

        /// <summary>
        /// Notifies that an actor invoked an action.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyInvokedAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
        }

        /// <summary>
        /// Notifies that an actor completed invoking an action.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyCompletedAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
        }

        /// <summary>
        /// Notifies that an actor invoked an action.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyInvokedOnEntryAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
        }

        /// <summary>
        /// Notifies that an actor completed invoking an action.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyCompletedOnEntryAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
        }

        /// <summary>
        /// Notifies that an actor invoked an action.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyInvokedOnExitAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
        }

        /// <summary>
        /// Notifies that an actor completed invoking an action.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyCompletedOnExitAction(Actor actor, MethodInfo action, Event receivedEvent)
        {
        }

        /// <summary>
        /// Notifies that a monitor invoked an action.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyInvokedAction(Monitor monitor, MethodInfo action, Event receivedEvent)
        {
        }

        /// <summary>
        /// Notifies that an actor raised an <see cref="Event"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyRaisedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
        }

        /// <summary>
        /// Notifies that a monitor raised an <see cref="Event"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyRaisedEvent(Monitor monitor, Event e, EventInfo eventInfo)
        {
        }

        /// <summary>
        /// Notifies that an actor dequeued an <see cref="Event"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyDequeuedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
        }

        /// <summary>
        /// Notifies that an actor invoked pop.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyPop(Actor actor)
        {
        }

        /// <summary>
        /// Notifies that an actor called Receive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyReceiveCalled(Actor actor)
        {
        }

        /// <summary>
        /// Notifies that an actor is handling a raised <see cref="Event"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyHandleRaisedEvent(Actor actor, Event e)
        {
        }

        /// <summary>
        /// Notifies that an actor is waiting for the specified task to complete.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyWaitTask(AsyncActor actor, Task task)
        {
        }

        /// <summary>
        /// Notifies that an actor is waiting to receive an event of one of the specified types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyWaitEvent(Actor actor, IEnumerable<Type> eventTypes)
        {
        }

        /// <summary>
        /// Notifies that an actor enqueued an event that it was waiting to receive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyReceivedEvent(Actor actor, Event e, EventInfo eventInfo)
        {
        }

        /// <summary>
        /// Notifies that an actor received an event without waiting because the event
        /// was already in the inbox when the actor invoked the receive statement.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyReceivedEventWithoutWaiting(Actor actor, Event e, EventInfo eventInfo)
        {
        }

        /// <summary>
        /// Notifies that an actor has halted.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyHalted(Actor actor)
        {
        }

        /// <summary>
        /// Notifies that the inbox of the specified actor is about to be
        /// checked to see if the default event handler should fire.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyDefaultEventHandlerCheck(Actor actor)
        {
        }

        /// <summary>
        /// Notifies that the default handler of the specified actor has been fired.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void NotifyDefaultHandlerFired(Actor actor)
        {
        }

        /// <summary>
        /// Use this method to override the default <see cref="RuntimeLogWriter"/>
        /// for logging runtime messages.
        /// </summary>
        public RuntimeLogWriter SetLogWriter(RuntimeLogWriter logWriter)
        {
            var logger = this.LogWriter.Logger;
            var prevLogWriter = this.LogWriter;
            this.LogWriter = logWriter ?? throw new InvalidOperationException("Cannot install a null log writer.");
            this.SetLogger(logger);
            return prevLogWriter;
        }

        /// <summary>
        /// Use this method to override the default <see cref="ILogger"/> for logging messages.
        /// </summary>
        public ILogger SetLogger(ILogger logger)
        {
            var prevLogger = this.LogWriter.Logger;
            if (this.LogWriter != null)
            {
                this.LogWriter.Logger = logger ?? throw new InvalidOperationException("Cannot install a null logger.");
            }
            else
            {
                throw new InvalidOperationException("Cannot install a logger on a null log writer.");
            }

            return prevLogger;
        }

        /// <summary>
        /// Raises the <see cref="OnFailure"/> event with the specified <see cref="Exception"/>.
        /// </summary>
        protected internal void RaiseOnFailureEvent(Exception exception)
        {
            if (this.Configuration.AttachDebugger && exception is ActorActionExceptionFilterException &&
                !((exception as ActorActionExceptionFilterException).InnerException is RuntimeException))
            {
                System.Diagnostics.Debugger.Break();
                this.Configuration.AttachDebugger = false;
            }

            this.OnFailure?.Invoke(exception);
        }

        /// <summary>
        /// Tries to handle the specified dropped <see cref="Event"/>.
        /// </summary>
        internal void TryHandleDroppedEvent(Event e, ActorId mid)
        {
            this.OnEventDropped?.Invoke(e, mid);
        }

        /// <summary>
        /// Throws an <see cref="AssertionFailureException"/> exception
        /// containing the specified exception.
        /// </summary>
        internal virtual void WrapAndThrowException(Exception exception, string s, params object[] args)
        {
            throw (exception is AssertionFailureException)
                ? exception
                : new AssertionFailureException(string.Format(CultureInfo.InvariantCulture, s, args), exception);
        }

        /// <summary>
        /// Waits until all actors have finished execution.
        /// </summary>
        public abstract Task WaitAsync();

        /// <summary>
        /// Disposes runtime resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ActorIdCounter = 0;
            }
        }

        /// <summary>
        /// Disposes runtime resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
