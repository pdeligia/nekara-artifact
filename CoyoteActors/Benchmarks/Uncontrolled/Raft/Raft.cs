﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Coyote;

namespace Benchmarks.Protocols
{
    internal class Raft
    {
        internal readonly static ConcurrentDictionary<int, bool> States = new ConcurrentDictionary<int, bool>();
        internal static int BugsFound = 0;
        internal static object BugsFoundLock = new object();

        public static void Execute(IActorRuntime runtime)
        {
            runtime.RegisterMonitor(typeof(SafetyMonitor));
            runtime.CreateActor(typeof(ClusterManager), "ClusterManager");
        }

        private class Log
        {
            public readonly int Term;
            public readonly int Command;

            public Log(int term, int command)
            {
                this.Term = term;
                this.Command = command;
            }
        }

        private class ClusterManager : Actor
        {
            internal class NotifyLeaderUpdate : Event
            {
                public ActorId Leader;
                public int Term;

                public NotifyLeaderUpdate(ActorId leader, int term)
                    : base()
                {
                    this.Leader = leader;
                    this.Term = term;
                }
            }

            internal class RedirectRequest : Event
            {
                public Event Request;

                public RedirectRequest(Event request)
                    : base()
                {
                    this.Request = request;
                }
            }

            internal class ShutDown : Event { }
            private class LocalEvent : Event { }

            ActorId[] Servers;
            int NumberOfServers;

            ActorId Leader;
            int LeaderTerm;

            int Counter;

            ActorId Client;

            [Start]
            [OnEntry(nameof(EntryOnInit))]
            [OnEventGotoState(typeof(LocalEvent), typeof(Configuring))]
            class Init : ActorState { }

            void EntryOnInit()
            {
                this.NumberOfServers = 5;
                this.LeaderTerm = 0;
                this.Counter = 0;

                this.Servers = new ActorId[this.NumberOfServers];

                for (int idx = 0; idx < this.NumberOfServers; idx++)
                {
                    this.Servers[idx] = this.CreateActor(typeof(Server), $"Server{idx}");
                }

                this.Client = this.CreateActor(typeof(Client), "Client");

                this.Raise(new LocalEvent());
            }

            [OnEntry(nameof(ConfiguringOnInit))]
            [OnEventGotoState(typeof(LocalEvent), typeof(Availability.Unavailable))]
            class Configuring : ActorState { }

            void ConfiguringOnInit()
            {
                for (int idx = 0; idx < this.NumberOfServers; idx++)
                {
                    this.Send(this.Servers[idx], new Server.ConfigureEvent(idx, this.Servers, this.Id));
                }

                this.Send(this.Client, new Client.ConfigureEvent(this.Id));

                this.Raise(new LocalEvent());
            }

            class Availability : StateGroup
            {
                [OnEventDoAction(typeof(NotifyLeaderUpdate), nameof(BecomeAvailable))]
                [OnEventDoAction(typeof(ShutDown), nameof(ShuttingDown))]
                [OnEventGotoState(typeof(LocalEvent), typeof(Available))]
                [DeferEvents(typeof(Client.Request))]
                public class Unavailable : ActorState { }


                [OnEventDoAction(typeof(Client.Request), nameof(SendClientRequestToLeader))]
                [OnEventDoAction(typeof(RedirectRequest), nameof(RedirectClientRequest))]
                [OnEventDoAction(typeof(NotifyLeaderUpdate), nameof(RefreshLeader))]
                [OnEventDoAction(typeof(ShutDown), nameof(ShuttingDown))]
                [OnEventGotoState(typeof(LocalEvent), typeof(Unavailable))]
                public class Available : ActorState { }
            }

            void BecomeAvailable()
            {
                this.UpdateLeader(this.ReceivedEvent as NotifyLeaderUpdate);
                this.Raise(new LocalEvent());
            }


            void SendClientRequestToLeader()
            {
                this.Send(this.Leader, this.ReceivedEvent);
            }

            void RedirectClientRequest()
            {
                if (this.Counter < 10)
                {
                    this.Send(this.Id, (this.ReceivedEvent as RedirectRequest).Request);
                }

                this.Counter++;
            }

            void RefreshLeader()
            {
                this.UpdateLeader(this.ReceivedEvent as NotifyLeaderUpdate);
            }

            void ShuttingDown()
            {
                for (int idx = 0; idx < this.NumberOfServers; idx++)
                {
                    this.Send(this.Servers[idx], new Server.ShutDown());
                }

                this.Raise(new Halt());
            }

            /// <summary>
            /// Updates the leader.
            /// </summary>
            /// <param name="request">NotifyLeaderUpdate</param>
            void UpdateLeader(NotifyLeaderUpdate request)
            {
                if (this.LeaderTerm < request.Term)
                {
                    this.Leader = request.Leader;
                    this.LeaderTerm = request.Term;
                }
            }
        }

        private class Server : Actor
        {
            /// <summary>
            /// Used to configure the server.
            /// </summary>
            public class ConfigureEvent : Event
            {
                public int Id;
                public ActorId[] Servers;
                public ActorId ClusterManager;

                public ConfigureEvent(int id, ActorId[] servers, ActorId manager)
                    : base()
                {
                    this.Id = id;
                    this.Servers = servers;
                    this.ClusterManager = manager;
                }
            }

            /// <summary>
            /// Initiated by candidates during elections.
            /// </summary>
            public class VoteRequest : Event
            {
                public int Term; // candidate’s term
                public ActorId CandidateId; // candidate requesting vote
                public int LastLogIndex; // index of candidate’s last log entry
                public int LastLogTerm; // term of candidate’s last log entry

                public VoteRequest(int term, ActorId candidateId, int lastLogIndex, int lastLogTerm)
                    : base()
                {
                    this.Term = term;
                    this.CandidateId = candidateId;
                    this.LastLogIndex = lastLogIndex;
                    this.LastLogTerm = lastLogTerm;
                }
            }

            /// <summary>
            /// Response to a vote request.
            /// </summary>
            public class VoteResponse : Event
            {
                public int Term; // currentTerm, for candidate to update itself
                public ActorId VoterId;
                public bool VoteGranted; // true means candidate received vote

                public VoteResponse(int term, ActorId voterId, bool voteGranted)
                    : base()
                {
                    this.Term = term;
                    this.VoterId = voterId;
                    this.VoteGranted = voteGranted;
                }
            }

            /// <summary>
            /// Initiated by leaders to replicate log entries and
            /// to provide a form of heartbeat.
            /// </summary>
            public class AppendEntriesRequest : Event
            {
                public int Term; // leader's term
                public ActorId LeaderId; // so follower can redirect clients
                public int PrevLogIndex; // index of log entry immediately preceding new ones
                public int PrevLogTerm; // term of PrevLogIndex entry
                public List<Log> Entries; // log entries to store (empty for heartbeat; may send more than one for efficiency)
                public int LeaderCommit; // leader’s CommitIndex

                public ActorId ReceiverEndpoint; // client

                public AppendEntriesRequest(int term, ActorId leaderId, int prevLogIndex,
                    int prevLogTerm, List<Log> entries, int leaderCommit, ActorId client)
                    : base()
                {
                    this.Term = term;
                    this.LeaderId = leaderId;
                    this.PrevLogIndex = prevLogIndex;
                    this.PrevLogTerm = prevLogTerm;
                    this.Entries = entries;
                    this.LeaderCommit = leaderCommit;
                    this.ReceiverEndpoint = client;
                }
            }

            /// <summary>
            /// Response to an append entries request.
            /// </summary>
            public class AppendEntriesResponse : Event
            {
                public int Term; // current Term, for leader to update itself
                public bool Success; // true if follower contained entry matching PrevLogIndex and PrevLogTerm

                public ActorId Server;
                public ActorId ReceiverEndpoint; // client

                public AppendEntriesResponse(int term, bool success, ActorId server, ActorId client)
                    : base()
                {
                    this.Term = term;
                    this.Success = success;
                    this.Server = server;
                    this.ReceiverEndpoint = client;
                }
            }

            // Events for transitioning a server between roles.
            private class BecomeFollower : Event { }
            private class BecomeCandidate : Event { }
            private class BecomeLeader : Event { }

            internal class ShutDown : Event { }

            /// <summary>
            /// The id of this server.
            /// </summary>
            int ServerId;

            /// <summary>
            /// The cluster manager actor.
            /// </summary>
            ActorId ClusterManager;

            /// <summary>
            /// The servers.
            /// </summary>
            ActorId[] Servers;

            /// <summary>
            /// Leader id.
            /// </summary>
            ActorId LeaderId;

            /// <summary>
            /// The election timer of this server.
            /// </summary>
            ActorId ElectionTimer;

            /// <summary>
            /// The periodic timer of this server.
            /// </summary>
            ActorId PeriodicTimer;

            /// <summary>
            /// Latest term server has seen (initialized to 0 on
            /// first boot, increases monotonically).
            /// </summary>
            int CurrentTerm;

            /// <summary>
            /// Candidate id that received vote in current term (or null if none).
            /// </summary>
            ActorId VotedFor;

            /// <summary>
            /// Log entries.
            /// </summary>
            List<Log> Logs;

            /// <summary>
            /// Index of highest log entry known to be committed (initialized
            /// to 0, increases monotonically).
            /// </summary>
            int CommitIndex;

            /// <summary>
            /// Index of highest log entry applied to state actor (initialized
            /// to 0, increases monotonically).
            /// </summary>
            int LastApplied;

            /// <summary>
            /// For each server, index of the next log entry to send to that
            /// server (initialized to leader last log index + 1).
            /// </summary>
            Dictionary<ActorId, int> NextIndex;

            /// <summary>
            /// For each server, index of highest log entry known to be replicated
            /// on server (initialized to 0, increases monotonically).
            /// </summary>
            Dictionary<ActorId, int> MatchIndex;

            /// <summary>
            /// Number of received votes.
            /// </summary>
            int VotesReceived;

            /// <summary>
            /// The latest client request.
            /// </summary>
            Client.Request LastClientRequest;

            /// <summary>
            /// Add custom hash
            /// </summary>
            protected override int HashedState
            {
                get
                {
                    unchecked
                    {
                        int hash = 37;

                        hash += (hash * 397) + (this.CurrentState?.FullName.GetHashCode() ?? "None".GetHashCode());
                        hash += (hash * 397) + (this.VotedFor?.GetHashCode() ?? "None".GetHashCode());
                        hash += (hash * 397) + VotesReceived.GetHashCode();

                        States.GetOrAdd(hash, true);

                        return hash;
                    }
                }
            }

            [Start]
            [OnEntry(nameof(EntryOnInit))]
            [OnEventDoAction(typeof(ConfigureEvent), nameof(Configure))]
            [OnEventGotoState(typeof(BecomeFollower), typeof(Follower))]
            [DeferEvents(typeof(VoteRequest), typeof(AppendEntriesRequest))]
            class Init : ActorState { }

            void EntryOnInit()
            {
                this.CurrentTerm = 0;

                this.LeaderId = null;
                this.VotedFor = null;

                this.Logs = new List<Log>();

                this.CommitIndex = 0;
                this.LastApplied = 0;

                this.NextIndex = new Dictionary<ActorId, int>();
                this.MatchIndex = new Dictionary<ActorId, int>();
            }

            void Configure()
            {
                this.ServerId = (this.ReceivedEvent as ConfigureEvent).Id;

                this.Servers = (this.ReceivedEvent as ConfigureEvent).Servers;
                this.ClusterManager = (this.ReceivedEvent as ConfigureEvent).ClusterManager;

                this.ElectionTimer = this.CreateActor(typeof(ElectionTimer), $"ElectionTimer{this.ServerId}");
                this.Send(this.ElectionTimer, new ElectionTimer.ConfigureEvent(this.Id));

                this.PeriodicTimer = this.CreateActor(typeof(PeriodicTimer), $"PeriodicTimer{this.ServerId}");
                this.Send(this.PeriodicTimer, new PeriodicTimer.ConfigureEvent(this.Id));

                this.Raise(new BecomeFollower());
            }

            [OnEntry(nameof(FollowerOnInit))]
            [OnEventDoAction(typeof(Client.Request), nameof(RedirectClientRequest))]
            [OnEventDoAction(typeof(VoteRequest), nameof(VoteAsFollower))]
            [OnEventDoAction(typeof(VoteResponse), nameof(RespondVoteAsFollower))]
            [OnEventDoAction(typeof(AppendEntriesRequest), nameof(AppendEntriesAsFollower))]
            [OnEventDoAction(typeof(AppendEntriesResponse), nameof(RespondAppendEntriesAsFollower))]
            [OnEventDoAction(typeof(ElectionTimer.TimeoutEvent), nameof(StartLeaderElection))]
            [OnEventDoAction(typeof(ShutDown), nameof(ShuttingDown))]
            [OnEventGotoState(typeof(BecomeFollower), typeof(Follower))]
            [OnEventGotoState(typeof(BecomeCandidate), typeof(Candidate))]
            [IgnoreEvents(typeof(PeriodicTimer.TimeoutEvent))]
            class Follower : ActorState { }

            void FollowerOnInit()
            {
                this.LeaderId = null;
                this.VotesReceived = 0;

                this.Send(this.ElectionTimer, new ElectionTimer.StartTimerEvent());
            }

            void RedirectClientRequest()
            {
                if (this.LeaderId != null)
                {
                    this.Send(this.LeaderId, this.ReceivedEvent);
                }
                else
                {
                    this.Send(this.ClusterManager, new ClusterManager.RedirectRequest(this.ReceivedEvent));
                }
            }

            void StartLeaderElection()
            {
                this.Raise(new BecomeCandidate());
            }

            void VoteAsFollower()
            {
                var request = this.ReceivedEvent as VoteRequest;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;
                }

                this.Vote(this.ReceivedEvent as VoteRequest);
            }

            void RespondVoteAsFollower()
            {
                var request = this.ReceivedEvent as VoteResponse;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;
                }
            }

            void AppendEntriesAsFollower()
            {
                var request = this.ReceivedEvent as AppendEntriesRequest;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;
                }

                this.AppendEntries(this.ReceivedEvent as AppendEntriesRequest);
            }

            void RespondAppendEntriesAsFollower()
            {
                var request = this.ReceivedEvent as AppendEntriesResponse;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;
                }
            }

            [OnEntry(nameof(CandidateOnInit))]
            [OnEventDoAction(typeof(Client.Request), nameof(RedirectClientRequest))]
            [OnEventDoAction(typeof(VoteRequest), nameof(VoteAsCandidate))]
            [OnEventDoAction(typeof(VoteResponse), nameof(RespondVoteAsCandidate))]
            [OnEventDoAction(typeof(AppendEntriesRequest), nameof(AppendEntriesAsCandidate))]
            [OnEventDoAction(typeof(AppendEntriesResponse), nameof(RespondAppendEntriesAsCandidate))]
            [OnEventDoAction(typeof(ElectionTimer.TimeoutEvent), nameof(StartLeaderElection))]
            [OnEventDoAction(typeof(PeriodicTimer.TimeoutEvent), nameof(BroadcastVoteRequests))]
            [OnEventDoAction(typeof(ShutDown), nameof(ShuttingDown))]
            [OnEventGotoState(typeof(BecomeLeader), typeof(Leader))]
            [OnEventGotoState(typeof(BecomeFollower), typeof(Follower))]
            [OnEventGotoState(typeof(BecomeCandidate), typeof(Candidate))]
            class Candidate : ActorState { }

            void CandidateOnInit()
            {
                this.CurrentTerm++;
                this.VotedFor = this.Id;
                this.VotesReceived = 1;

                this.Send(this.ElectionTimer, new ElectionTimer.StartTimerEvent());

                this.Logger.WriteLine("\n [Candidate] " + this.ServerId + " | term " + this.CurrentTerm +
                    " | election votes " + this.VotesReceived + " | log " + this.Logs.Count + "\n");

                this.BroadcastVoteRequests();
            }

            void BroadcastVoteRequests()
            {
                // BUG: duplicate votes from same follower
                this.Send(this.PeriodicTimer, new PeriodicTimer.StartTimerEvent());

                for (int idx = 0; idx < this.Servers.Length; idx++)
                {
                    if (idx == this.ServerId)
                        continue;

                    var lastLogIndex = this.Logs.Count;
                    var lastLogTerm = this.GetLogTermForIndex(lastLogIndex);

                    this.Send(this.Servers[idx], new VoteRequest(this.CurrentTerm, this.Id,
                        lastLogIndex, lastLogTerm));
                }
            }

            void VoteAsCandidate()
            {
                var request = this.ReceivedEvent as VoteRequest;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;
                    this.Vote(this.ReceivedEvent as VoteRequest);
                    this.Raise(new BecomeFollower());
                }
                else
                {
                    this.Vote(this.ReceivedEvent as VoteRequest);
                }
            }

            void RespondVoteAsCandidate()
            {
                var request = this.ReceivedEvent as VoteResponse;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;
                    this.Raise(new BecomeFollower());
                    return;
                }
                else if (request.Term != this.CurrentTerm)
                {
                    return;
                }

                if (request.VoteGranted)
                {
                    this.VotesReceived++;
                    if (this.VotesReceived >= (this.Servers.Length / 2) + 1)
                    {
                        this.Logger.WriteLine("\n [Leader] " + this.ServerId + " | term " + this.CurrentTerm +
                            " | election votes " + this.VotesReceived + " | log " + this.Logs.Count + "\n");
                        this.VotesReceived = 0;
                        this.Raise(new BecomeLeader());
                    }
                }
            }

            void AppendEntriesAsCandidate()
            {
                var request = this.ReceivedEvent as AppendEntriesRequest;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;
                    this.AppendEntries(this.ReceivedEvent as AppendEntriesRequest);
                    this.Raise(new BecomeFollower());
                }
                else
                {
                    this.AppendEntries(this.ReceivedEvent as AppendEntriesRequest);
                }
            }

            void RespondAppendEntriesAsCandidate()
            {
                var request = this.ReceivedEvent as AppendEntriesResponse;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;
                    this.Raise(new BecomeFollower());
                }
            }

            [OnEntry(nameof(LeaderOnInit))]
            [OnEventDoAction(typeof(Client.Request), nameof(ProcessClientRequest))]
            [OnEventDoAction(typeof(VoteRequest), nameof(VoteAsLeader))]
            [OnEventDoAction(typeof(VoteResponse), nameof(RespondVoteAsLeader))]
            [OnEventDoAction(typeof(AppendEntriesRequest), nameof(AppendEntriesAsLeader))]
            [OnEventDoAction(typeof(AppendEntriesResponse), nameof(RespondAppendEntriesAsLeader))]
            [OnEventDoAction(typeof(ShutDown), nameof(ShuttingDown))]
            [OnEventGotoState(typeof(BecomeFollower), typeof(Follower))]
            [IgnoreEvents(typeof(ElectionTimer.TimeoutEvent), typeof(PeriodicTimer.TimeoutEvent))]
            class Leader : ActorState { }

            void LeaderOnInit()
            {
                this.Monitor<SafetyMonitor>(new SafetyMonitor.NotifyLeaderElected(this.CurrentTerm));
                this.Send(this.ClusterManager, new ClusterManager.NotifyLeaderUpdate(this.Id, this.CurrentTerm));

                var logIndex = this.Logs.Count;
                var logTerm = this.GetLogTermForIndex(logIndex);

                this.NextIndex.Clear();
                this.MatchIndex.Clear();
                for (int idx = 0; idx < this.Servers.Length; idx++)
                {
                    if (idx == this.ServerId)
                        continue;
                    this.NextIndex.Add(this.Servers[idx], logIndex + 1);
                    this.MatchIndex.Add(this.Servers[idx], 0);
                }

                for (int idx = 0; idx < this.Servers.Length; idx++)
                {
                    if (idx == this.ServerId)
                        continue;
                    this.Send(this.Servers[idx], new AppendEntriesRequest(this.CurrentTerm, this.Id,
                        logIndex, logTerm, new List<Log>(), this.CommitIndex, null));
                }
            }

            void ProcessClientRequest()
            {
                this.LastClientRequest = this.ReceivedEvent as Client.Request;

                var log = new Log(this.CurrentTerm, this.LastClientRequest.Command);
                this.Logs.Add(log);

                this.BroadcastLastClientRequest();
            }

            void BroadcastLastClientRequest()
            {
                this.Logger.WriteLine("\n [Leader] " + this.ServerId + " sends append requests | term " +
                    this.CurrentTerm + " | log " + this.Logs.Count + "\n");

                var lastLogIndex = this.Logs.Count;

                this.VotesReceived = 1;
                for (int idx = 0; idx < this.Servers.Length; idx++)
                {
                    if (idx == this.ServerId)
                        continue;

                    var server = this.Servers[idx];
                    if (lastLogIndex < this.NextIndex[server])
                        continue;

                    var logs = this.Logs.GetRange(this.NextIndex[server] - 1,
                        this.Logs.Count - (this.NextIndex[server] - 1));

                    var prevLogIndex = this.NextIndex[server] - 1;
                    var prevLogTerm = this.GetLogTermForIndex(prevLogIndex);

                    this.Send(server, new AppendEntriesRequest(this.CurrentTerm, this.Id, prevLogIndex,
                        prevLogTerm, logs, this.CommitIndex, this.LastClientRequest.Client));
                }
            }

            void VoteAsLeader()
            {
                var request = this.ReceivedEvent as VoteRequest;

                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;

                    this.RedirectLastClientRequestToClusterManager();
                    this.Vote(this.ReceivedEvent as VoteRequest);

                    this.Raise(new BecomeFollower());
                }
                else
                {
                    this.Vote(this.ReceivedEvent as VoteRequest);
                }
            }

            void RespondVoteAsLeader()
            {
                var request = this.ReceivedEvent as VoteResponse;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;

                    this.RedirectLastClientRequestToClusterManager();
                    this.Raise(new BecomeFollower());
                }
            }

            void AppendEntriesAsLeader()
            {
                var request = this.ReceivedEvent as AppendEntriesRequest;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;

                    this.RedirectLastClientRequestToClusterManager();
                    this.AppendEntries(this.ReceivedEvent as AppendEntriesRequest);

                    this.Raise(new BecomeFollower());
                }
            }

            void RespondAppendEntriesAsLeader()
            {
                var request = this.ReceivedEvent as AppendEntriesResponse;
                if (request.Term > this.CurrentTerm)
                {
                    this.CurrentTerm = request.Term;
                    this.VotedFor = null;

                    this.RedirectLastClientRequestToClusterManager();
                    this.Raise(new BecomeFollower());
                    return;
                }
                else if (request.Term != this.CurrentTerm)
                {
                    return;
                }

                if (request.Success)
                {
                    this.NextIndex[request.Server] = this.Logs.Count + 1;
                    this.MatchIndex[request.Server] = this.Logs.Count;

                    this.VotesReceived++;
                    if (request.ReceiverEndpoint != null &&
                        this.VotesReceived >= (this.Servers.Length / 2) + 1)
                    {
                        this.Logger.WriteLine("\n [Leader] " + this.ServerId + " | term " + this.CurrentTerm +
                            " | append votes " + this.VotesReceived + " | append success\n");

                        var commitIndex = this.MatchIndex[request.Server];
                        if (commitIndex > this.CommitIndex &&
                            this.Logs[commitIndex - 1].Term == this.CurrentTerm)
                        {
                            this.CommitIndex = commitIndex;

                            this.Logger.WriteLine("\n [Leader] " + this.ServerId + " | term " + this.CurrentTerm +
                                " | log " + this.Logs.Count + " | command " + this.Logs[commitIndex - 1].Command + "\n");
                        }

                        this.VotesReceived = 0;
                        this.LastClientRequest = null;

                        this.Send(request.ReceiverEndpoint, new Client.Response());
                    }
                }
                else
                {
                    if (this.NextIndex[request.Server] > 1)
                    {
                        this.NextIndex[request.Server] = this.NextIndex[request.Server] - 1;
                    }

                    var logs = this.Logs.GetRange(this.NextIndex[request.Server] - 1,
                        this.Logs.Count - (this.NextIndex[request.Server] - 1));

                    var prevLogIndex = this.NextIndex[request.Server] - 1;
                    var prevLogTerm = this.GetLogTermForIndex(prevLogIndex);

                    this.Logger.WriteLine("\n [Leader] " + this.ServerId + " | term " + this.CurrentTerm + " | log " +
                        this.Logs.Count + " | append votes " + this.VotesReceived +
                        " | append fail (next idx = " + this.NextIndex[request.Server] + ")\n");

                    this.Send(request.Server, new AppendEntriesRequest(this.CurrentTerm, this.Id, prevLogIndex,
                        prevLogTerm, logs, this.CommitIndex, request.ReceiverEndpoint));
                }
            }

            /// <summary>
            /// Processes the given vote request.
            /// </summary>
            /// <param name="request">VoteRequest</param>
            void Vote(VoteRequest request)
            {
                var lastLogIndex = this.Logs.Count;
                var lastLogTerm = this.GetLogTermForIndex(lastLogIndex);

                if (request.Term < this.CurrentTerm ||
                    (this.VotedFor != null && this.VotedFor != request.CandidateId) ||
                    lastLogIndex > request.LastLogIndex ||
                    lastLogTerm > request.LastLogTerm)
                {
                    this.Logger.WriteLine("\n [Server] " + this.ServerId + " | term " + this.CurrentTerm +
                        " | log " + this.Logs.Count + " | vote false\n");
                    this.Send(request.CandidateId, new VoteResponse(this.CurrentTerm, this.Id, false));
                }
                else
                {
                    this.Logger.WriteLine("\n [Server] " + this.ServerId + " | term " + this.CurrentTerm +
                        " | log " + this.Logs.Count + " | vote true\n");

                    this.VotedFor = request.CandidateId;
                    this.LeaderId = null;

                    this.Send(request.CandidateId, new VoteResponse(this.CurrentTerm, this.Id, true));
                }
            }

            /// <summary>
            /// Processes the given append entries request.
            /// </summary>
            /// <param name="request">AppendEntriesRequest</param>
            void AppendEntries(AppendEntriesRequest request)
            {
                if (request.Term < this.CurrentTerm)
                {
                    this.Logger.WriteLine("\n [Server] " + this.ServerId + " | term " + this.CurrentTerm + " | log " +
                        this.Logs.Count + " | last applied: " + this.LastApplied + " | append false (< term)\n");

                    this.Send(request.LeaderId, new AppendEntriesResponse(this.CurrentTerm, false,
                        this.Id, request.ReceiverEndpoint));
                }
                else
                {
                    if (request.PrevLogIndex > 0 &&
                        (this.Logs.Count < request.PrevLogIndex ||
                        this.Logs[request.PrevLogIndex - 1].Term != request.PrevLogTerm))
                    {
                        this.Logger.WriteLine("\n [Server] " + this.ServerId + " | term " + this.CurrentTerm + " | log " +
                            this.Logs.Count + " | last applied: " + this.LastApplied + " | append false (not in log)\n");

                        this.Send(request.LeaderId, new AppendEntriesResponse(this.CurrentTerm,
                            false, this.Id, request.ReceiverEndpoint));
                    }
                    else
                    {
                        if (request.Entries.Count > 0)
                        {
                            var currentIndex = request.PrevLogIndex + 1;
                            foreach (var entry in request.Entries)
                            {
                                if (this.Logs.Count < currentIndex)
                                {
                                    this.Logs.Add(entry);
                                }
                                else if (this.Logs[currentIndex - 1].Term != entry.Term)
                                {
                                    this.Logs.RemoveRange(currentIndex - 1, this.Logs.Count - (currentIndex - 1));
                                    this.Logs.Add(entry);
                                }

                                currentIndex++;
                            }
                        }

                        if (request.LeaderCommit > this.CommitIndex &&
                            this.Logs.Count < request.LeaderCommit)
                        {
                            this.CommitIndex = this.Logs.Count;
                        }
                        else if (request.LeaderCommit > this.CommitIndex)
                        {
                            this.CommitIndex = request.LeaderCommit;
                        }

                        if (this.CommitIndex > this.LastApplied)
                        {
                            this.LastApplied++;
                        }

                        this.Logger.WriteLine("\n [Server] " + this.ServerId + " | term " + this.CurrentTerm + " | log " +
                            this.Logs.Count + " | entries received " + request.Entries.Count + " | last applied " +
                            this.LastApplied + " | append true\n");

                        this.LeaderId = request.LeaderId;
                        this.Send(request.LeaderId, new AppendEntriesResponse(this.CurrentTerm,
                            true, this.Id, request.ReceiverEndpoint));
                    }
                }
            }

            void RedirectLastClientRequestToClusterManager()
            {
                if (this.LastClientRequest != null)
                {
                    this.Send(this.ClusterManager, this.LastClientRequest);
                }
            }

            /// <summary>
            /// Returns the log term for the given log index.
            /// </summary>
            /// <param name="logIndex">Index</param>
            /// <returns>Term</returns>
            int GetLogTermForIndex(int logIndex)
            {
                var logTerm = 0;
                if (logIndex > 0)
                {
                    logTerm = this.Logs[logIndex - 1].Term;
                }

                return logTerm;
            }

            void ShuttingDown()
            {
                this.Send(this.ElectionTimer, new Halt());
                this.Send(this.PeriodicTimer, new Halt());

                this.Raise(new Halt());
            }
        }

        private class Client : Actor
        {

            /// <summary>
            /// Used to configure the client.
            /// </summary>
            public class ConfigureEvent : Event
            {
                public ActorId Cluster;

                public ConfigureEvent(ActorId cluster)
                    : base()
                {
                    this.Cluster = cluster;
                }
            }

            /// <summary>
            /// Used for a client request.
            /// </summary>
            internal class Request : Event
            {
                public ActorId Client;
                public int Command;

                public Request(ActorId client, int command)
                    : base()
                {
                    this.Client = client;
                    this.Command = command;
                }
            }

            internal class Response : Event { }

            private class LocalEvent : Event { }

            ActorId Cluster;

            int LatestCommand;
            int Counter;

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventDoAction(typeof(ConfigureEvent), nameof(Configure))]
            [OnEventGotoState(typeof(LocalEvent), typeof(PumpRequest))]
            class Init : ActorState { }

            void InitOnEntry()
            {
                this.LatestCommand = -1;
                this.Counter = 0;
            }

            void Configure()
            {
                this.Cluster = (this.ReceivedEvent as ConfigureEvent).Cluster;
                this.Raise(new LocalEvent());
            }

            [OnEntry(nameof(PumpRequestOnEntry))]
            [OnEventDoAction(typeof(Response), nameof(ProcessResponse))]
            [OnEventGotoState(typeof(LocalEvent), typeof(PumpRequest))]
            class PumpRequest : ActorState { }

            void PumpRequestOnEntry()
            {
                this.LatestCommand = this.RandomInteger(5); //new Random().Next(100);
                this.Counter++;

                this.Logger.WriteLine("\n [Client] new request " + this.LatestCommand + "\n");

                this.Send(this.Cluster, new Request(this.Id, this.LatestCommand));
            }

            void ProcessResponse()
            {
                if (this.Counter == 1)
                {
                    this.Send(this.Cluster, new ClusterManager.ShutDown());
                    this.Raise(new Halt());
                }
                else
                {
                    this.Raise(new LocalEvent());
                }
            }
        }

        private class ElectionTimer : Actor
        {
            internal class ConfigureEvent : Event
            {
                public ActorId Target;

                public ConfigureEvent(ActorId id)
                    : base()
                {
                    this.Target = id;
                }
            }

            internal class StartTimerEvent : Event { }
            internal class CancelTimerEvent : Event { }
            internal class TimeoutEvent : Event { }

            private class TickEvent : Event { }

            ActorId Target;
            int Counter;

            [Start]
            [OnEventDoAction(typeof(ConfigureEvent), nameof(Configure))]
            [OnEventGotoState(typeof(StartTimerEvent), typeof(Active))]
            class Init : ActorState { }

            void Configure()
            {
                this.Target = (this.ReceivedEvent as ConfigureEvent).Target;
                this.Counter = 0;
                //this.Raise(new StartTimerEvent());
            }

            [OnEntry(nameof(ActiveOnEntry))]
            [OnEventDoAction(typeof(TickEvent), nameof(Tick))]
            [OnEventGotoState(typeof(CancelTimerEvent), typeof(Inactive))]
            [IgnoreEvents(typeof(StartTimerEvent))]
            class Active : ActorState { }

            void ActiveOnEntry()
            {
                this.Send(this.Id, new TickEvent());
            }

            void Tick()
            {
                int threshold = 2;
                this.Counter++;

                if (this.Counter == threshold)
                {
                    this.Logger.WriteLine("\n [ElectionTimer] " + this.Target + " | timed out\n");
                    this.Send(this.Target, new TimeoutEvent());
                    this.Counter = 0;
                }

                if (this.Random())
                {
                    this.Send(this.Id, new TickEvent());
                }
                else
                {
                    // this.Raise(new CancelTimerEvent());
                    this.Raise(new Halt());
                }
            }

            // [OnEventGotoState(typeof(StartTimerEvent), typeof(Active))]
            [IgnoreEvents(typeof(StartTimerEvent), typeof(CancelTimerEvent), typeof(TickEvent))]
            class Inactive : ActorState { }
        }

        private class PeriodicTimer : Actor
        {
            internal class ConfigureEvent : Event
            {
                public ActorId Target;

                public ConfigureEvent(ActorId id)
                    : base()
                {
                    this.Target = id;
                }
            }

            internal class StartTimerEvent : Event { }
            internal class CancelTimerEvent : Event { }
            internal class TimeoutEvent : Event { }

            private class TickEvent : Event { }

            ActorId Target;

            int Count;

            [Start]
            [OnEventDoAction(typeof(ConfigureEvent), nameof(Configure))]
            [OnEventGotoState(typeof(StartTimerEvent), typeof(Active))]
            class Init : ActorState { }

            void Configure()
            {
                this.Target = (this.ReceivedEvent as ConfigureEvent).Target;
                //this.Raise(new StartTimerEvent());
            }

            [OnEntry(nameof(ActiveOnEntry))]
            [OnEventDoAction(typeof(TickEvent), nameof(Tick))]
            [OnEventGotoState(typeof(CancelTimerEvent), typeof(Inactive))]
            [IgnoreEvents(typeof(StartTimerEvent))]
            class Active : ActorState { }

            void ActiveOnEntry()
            {
                this.Count = 0;
                this.Send(this.Id, new TickEvent());
            }

            void Tick()
            {
                if (this.Random())
                {
                    this.Logger.WriteLine("\n [PeriodicTimer] " + this.Target + " | timed out\n");
                    this.Send(this.Target, new TimeoutEvent());
                }

                this.Send(this.Id, new TickEvent());

                if (this.Count is 10)
                {
                    // this.Raise(new CancelTimerEvent());
                    this.Raise(new Halt());
                }

                this.Count++;
            }

            // [OnEventGotoState(typeof(StartTimerEvent), typeof(Active))]
            [IgnoreEvents(typeof(StartTimerEvent), typeof(CancelTimerEvent), typeof(TickEvent))]
            class Inactive : ActorState { }
        }

        private class SafetyMonitor : Microsoft.Coyote.Monitor
        {
            internal class NotifyLeaderElected : Event
            {
                public int Term;

                public NotifyLeaderElected(int term)
                    : base()
                {
                    this.Term = term;
                }
            }

            private class LocalEvent : Event { }

            //unused: private int CurrentTerm;
            private HashSet<int> TermsWithLeader;

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventGotoState(typeof(LocalEvent), typeof(Monitoring))]
            class Init : MonitorState { }

            void InitOnEntry()
            {
                //this.CurrentTerm = -1;
                this.TermsWithLeader = new HashSet<int>();
                this.Raise(new LocalEvent());
            }

            [OnEventDoAction(typeof(NotifyLeaderElected), nameof(ProcessLeaderElected))]
            class Monitoring : MonitorState { }

            void ProcessLeaderElected()
            {
                var term = (this.ReceivedEvent as NotifyLeaderElected).Term;

                if (this.TermsWithLeader.Contains(term))
                {
                    lock (BugsFoundLock)
                    {
                        BugsFound++;
                    }
                }

                this.Assert(!this.TermsWithLeader.Contains(term), "Detected more than one leader in term " + term);
                this.TermsWithLeader.Add(term);
            }
        }
    }
}
