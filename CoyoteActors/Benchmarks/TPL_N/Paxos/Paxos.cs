﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CoyoteActors;

namespace Benchmarks
{
    internal class Paxos
    {
        internal readonly static ConcurrentDictionary<int, bool> States = new ConcurrentDictionary<int, bool>();
        internal static int BugsFound = 0;
        internal static object BugsFoundLock = new object();

        public static void Execute(IActorRuntime runtime)
        {
            runtime.RegisterMonitor(typeof(SafetyMonitor));
            runtime.CreateActor(typeof(ClusterManager), new ClusterManagerSetupEvent(runtime));
        }

        private class Proposal
        {
            public Proposal(string proposerName, int id)
            {
                this.ProposerName = proposerName;
                this.Id = id;
            }

            public string ProposerName { get; private set; }

            public int Id { get; private set; }

            public bool GreaterThan(Proposal p2)
            {
                if (Id != p2.Id)
                {
                    return Id > p2.Id;
                }

                return ProposerName.CompareTo(p2.ProposerName) > 0;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                if (!(obj is Proposal))
                {
                    return false;
                }

                var other = (Proposal)obj;

                return Id == other.Id && ProposerName == other.ProposerName;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class AcceptorSetupEvent : Event
        {
            public AcceptorSetupEvent(string name, Dictionary<string, ActorId> proposers, Dictionary<string, ActorId> learners)
            {
                this.Name = name;
                this.Proposers = proposers;
                this.Learners = learners;
            }

            public string Name { get; private set; }

            public Dictionary<string, ActorId> Proposers { get; private set; }

            public Dictionary<string, ActorId> Learners { get; private set; }
        }

        private class AcceptRequest : Event
        {
            public AcceptRequest(ActorId from, Proposal proposal, string value)
            {
                this.From = from;
                this.Proposal = proposal;
                this.Value = value;
            }

            public ActorId From { get; private set; }

            public string Value { get; private set; }

            public Proposal Proposal { get; private set; }
        }

        private class ClientProposeValueRequest : Event
        {
            public ClientProposeValueRequest(ActorId from, string value)
            {
                this.From = from;
                this.Value = value;
            }

            public ActorId From { get; private set; }

            public string Value { get; private set; }
        }

        private class LearnerSetupEvent : Event
        {
            public LearnerSetupEvent(string name, Dictionary<string, ActorId> acceptors)
            {
                this.Name = name;
                this.Acceptors = acceptors;
            }

            public string Name { get; private set; }
            public Dictionary<string, ActorId> Acceptors { get; private set; }
        }

        private class ClusterManagerSetupEvent : Event
        {
            public ClusterManagerSetupEvent(IActorRuntime runtime)
            {
                this.Runtime = runtime;
            }

            public IActorRuntime Runtime { get; private set; }
        }

        private class ProposalRequest : Event
        {
            public ProposalRequest(ActorId from, Proposal proposal)
            {
                this.From = from;
                this.Proposal = proposal;
            }

            public ActorId From { get; private set; }

            public Proposal Proposal { get; private set; }
        }

        private class ProposerInitEvent : Event
        {
            public ProposerInitEvent(string name, Dictionary<string, ActorId> acceptors)
            {
                this.Name = name;
                this.Acceptors = acceptors;
            }

            public string Name { get; private set; }

            public Dictionary<string, ActorId> Acceptors { get; private set; }
        }

        private class ValueAcceptedEvent : Event
        {
            public ValueAcceptedEvent(ActorId acceptor, Proposal proposal, string value)
            {
                this.Proposal = proposal;
                this.Acceptor = acceptor;
                this.Value = value;
            }

            public ActorId Acceptor { get; private set; }

            public Proposal Proposal { get; private set; }

            public string Value { get; private set; }
        }

        private class ValueLearnedEvent : Event
        {
        }

        private class ProposalResponse : Event
        {
            public ProposalResponse(
                ActorId from,
                Proposal proposal,
                bool acknowledged,
                Proposal previouslyAcceptedProposal,
                string previouslyAcceptedValue)
            {
                this.From = from;
                this.Proposal = proposal;
                this.Acknowledged = acknowledged;
                this.PreviouslyAcceptedProposal = previouslyAcceptedProposal;
                this.PreviouslyAcceptedValue = previouslyAcceptedValue;
            }

            public ActorId From { get; private set; }

            public Proposal Proposal { get; private set; }

            public bool Acknowledged { get; private set; }

            public Proposal PreviouslyAcceptedProposal { get; private set; }

            public string PreviouslyAcceptedValue { get; private set; }
        }

        private class ClusterManager : Actor
        {
            private static int numProposers = 3;
            private static int numAcceptors = 5;
            private static int numLearners = 1;
            private static int maxAcceptorFailureCount = 2;

            private static Dictionary<string, ActorId> proposerNameToActorId;
            private static Dictionary<string, ActorId> acceptorNameToActorId;
            private static Dictionary<string, ActorId> learnerNameToActorId;

            public void InitOnEntry()
            {
                var initEvent = (ClusterManagerSetupEvent)ReceivedEvent;
                var runtime = initEvent.Runtime;

                proposerNameToActorId = CreateActorIds(
                    runtime,
                    typeof(Proposer),
                    GetProposerName,
                    numProposers);

                acceptorNameToActorId = CreateActorIds(
                    runtime,
                    typeof(Acceptor),
                    GetAcceptorName,
                    numAcceptors);

                learnerNameToActorId = CreateActorIds(
                    runtime,
                    typeof(Learner),
                    GetLearnerName,
                    numLearners);

                foreach (var name in proposerNameToActorId.Keys)
                {
                    runtime.CreateActor(
                        proposerNameToActorId[name],
                        typeof(Proposer),
                        new ProposerInitEvent(
                            name,
                            acceptorNameToActorId));
                }

                foreach (var name in acceptorNameToActorId.Keys)
                {
                    runtime.CreateActor(
                        acceptorNameToActorId[name],
                        typeof(Acceptor),
                        new AcceptorSetupEvent(
                            name,
                            proposerNameToActorId,
                            learnerNameToActorId));
                }

                foreach (var name in learnerNameToActorId.Keys)
                {
                    runtime.CreateActor(
                        learnerNameToActorId[name],
                        typeof(Learner),
                        new LearnerSetupEvent(
                            name,
                            acceptorNameToActorId));
                }

                for (int i = 0; i < numProposers; i++)
                {
                    var proposerName = GetProposerName(i);
                    var value = GetValue(i);
                    runtime.SendEvent(proposerNameToActorId[proposerName], new ClientProposeValueRequest(null, value));
                }

                int failureCount = 0;
                for (int i = 0; i < numAcceptors; i++)
                {
                    if (Random() && failureCount < maxAcceptorFailureCount)
                    {
                        failureCount++;
                        Send(acceptorNameToActorId[GetAcceptorName(i)], new Halt());
                    }
                }
            }

            private static Dictionary<string, ActorId> CreateActorIds(
              IActorRuntime runtime,
              Type actorType,
              Func<int, string> actorNameFunc,
              int numActors)
            {
                var result = new Dictionary<string, ActorId>();

                for (int i = 0; i < numActors; i++)
                {
                    var name = actorNameFunc(i);
                    var actorId = runtime.CreateActorIdFromName(actorType, name);
                    result[name] = actorId;
                }

                return result;
            }

            private static string GetProposerName(int index)
            {
                return "p" + (index + 1);
            }

            private static string GetAcceptorName(int index)
            {
                return "a" + (index + 1);
            }

            private static string GetLearnerName(int index)
            {
                return "l" + (index + 1);
            }

            private static string GetValue(int index)
            {
                return "v" + (index + 1);
            }

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            internal class Init : ActorState
            {
            }
        }

        private class Proposer : Actor
        {
            private string name;
            private Dictionary<string, ActorId> acceptors;

            private int proposalCounter = 0;

            private string value;
            private Proposal currentProposal = null;
            private HashSet<ActorId> receivedProposalResponses = null;

            private Dictionary<ActorId, Proposal> acceptorToPreviouslyAcceptedProposal = new Dictionary<ActorId, Proposal>();
            private Dictionary<ActorId, string> acceptorToPreviouslyAcceptedValue = new Dictionary<ActorId, string>();

            private bool finalValueChosen = false;

            protected override int HashedState
            {
                get
                {
                    int hash = 37;
                    hash = (hash * 397) + this.finalValueChosen.GetHashCode();
                    hash = (hash * 397) + this.proposalCounter.GetHashCode();
                    if (this.value != null)
                    {
                        hash = (hash * 397) + this.value.GetHashCode();
                    }

                    States.GetOrAdd(hash, true);

                    return hash;
                }
            }

            public void InitOnEntry()
            {
                var initEvent = (ProposerInitEvent)ReceivedEvent;

                this.name = initEvent.Name;
                this.acceptors = initEvent.Acceptors;

                Goto<Ready>();
            }

            public void ProposeValueRequestHandler()
            {
                var request = (ClientProposeValueRequest)ReceivedEvent;

                this.value = request.Value;
                this.proposalCounter++;
                this.receivedProposalResponses = new HashSet<ActorId>();
                this.currentProposal = new Proposal(this.name, proposalCounter);

                foreach (var acceptor in acceptors.Values)
                {
                    Send(acceptor, new ProposalRequest(this.Id, this.currentProposal));
                }

                Goto<WaitingForAcks>();
            }

            public void ProposalResponseHandler()
            {
                var response = (ProposalResponse)ReceivedEvent;

                if (!response.Acknowledged)
                {
                    return;
                }

                var fromAcceptor = response.From;
                var previouslyAcceptedProposal = response.PreviouslyAcceptedProposal;
                var previouslyAcceptedValue = response.PreviouslyAcceptedValue;

                if (previouslyAcceptedProposal != null)
                {
                    acceptorToPreviouslyAcceptedProposal[fromAcceptor] = previouslyAcceptedProposal;
                    acceptorToPreviouslyAcceptedValue[fromAcceptor] = previouslyAcceptedValue;
                }

                receivedProposalResponses.Add(response.From);

                bool greaterThan50PercentObservations = (receivedProposalResponses.Count / (double)acceptors.Count) > 0.5;
                if (!finalValueChosen && greaterThan50PercentObservations)
                {
                    Proposal chosenPreviouslyAcceptedProposal = null;
                    string chosenValue = null;
                    foreach (var acceptor in acceptorToPreviouslyAcceptedProposal.Keys)
                    {
                        var proposal = acceptorToPreviouslyAcceptedProposal[acceptor];
                        if (chosenPreviouslyAcceptedProposal == null)
                        {
                            chosenPreviouslyAcceptedProposal = proposal;
                            chosenValue = acceptorToPreviouslyAcceptedValue[acceptor];
                        }
                        else if (!proposal.GreaterThan(chosenPreviouslyAcceptedProposal))
                        {
                            chosenPreviouslyAcceptedProposal = proposal;
                            chosenValue = acceptorToPreviouslyAcceptedValue[acceptor];
                        }
                    }

                    if (chosenPreviouslyAcceptedProposal != null)
                    {
                        Assert(chosenValue != null);
                        this.value = chosenValue;
                    }

                    finalValueChosen = true;

                    foreach (var acceptor in receivedProposalResponses)
                    {
                        Send(acceptor, new AcceptRequest(this.Id, this.currentProposal, this.value));
                    }
                }
                else if (finalValueChosen)
                {
                    Send(fromAcceptor, new AcceptRequest(this.Id, this.currentProposal, this.value));
                }
            }

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            internal class Init : ActorState
            {
            }

            [OnEventDoAction(typeof(ClientProposeValueRequest), nameof(ProposeValueRequestHandler))]
            internal class Ready : ActorState
            {
            }

            [OnEventDoAction(typeof(ProposalResponse), nameof(ProposalResponseHandler))]
            internal class WaitingForAcks : ActorState
            {
            }
        }

        private class Acceptor : Actor
        {
            private string name;
            private Dictionary<string, ActorId> proposers;
            private Dictionary<string, ActorId> learners;

            private Proposal lastAckedProposal;

            private Proposal lastAcceptedProposal;
            private string acceptedValue;

            protected override int HashedState
            {
                get
                {
                    int hash = 37;
                    if (this.lastAckedProposal != null)
                    {
                        hash = (hash * 397) + this.lastAckedProposal.ProposerName.GetHashCode();
                        hash = (hash * 397) + this.lastAckedProposal.Id.GetHashCode();
                    }

                    if (this.lastAcceptedProposal != null)
                    {
                        hash = (hash * 397) + this.lastAcceptedProposal.ProposerName.GetHashCode();
                        hash = (hash * 397) + this.lastAcceptedProposal.Id.GetHashCode();
                    }

                    if (this.acceptedValue != null)
                    {
                        hash = (hash * 397) + this.acceptedValue.GetHashCode();
                    }

                    States.GetOrAdd(hash, true);

                    return hash;
                }
            }

            public void InitOnEntry()
            {
                var initEvent = (AcceptorSetupEvent)ReceivedEvent;

                this.name = initEvent.Name;
                this.proposers = initEvent.Proposers;
                this.learners = initEvent.Learners;
            }

            public void ProposalRequestHandler()
            {
                var proposalRequest = (ProposalRequest)ReceivedEvent;

                var proposer = proposalRequest.From;
                var proposal = proposalRequest.Proposal;

                if ((lastAckedProposal == null ||
                     proposal.GreaterThan(lastAckedProposal)))
                {
                    lastAckedProposal = proposal;
                    Send(proposer, new ProposalResponse(
                        this.Id,
                        proposal,
                        true /* acknowledged */,
                        this.lastAcceptedProposal,
                        this.acceptedValue));
                }
            }

            public void AcceptRequestHandler()
            {
                var acceptRequest = (AcceptRequest)ReceivedEvent;
                var proposal = acceptRequest.Proposal;
                var value = acceptRequest.Value;

                if (lastAckedProposal == null ||
                    !lastAckedProposal.Equals(proposal))
                {
                    return;
                }

                this.lastAcceptedProposal = proposal;
                this.acceptedValue = value;

                foreach (var learner in learners.Values)
                {
                    Send(learner, new ValueAcceptedEvent(this.Id, proposal, value));
                }
            }

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventDoAction(typeof(ProposalRequest), nameof(ProposalRequestHandler))]
            [OnEventDoAction(typeof(AcceptRequest), nameof(AcceptRequestHandler))]
            internal class Init : ActorState
            {
            }
        }

        private class Learner : Actor
        {
            private string name;
            private Dictionary<string, ActorId> acceptors;

            private Dictionary<ActorId, Proposal> acceptorToProposalMap = new Dictionary<ActorId, Proposal>();
            private Dictionary<Proposal, string> proposalToValueMap = new Dictionary<Proposal, string>();

            private string learnedValue = null;

            protected override int HashedState
            {
                get
                {
                    int hash = 37;
                    if (this.learnedValue != null)
                    {
                        hash = (hash * 397) + this.learnedValue.GetHashCode();
                    }

                    States.GetOrAdd(hash, true);

                    return hash;
                }
            }

            public void InitOnEntry()
            {
                var initEvent = (LearnerSetupEvent)ReceivedEvent;

                this.name = initEvent.Name;
                this.acceptors = initEvent.Acceptors;
            }

            public void ValueAcceptedEventHandler()
            {
                var acceptedEvent = (ValueAcceptedEvent)ReceivedEvent;

                var acceptor = acceptedEvent.Acceptor;
                var acceptedProposal = acceptedEvent.Proposal;
                var value = acceptedEvent.Value;

                acceptorToProposalMap[acceptor] = acceptedProposal;
                proposalToValueMap[acceptedProposal] = value;

                var proposalGroups = acceptorToProposalMap.Values.GroupBy(p => p);
                var majorityProposalList = proposalGroups.OrderByDescending(g => g.Count()).First();

                bool greaterThan50PercentObservations = (majorityProposalList.Count() / (double)acceptors.Count) > 0.5;
                if (greaterThan50PercentObservations)
                {
                    var majorityProposal = majorityProposalList.First();
                    var majorityValue = this.proposalToValueMap[majorityProposal];

                    if (this.learnedValue == null)
                    {
                        this.learnedValue = majorityValue;
                        Monitor<SafetyMonitor>(new ValueLearnedEvent());
                    }
                    else
                    {
                        if (this.learnedValue == majorityValue)
                        {
                            Monitor<SafetyMonitor>(new ValueLearnedEvent());
                        }
                        else
                        {
                            lock (BugsFoundLock)
                            {
                                BugsFound++;
                            }

                            // We instead count the BugsFount above.
                            // this.Assert(false, "Conflicting values learned");
                        }
                    }
                }
            }

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventDoAction(typeof(ValueAcceptedEvent), nameof(ValueAcceptedEventHandler))]
            internal class Init : ActorState
            {
            }
        }

        private class SafetyMonitor : Monitor
        {
            [Start]
            [OnEventGotoState(typeof(ValueLearnedEvent), typeof(ValueLearned))]
            internal class Init : MonitorState
            {
            }

            [OnEventGotoState(typeof(ValueLearnedEvent), typeof(ValueLearned))]
            internal class ValueLearned : MonitorState
            {
            }
        }
    }
}
