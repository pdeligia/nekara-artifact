// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microsoft.CoyoteActors.TestingServices.Tracing.Error
{
    /// <summary>
    /// Class implementing a Coyote bug trace. A trace is a
    /// series of transitions from some initial state to
    /// some end state.
    /// </summary>
    [DataContract]
    internal sealed class BugTrace : IEnumerable, IEnumerable<BugTraceStep>
    {
        /// <summary>
        /// The steps of the bug trace.
        /// </summary>
        [DataMember]
        private readonly List<BugTraceStep> Steps;

        /// <summary>
        /// The number of steps in the bug trace.
        /// </summary>
        internal int Count
        {
            get { return this.Steps.Count; }
        }

        /// <summary>
        /// Index for the bug trace.
        /// </summary>
        internal BugTraceStep this[int index]
        {
            get { return this.Steps[index]; }
            set { this.Steps[index] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BugTrace"/> class.
        /// </summary>
        internal BugTrace()
        {
            this.Steps = new List<BugTraceStep>();
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddCreateActorStep(Actor actor, ActorId targetActor, EventInfo eventInfo)
        {
            ActorId mid = null;
            string actorState = null;
            if (actor != null)
            {
                mid = actor.Id;
                actorState = actor.CurrentStateName;
            }

            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.CreateActor,
                mid, actorState, eventInfo, null, targetActor, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddCreateMonitorStep(ActorId monitor)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.CreateMonitor,
                null, null, null, null, monitor, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddSendEventStep(ActorId actor, string actorState,
            EventInfo eventInfo, ActorId targetActor)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.SendEvent,
                actor, actorState, eventInfo, null, targetActor, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddDequeueEventStep(ActorId actor, string actorState, EventInfo eventInfo)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.DequeueEvent,
                actor, actorState, eventInfo, null, null, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddRaiseEventStep(ActorId actor, string actorState, EventInfo eventInfo)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.RaiseEvent,
                actor, actorState, eventInfo, null, null, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddGotoStateStep(ActorId actor, string actorState)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.GotoState,
                actor, actorState, null, null, null, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddInvokeActionStep(ActorId actor, string actorState, MethodInfo action)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.InvokeAction,
                actor, actorState, null, action, null, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddWaitToReceiveStep(ActorId actor, string actorState, string eventNames)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.WaitToReceive,
                actor, actorState, null, null, null, null, null, eventNames);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddReceivedEventStep(ActorId actor, string actorState, EventInfo eventInfo)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.ReceiveEvent,
                actor, actorState, eventInfo, null, null, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddRandomChoiceStep(ActorId actor, string actorState, bool choice)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.RandomChoice,
                actor, actorState, null, null, null, choice, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddRandomChoiceStep(ActorId actor, string actorState, int choice)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.RandomChoice,
                actor, actorState, null, null, null, null, choice, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Adds a bug trace step.
        /// </summary>
        internal void AddHaltStep(ActorId actor, string actorState)
        {
            var scheduleStep = BugTraceStep.Create(this.Count, BugTraceStepType.Halt,
                actor, actorState, null, null, null, null, null, null);
            this.Push(scheduleStep);
        }

        /// <summary>
        /// Returns the latest bug trace step and removes it from the trace.
        /// </summary>
        internal BugTraceStep Pop()
        {
            if (this.Count > 0)
            {
                this.Steps[this.Count - 1].Next = null;
            }

            var step = this.Steps[this.Count - 1];
            this.Steps.RemoveAt(this.Count - 1);

            return step;
        }

        /// <summary>
        /// Returns the latest bug trace step without removing it.
        /// </summary>
        internal BugTraceStep Peek()
        {
            BugTraceStep step = null;

            if (this.Steps.Count > 0)
            {
                step = this.Steps[this.Count - 1];
            }

            return step;
        }

        /// <summary>
        /// Returns an enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Steps.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator.
        /// </summary>
        IEnumerator<BugTraceStep> IEnumerable<BugTraceStep>.GetEnumerator()
        {
            return this.Steps.GetEnumerator();
        }

        /// <summary>
        /// Pushes a new step to the trace.
        /// </summary>
        private void Push(BugTraceStep step)
        {
            if (this.Count > 0)
            {
                this.Steps[this.Count - 1].Next = step;
                step.Previous = this.Steps[this.Count - 1];
            }

            this.Steps.Add(step);
        }
    }
}
