// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.Coyote.TestingServices.Coverage
{
    /// <summary>
    /// The Coyote code coverage reporter.
    /// </summary>
    public class ActivityCoverageReporter
    {
        /// <summary>
        /// Data structure containing information
        /// regarding testing coverage.
        /// </summary>
        private readonly CoverageInfo CoverageInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityCoverageReporter"/> class.
        /// </summary>
        public ActivityCoverageReporter(CoverageInfo coverageInfo)
        {
            this.CoverageInfo = coverageInfo;
        }

        /// <summary>
        /// Emits the visualization graph.
        /// </summary>
        public void EmitVisualizationGraph(string graphFile)
        {
            using (var writer = new XmlTextWriter(graphFile, Encoding.UTF8))
            {
                this.WriteVisualizationGraph(writer);
            }
        }

        /// <summary>
        /// Emits the code coverage report.
        /// </summary>
        public void EmitCoverageReport(string coverageFile)
        {
            using (var writer = new StreamWriter(coverageFile))
            {
                this.WriteCoverageText(writer);
            }
        }

        /// <summary>
        /// Writes the visualization graph.
        /// </summary>
        private void WriteVisualizationGraph(XmlTextWriter writer)
        {
            // Starts document.
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;

            // Starts DirectedGraph element.
            writer.WriteStartElement("DirectedGraph", @"http://schemas.microsoft.com/vs/2009/dgml");

            // Starts Nodes element.
            writer.WriteStartElement("Nodes");

            // Iterates actors.
            foreach (var actor in this.CoverageInfo.ActorsToStates.Keys)
            {
                writer.WriteStartElement("Node");
                writer.WriteAttributeString("Id", actor);
                writer.WriteAttributeString("Group", "Expanded");
                writer.WriteEndElement();
            }

            // Iterates states.
            foreach (var tup in this.CoverageInfo.ActorsToStates)
            {
                var actor = tup.Key;
                foreach (var state in tup.Value)
                {
                    writer.WriteStartElement("Node");
                    writer.WriteAttributeString("Id", GetStateId(actor, state));
                    writer.WriteAttributeString("Label", state);
                    writer.WriteEndElement();
                }
            }

            // Ends Nodes element.
            writer.WriteEndElement();

            // Starts Links element.
            writer.WriteStartElement("Links");

            // Iterates states.
            foreach (var tup in this.CoverageInfo.ActorsToStates)
            {
                var actor = tup.Key;
                foreach (var state in tup.Value)
                {
                    writer.WriteStartElement("Link");
                    writer.WriteAttributeString("Source", actor);
                    writer.WriteAttributeString("Target", GetStateId(actor, state));
                    writer.WriteAttributeString("Category", "Contains");
                    writer.WriteEndElement();
                }
            }

            var parallelEdgeCounter = new Dictionary<Tuple<string, string>, int>();

            // Iterates transitions.
            foreach (var transition in this.CoverageInfo.Transitions)
            {
                var source = GetStateId(transition.ActorOrigin, transition.StateOrigin);
                var target = GetStateId(transition.ActorTarget, transition.StateTarget);
                var counter = 0;
                if (parallelEdgeCounter.ContainsKey(Tuple.Create(source, target)))
                {
                    counter = parallelEdgeCounter[Tuple.Create(source, target)];
                    parallelEdgeCounter[Tuple.Create(source, target)] = counter + 1;
                }
                else
                {
                    parallelEdgeCounter[Tuple.Create(source, target)] = 1;
                }

                writer.WriteStartElement("Link");
                writer.WriteAttributeString("Source", source);
                writer.WriteAttributeString("Target", target);
                writer.WriteAttributeString("Label", transition.EdgeLabel);
                if (counter != 0)
                {
                    writer.WriteAttributeString("Index", counter.ToString());
                }

                writer.WriteEndElement();
            }

            // Ends Links element.
            writer.WriteEndElement();

            // Ends DirectedGraph element.
            writer.WriteEndElement();

            // Ends document.
            writer.WriteEndDocument();
        }

        /// <summary>
        /// Writes the visualization text.
        /// </summary>
        internal void WriteCoverageText(TextWriter writer)
        {
            var actors = new List<string>(this.CoverageInfo.ActorsToStates.Keys);

            var uncoveredEvents = new HashSet<Tuple<string, string, string>>(this.CoverageInfo.RegisteredEvents);
            foreach (var transition in this.CoverageInfo.Transitions)
            {
                if (transition.ActorOrigin == transition.ActorTarget)
                {
                    uncoveredEvents.Remove(Tuple.Create(transition.ActorOrigin, transition.StateOrigin, transition.EdgeLabel));
                }
                else
                {
                    uncoveredEvents.Remove(Tuple.Create(transition.ActorTarget, transition.StateTarget, transition.EdgeLabel));
                }
            }

            string eventCoverage = this.CoverageInfo.RegisteredEvents.Count == 0 ? "100.0" :
                ((this.CoverageInfo.RegisteredEvents.Count - uncoveredEvents.Count) * 100.0 / this.CoverageInfo.RegisteredEvents.Count).ToString("F1");
            writer.WriteLine("Total event coverage: {0}%", eventCoverage);

            // Map from actors to states to registered events.
            var actorToStatesToEvents = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            actors.ForEach(m => actorToStatesToEvents.Add(m, new Dictionary<string, HashSet<string>>()));
            actors.ForEach(m =>
            {
                foreach (var state in this.CoverageInfo.ActorsToStates[m])
                {
                    actorToStatesToEvents[m].Add(state, new HashSet<string>());
                }
            });

            foreach (var ev in this.CoverageInfo.RegisteredEvents)
            {
                actorToStatesToEvents[ev.Item1][ev.Item2].Add(ev.Item3);
            }

            // Maps from actors to transitions.
            var actorToOutgoingTransitions = new Dictionary<string, List<Transition>>();
            var actorToIncomingTransitions = new Dictionary<string, List<Transition>>();
            var actorToIntraTransitions = new Dictionary<string, List<Transition>>();

            actors.ForEach(m => actorToIncomingTransitions.Add(m, new List<Transition>()));
            actors.ForEach(m => actorToOutgoingTransitions.Add(m, new List<Transition>()));
            actors.ForEach(m => actorToIntraTransitions.Add(m, new List<Transition>()));

            foreach (var tr in this.CoverageInfo.Transitions)
            {
                if (tr.ActorOrigin == tr.ActorTarget)
                {
                    actorToIntraTransitions[tr.ActorOrigin].Add(tr);
                }
                else
                {
                    actorToIncomingTransitions[tr.ActorTarget].Add(tr);
                    actorToOutgoingTransitions[tr.ActorOrigin].Add(tr);
                }
            }

            // Per-actor data.
            foreach (var actor in actors)
            {
                writer.WriteLine("Actor: {0}", actor);
                writer.WriteLine("***************");

                var actorUncoveredEvents = new Dictionary<string, HashSet<string>>();
                foreach (var state in this.CoverageInfo.ActorsToStates[actor])
                {
                    actorUncoveredEvents.Add(state, new HashSet<string>(actorToStatesToEvents[actor][state]));
                }

                foreach (var tr in actorToIncomingTransitions[actor])
                {
                    actorUncoveredEvents[tr.StateTarget].Remove(tr.EdgeLabel);
                }

                foreach (var tr in actorToIntraTransitions[actor])
                {
                    actorUncoveredEvents[tr.StateOrigin].Remove(tr.EdgeLabel);
                }

                var numTotalEvents = 0;
                foreach (var tup in actorToStatesToEvents[actor])
                {
                    numTotalEvents += tup.Value.Count;
                }

                var numUncoveredEvents = 0;
                foreach (var tup in actorUncoveredEvents)
                {
                    numUncoveredEvents += tup.Value.Count;
                }

                eventCoverage = numTotalEvents == 0 ? "100.0" : ((numTotalEvents - numUncoveredEvents) * 100.0 / numTotalEvents).ToString("F1");
                writer.WriteLine("Actor event coverage: {0}%", eventCoverage);

                // Find uncovered states.
                var uncoveredStates = new HashSet<string>(this.CoverageInfo.ActorsToStates[actor]);
                foreach (var tr in actorToIntraTransitions[actor])
                {
                    uncoveredStates.Remove(tr.StateOrigin);
                    uncoveredStates.Remove(tr.StateTarget);
                }

                foreach (var tr in actorToIncomingTransitions[actor])
                {
                    uncoveredStates.Remove(tr.StateTarget);
                }

                foreach (var tr in actorToOutgoingTransitions[actor])
                {
                    uncoveredStates.Remove(tr.StateOrigin);
                }

                // State maps.
                var stateToIncomingEvents = new Dictionary<string, HashSet<string>>();
                foreach (var tr in actorToIncomingTransitions[actor])
                {
                    if (!stateToIncomingEvents.ContainsKey(tr.StateTarget))
                    {
                        stateToIncomingEvents.Add(tr.StateTarget, new HashSet<string>());
                    }

                    stateToIncomingEvents[tr.StateTarget].Add(tr.EdgeLabel);
                }

                var stateToOutgoingEvents = new Dictionary<string, HashSet<string>>();
                foreach (var tr in actorToOutgoingTransitions[actor])
                {
                    if (!stateToOutgoingEvents.ContainsKey(tr.StateOrigin))
                    {
                        stateToOutgoingEvents.Add(tr.StateOrigin, new HashSet<string>());
                    }

                    stateToOutgoingEvents[tr.StateOrigin].Add(tr.EdgeLabel);
                }

                var stateToOutgoingStates = new Dictionary<string, HashSet<string>>();
                var stateToIncomingStates = new Dictionary<string, HashSet<string>>();
                foreach (var tr in actorToIntraTransitions[actor])
                {
                    if (!stateToOutgoingStates.ContainsKey(tr.StateOrigin))
                    {
                        stateToOutgoingStates.Add(tr.StateOrigin, new HashSet<string>());
                    }

                    stateToOutgoingStates[tr.StateOrigin].Add(tr.StateTarget);

                    if (!stateToIncomingStates.ContainsKey(tr.StateTarget))
                    {
                        stateToIncomingStates.Add(tr.StateTarget, new HashSet<string>());
                    }

                    stateToIncomingStates[tr.StateTarget].Add(tr.StateOrigin);
                }

                // Per-state data.
                foreach (var state in this.CoverageInfo.ActorsToStates[actor])
                {
                    writer.WriteLine();
                    writer.WriteLine("\tState: {0}{1}", state, uncoveredStates.Contains(state) ? " is uncovered" : string.Empty);
                    if (!uncoveredStates.Contains(state))
                    {
                        eventCoverage = actorToStatesToEvents[actor][state].Count == 0 ? "100.0" :
                            ((actorToStatesToEvents[actor][state].Count - actorUncoveredEvents[state].Count) * 100.0 /
                              actorToStatesToEvents[actor][state].Count).ToString("F1");
                        writer.WriteLine("\t\tState event coverage: {0}%", eventCoverage);
                    }

                    if (stateToIncomingEvents.ContainsKey(state) && stateToIncomingEvents[state].Count > 0)
                    {
                        writer.Write("\t\tEvents received: ");
                        foreach (var e in stateToIncomingEvents[state])
                        {
                            writer.Write("{0} ", e);
                        }

                        writer.WriteLine();
                    }

                    if (stateToOutgoingEvents.ContainsKey(state) && stateToOutgoingEvents[state].Count > 0)
                    {
                        writer.Write("\t\tEvents sent: ");
                        foreach (var e in stateToOutgoingEvents[state])
                        {
                            writer.Write("{0} ", e);
                        }

                        writer.WriteLine();
                    }

                    if (actorUncoveredEvents.ContainsKey(state) && actorUncoveredEvents[state].Count > 0)
                    {
                        writer.Write("\t\tEvents not covered: ");
                        foreach (var e in actorUncoveredEvents[state])
                        {
                            writer.Write("{0} ", e);
                        }

                        writer.WriteLine();
                    }

                    if (stateToIncomingStates.ContainsKey(state) && stateToIncomingStates[state].Count > 0)
                    {
                        writer.Write("\t\tPrevious states: ");
                        foreach (var s in stateToIncomingStates[state])
                        {
                            writer.Write("{0} ", s);
                        }

                        writer.WriteLine();
                    }

                    if (stateToOutgoingStates.ContainsKey(state) && stateToOutgoingStates[state].Count > 0)
                    {
                        writer.Write("\t\tNext states: ");
                        foreach (var s in stateToOutgoingStates[state])
                        {
                            writer.Write("{0} ", s);
                        }

                        writer.WriteLine();
                    }
                }

                writer.WriteLine();
            }
        }

        private static string GetStateId(string actorName, string stateName) =>
            string.Format("{0}::{1}", stateName, actorName);
    }
}
