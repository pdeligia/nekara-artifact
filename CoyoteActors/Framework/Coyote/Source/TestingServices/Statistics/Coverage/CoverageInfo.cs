// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CoyoteActors.TestingServices.Coverage
{
    /// <summary>
    /// Class for storing coverage-specific data
    /// across multiple testing iterations.
    /// </summary>
    [DataContract]
    public class CoverageInfo
    {
        /// <summary>
        /// Map from actors to states.
        /// </summary>
        [DataMember]
        public Dictionary<string, HashSet<string>> ActorsToStates { get; private set; }

        /// <summary>
        /// Set of (actors, states, registered events).
        /// </summary>
        [DataMember]
        public HashSet<Tuple<string, string, string>> RegisteredEvents { get; private set; }

        /// <summary>
        /// Set of actor transitions.
        /// </summary>
        [DataMember]
        public HashSet<Transition> Transitions { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageInfo"/> class.
        /// </summary>
        public CoverageInfo()
        {
            this.ActorsToStates = new Dictionary<string, HashSet<string>>();
            this.RegisteredEvents = new HashSet<Tuple<string, string, string>>();
            this.Transitions = new HashSet<Transition>();
        }

        /// <summary>
        /// Checks if the actor type has already been registered for coverage.
        /// </summary>
        public bool IsActorDeclared(string actorName) => this.ActorsToStates.ContainsKey(actorName);

        /// <summary>
        /// Adds a new transition.
        /// </summary>
        public void AddTransition(string actorOrigin, string stateOrigin, string edgeLabel,
            string actorTarget, string stateTarget)
        {
            this.AddState(actorOrigin, stateOrigin);
            this.AddState(actorTarget, stateTarget);
            this.Transitions.Add(new Transition(actorOrigin, stateOrigin,
                edgeLabel, actorTarget, stateTarget));
        }

        /// <summary>
        /// Declares a state.
        /// </summary>
        public void DeclareActorState(string actor, string state) => this.AddState(actor, state);

        /// <summary>
        /// Declares a registered state, event pair.
        /// </summary>
        public void DeclareStateEvent(string actor, string state, string eventName)
        {
            this.AddState(actor, state);
            this.RegisteredEvents.Add(Tuple.Create(actor, state, eventName));
        }

        /// <summary>
        /// Merges the information from the specified
        /// coverage info. This is not thread-safe.
        /// </summary>
        public void Merge(CoverageInfo coverageInfo)
        {
            foreach (var actor in coverageInfo.ActorsToStates)
            {
                foreach (var state in actor.Value)
                {
                    this.DeclareActorState(actor.Key, state);
                }
            }

            foreach (var tup in coverageInfo.RegisteredEvents)
            {
                this.DeclareStateEvent(tup.Item1, tup.Item2, tup.Item3);
            }

            foreach (var transition in coverageInfo.Transitions)
            {
                this.AddTransition(transition.ActorOrigin, transition.StateOrigin,
                    transition.EdgeLabel, transition.ActorTarget, transition.StateTarget);
            }
        }

        /// <summary>
        /// Adds a new state.
        /// </summary>
        private void AddState(string actorName, string stateName)
        {
            if (!this.ActorsToStates.ContainsKey(actorName))
            {
                this.ActorsToStates.Add(actorName, new HashSet<string>());
            }

            this.ActorsToStates[actorName].Add(stateName);
        }
    }
}
