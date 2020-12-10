﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CoyoteActors;

namespace Benchmarks
{
    internal class ChainReplication
    {
        internal readonly static ConcurrentDictionary<int, bool> States = new ConcurrentDictionary<int, bool>();
        internal static int BugsFound = 0;
        internal static object BugsFoundLock = new object();

        public static void Execute(IActorRuntime runtime)
        {
            runtime.RegisterMonitor(typeof(InvariantMonitor));
            runtime.RegisterMonitor(typeof(ServerResponseSeqMonitor));
            runtime.CreateActor(typeof(Environment));
        }

        private class SentLog
        {
            public int NextSeqId;
            public ActorId Client;
            public int Key;
            public int Value;

            public SentLog(int nextSeqId, ActorId client, int key, int val)
            {
                this.NextSeqId = nextSeqId;
                this.Client = client;
                this.Key = key;
                this.Value = val;
            }
        }

        private class Environment : Actor
        {
            private List<ActorId> Servers;
            private List<ActorId> Clients;
            private int NumOfServers;

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            private class Init : ActorState
            {
            }

            private void InitOnEntry()
            {
                this.Servers = new List<ActorId>();
                this.Clients = new List<ActorId>();

                this.NumOfServers = 3;

                for (int i = 0; i < this.NumOfServers; i++)
                {
                    ActorId server;

                    if (i is 0)
                    {
                        server = this.CreateActor(
                            typeof(ChainReplicationServer),
                            new ChainReplicationServer.SetupEvent(i, true, false));
                    }
                    else if (i == this.NumOfServers - 1)
                    {
                        server = this.CreateActor(
                            typeof(ChainReplicationServer),
                            new ChainReplicationServer.SetupEvent(i, false, true));
                    }
                    else
                    {
                        server = this.CreateActor(
                            typeof(ChainReplicationServer),
                            new ChainReplicationServer.SetupEvent(i, false, false));
                    }

                    this.Servers.Add(server);
                }

                this.Monitor<InvariantMonitor>(
                    new InvariantMonitor.SetupEvent(this.Servers));
                this.Monitor<ServerResponseSeqMonitor>(
                    new ServerResponseSeqMonitor.SetupEvent(this.Servers));

                for (int i = 0; i < this.NumOfServers; i++)
                {
                    ActorId pred;
                    ActorId succ;

                    if (i > 0)
                    {
                        pred = this.Servers[i - 1];
                    }
                    else
                    {
                        pred = this.Servers[0];
                    }

                    if (i < this.NumOfServers - 1)
                    {
                        succ = this.Servers[i + 1];
                    }
                    else
                    {
                        succ = this.Servers[this.NumOfServers - 1];
                    }

                    this.Send(this.Servers[i], new ChainReplicationServer.PredSucc(pred, succ));
                }

                this.Clients.Add(this.CreateActor(typeof(Client),
                    new Client.SetupEvent(0, this.Servers[0], this.Servers[this.NumOfServers - 1], 1)));

                this.Clients.Add(this.CreateActor(typeof(Client),
                    new Client.SetupEvent(1, this.Servers[0], this.Servers[this.NumOfServers - 1], 100)));

                this.CreateActor(typeof(ChainReplicationMaster),
                    new ChainReplicationMaster.SetupEvent(this.Servers, this.Clients));

                this.Send(this.Id, new Halt());
            }
        }

        private class FailureDetector : Actor
        {
            internal class SetupEvent : Event
            {
                public ActorId Main;
                public List<ActorId> Servers;

                public SetupEvent(ActorId main, List<ActorId> servers)
                    : base()
                {
                    this.Main = main;
                    this.Servers = servers;
                }
            }

            internal class FailureDetected : Event
            {
                public ActorId Server;

                public FailureDetected(ActorId server)
                    : base()
                {
                    this.Server = server;
                }
            }

            internal class FailureCorrected : Event
            {
                public List<ActorId> Servers;

                public FailureCorrected(List<ActorId> servers)
                    : base()
                {
                    this.Servers = servers;
                }
            }

            internal class Ping : Event
            {
                public ActorId Target;

                public Ping(ActorId target)
                    : base()
                {
                    this.Target = target;
                }
            }

            internal class Pong : Event
            {
            }

            private class InjectFailure : Event
            {
            }

            private class Local : Event
            {
            }

            private ActorId Main;
            private List<ActorId> Servers;

            private int CheckNodeIdx;
            private int Failures;

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(StartMonitoring))]
            private class Init : ActorState
            {
            }

            private void InitOnEntry()
            {
                Event e = this.ReceivedEvent;
                this.Main = (e as SetupEvent).Main;
                this.Servers = (e as SetupEvent).Servers;

                this.CheckNodeIdx = 0;
                this.Failures = 100;

                this.Raise(new Local());
            }

            [OnEntry(nameof(StartMonitoringOnEntry))]
            [OnEventGotoState(typeof(Pong), typeof(StartMonitoring), nameof(HandlePong))]
            [OnEventGotoState(typeof(InjectFailure), typeof(HandleFailure))]
            private class StartMonitoring : ActorState
            {
            }

            private void StartMonitoringOnEntry()
            {
                if (this.Failures < 1)
                {
                    this.Raise(new Halt());
                }
                else
                {
                    this.Send(this.Servers[this.CheckNodeIdx], new Ping(this.Id));

                    if (this.Servers.Count > 1)
                    {
                        if (this.Random())
                        {
                            this.Send(this.Id, new InjectFailure());
                        }
                        else
                        {
                            this.Send(this.Id, new Pong());
                        }
                    }
                    else
                    {
                        this.Send(this.Id, new Pong());
                    }

                    this.Failures--;
                }
            }

            private void HandlePong()
            {
                this.CheckNodeIdx++;
                if (this.CheckNodeIdx == this.Servers.Count)
                {
                    this.CheckNodeIdx = 0;
                }
            }

            [OnEntry(nameof(HandleFailureOnEntry))]
            [OnEventGotoState(typeof(FailureCorrected), typeof(StartMonitoring), nameof(ProcessFailureCorrected))]
            [IgnoreEvents(typeof(Pong), typeof(InjectFailure))]
            private class HandleFailure : ActorState
            {
            }

            private void HandleFailureOnEntry()
            {
                this.Send(this.Main, new FailureDetected(this.Servers[this.CheckNodeIdx]));
            }

            private void ProcessFailureCorrected()
            {
                Event e = this.ReceivedEvent;
                this.CheckNodeIdx = 0;
                this.Servers = (e as FailureCorrected).Servers;
            }
        }

        private class ChainReplicationMaster : Actor
        {
            internal class SetupEvent : Event
            {
                public List<ActorId> Servers;
                public List<ActorId> Clients;

                public SetupEvent(List<ActorId> servers, List<ActorId> clients)
                    : base()
                {
                    this.Servers = servers;
                    this.Clients = clients;
                }
            }

            internal class BecomeHead : Event
            {
                public ActorId Target;

                public BecomeHead(ActorId target)
                    : base()
                {
                    this.Target = target;
                }
            }

            internal class BecomeTail : Event
            {
                public ActorId Target;

                public BecomeTail(ActorId target)
                    : base()
                {
                    this.Target = target;
                }
            }

            internal class Success : Event
            {
            }

            internal class HeadChanged : Event
            {
            }

            internal class TailChanged : Event
            {
            }

            private class HeadFailed : Event
            {
            }

            private class TailFailed : Event
            {
            }

            private class ServerFailed : Event
            {
            }

            private class FixSuccessor : Event
            {
            }

            private class FixPredecessor : Event
            {
            }

            private class Local : Event
            {
            }

            private class Done : Event
            {
            }

            private List<ActorId> Servers;
            private List<ActorId> Clients;

            private ActorId FailureDetector;

            private ActorId Head;
            private ActorId Tail;

            private int FaultyNodeIndex;
            private int LastUpdateReceivedSucc;
            private int LastAckSent;

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(WaitForFailure))]
            private class Init : ActorState
            {
            }

            private void InitOnEntry()
            {
                Event e = this.ReceivedEvent;
                this.Servers = (e as SetupEvent).Servers;
                this.Clients = (e as SetupEvent).Clients;

                this.FailureDetector = this.CreateActor(
                    typeof(FailureDetector),
                    new FailureDetector.SetupEvent(this.Id, this.Servers));

                this.Head = this.Servers[0];
                this.Tail = this.Servers[this.Servers.Count - 1];

                this.Raise(new Local());
            }

            [OnEventGotoState(typeof(HeadFailed), typeof(CorrectHeadFailure))]
            [OnEventGotoState(typeof(TailFailed), typeof(CorrectTailFailure))]
            [OnEventGotoState(typeof(ServerFailed), typeof(CorrectServerFailure))]
            [OnEventDoAction(typeof(FailureDetector.FailureDetected), nameof(CheckWhichNodeFailed))]
            private class WaitForFailure : ActorState
            {
            }

            private void CheckWhichNodeFailed()
            {
                Event e = this.ReceivedEvent;
                this.Assert(this.Servers.Count > 1, "All nodes have failed.");

                var failedServer = (e as FailureDetector.FailureDetected).Server;

                if (this.Head.Equals(failedServer))
                {
                    this.Raise(new HeadFailed());
                }
                else if (this.Tail.Equals(failedServer))
                {
                    this.Raise(new TailFailed());
                }
                else
                {
                    for (int i = 0; i < this.Servers.Count - 1; i++)
                    {
                        if (this.Servers[i].Equals(failedServer))
                        {
                            this.FaultyNodeIndex = i;
                        }
                    }

                    this.Raise(new ServerFailed());
                }
            }

            [OnEntry(nameof(CorrectHeadFailureOnEntry))]
            [OnEventGotoState(typeof(Done), typeof(WaitForFailure), nameof(UpdateFailureDetector))]
            [OnEventDoAction(typeof(HeadChanged), nameof(UpdateClients))]
            private class CorrectHeadFailure : ActorState
            {
            }

            private void CorrectHeadFailureOnEntry()
            {
                this.Servers.RemoveAt(0);

                this.Monitor<InvariantMonitor>(
                    new InvariantMonitor.UpdateServers(this.Servers));
                this.Monitor<ServerResponseSeqMonitor>(
                    new ServerResponseSeqMonitor.UpdateServers(this.Servers));

                this.Head = this.Servers[0];

                this.Send(this.Head, new BecomeHead(this.Id));
            }

            private void UpdateClients()
            {
                for (int i = 0; i < this.Clients.Count; i++)
                {
                    this.Send(this.Clients[i], new Client.UpdateHeadTail(this.Head, this.Tail));
                }

                this.Raise(new Done());
            }

            private void UpdateFailureDetector()
            {
                this.Send(this.FailureDetector, new FailureDetector.FailureCorrected(this.Servers));
            }

            [OnEntry(nameof(CorrectTailFailureOnEntry))]
            [OnEventGotoState(typeof(Done), typeof(WaitForFailure), nameof(UpdateFailureDetector))]
            [OnEventDoAction(typeof(TailChanged), nameof(UpdateClients))]
            private class CorrectTailFailure : ActorState
            {
            }

            private void CorrectTailFailureOnEntry()
            {
                this.Servers.RemoveAt(this.Servers.Count - 1);

                this.Monitor<InvariantMonitor>(
                    new InvariantMonitor.UpdateServers(this.Servers));
                this.Monitor<ServerResponseSeqMonitor>(
                    new ServerResponseSeqMonitor.UpdateServers(this.Servers));

                this.Tail = this.Servers[this.Servers.Count - 1];

                this.Send(this.Tail, new BecomeTail(this.Id));
            }

            [OnEntry(nameof(CorrectServerFailureOnEntry))]
            [OnEventGotoState(typeof(Done), typeof(WaitForFailure), nameof(UpdateFailureDetector))]
            [OnEventDoAction(typeof(FixSuccessor), nameof(UpdateClients))]
            [OnEventDoAction(typeof(FixPredecessor), nameof(ProcessFixPredecessor))]
            [OnEventDoAction(typeof(ChainReplicationServer.NewSuccInfo), nameof(SetLastUpdate))]
            [OnEventDoAction(typeof(Success), nameof(ProcessSuccess))]
            private class CorrectServerFailure : ActorState
            {
            }

            private void CorrectServerFailureOnEntry()
            {
                this.Servers.RemoveAt(this.FaultyNodeIndex);

                this.Monitor<InvariantMonitor>(
                    new InvariantMonitor.UpdateServers(this.Servers));
                this.Monitor<ServerResponseSeqMonitor>(
                    new ServerResponseSeqMonitor.UpdateServers(this.Servers));

                this.Raise(new FixSuccessor());
            }

            private void ProcessFixPredecessor()
            {
                this.Send(this.Servers[this.FaultyNodeIndex - 1], new ChainReplicationServer.NewSuccessor(
                    this.Id, this.Servers[this.FaultyNodeIndex], this.LastAckSent, this.LastUpdateReceivedSucc));
            }

            private void SetLastUpdate()
            {
                Event e = this.ReceivedEvent;
                this.LastUpdateReceivedSucc = (e as
                    ChainReplicationServer.NewSuccInfo).LastUpdateReceivedSucc;
                this.LastAckSent = (e as
                    ChainReplicationServer.NewSuccInfo).LastAckSent;
                this.Raise(new FixPredecessor());
            }

            private void ProcessSuccess() => this.Raise(new Done());
        }

        private class ChainReplicationServer : Actor
        {
            internal class SetupEvent : Event
            {
                public int Id;
                public bool IsHead;
                public bool IsTail;

                public SetupEvent(int id, bool isHead, bool isTail)
                    : base()
                {
                    this.Id = id;
                    this.IsHead = isHead;
                    this.IsTail = isTail;
                }
            }

            internal class PredSucc : Event
            {
                public ActorId Predecessor;
                public ActorId Successor;

                public PredSucc(ActorId pred, ActorId succ)
                    : base()
                {
                    this.Predecessor = pred;
                    this.Successor = succ;
                }
            }

            internal class ForwardUpdate : Event
            {
                public ActorId Predecessor;
                public int NextSeqId;
                public ActorId Client;
                public int Key;
                public int Value;

                public ForwardUpdate(ActorId pred, int nextSeqId, ActorId client, int key, int val)
                    : base()
                {
                    this.Predecessor = pred;
                    this.NextSeqId = nextSeqId;
                    this.Client = client;
                    this.Key = key;
                    this.Value = val;
                }
            }

            internal class BackwardAck : Event
            {
                public int NextSeqId;

                public BackwardAck(int nextSeqId)
                    : base()
                {
                    this.NextSeqId = nextSeqId;
                }
            }

            internal class NewPredecessor : Event
            {
                public ActorId Main;
                public ActorId Predecessor;

                public NewPredecessor(ActorId main, ActorId pred)
                    : base()
                {
                    this.Main = main;
                    this.Predecessor = pred;
                }
            }

            internal class NewSuccessor : Event
            {
                public ActorId Main;
                public ActorId Successor;
                public int LastUpdateReceivedSucc;
                public int LastAckSent;

                public NewSuccessor(ActorId main, ActorId succ,
                    int lastUpdateReceivedSucc, int lastAckSent)
                    : base()
                {
                    this.Main = main;
                    this.Successor = succ;
                    this.LastUpdateReceivedSucc = lastUpdateReceivedSucc;
                    this.LastAckSent = lastAckSent;
                }
            }

            internal class NewSuccInfo : Event
            {
                public int LastUpdateReceivedSucc;
                public int LastAckSent;

                public NewSuccInfo(int lastUpdateReceivedSucc, int lastAckSent)
                    : base()
                {
                    this.LastUpdateReceivedSucc = lastUpdateReceivedSucc;
                    this.LastAckSent = lastAckSent;
                }
            }

            internal class ResponseToQuery : Event
            {
                public int Value;

                public ResponseToQuery(int val)
                    : base()
                {
                    this.Value = val;
                }
            }

            internal class ResponseToUpdate : Event
            {
            }

            private class Local : Event
            {
            }

            private int ServerId;
            private bool IsHead;
            private bool IsTail;

            private ActorId Predecessor;
            private ActorId Successor;

            private Dictionary<int, int> KeyValueStore;
            private List<int> History;
            private List<SentLog> SentHistory;

            private int NextSeqId;

            protected override int HashedState
            {
                get
                {
                    unchecked
                    {
                        int hash = 37;

                        hash += (hash * 397) + (this.CurrentState?.FullName.GetHashCode() ?? "None".GetHashCode());
                        hash += (hash * 397) + this.IsHead.GetHashCode();
                        hash += (hash * 397) + this.IsTail.GetHashCode();
                        hash += (hash * 397) + this.NextSeqId.GetHashCode();

                        if (this.KeyValueStore != null)
                        {
                            foreach (var kvp in this.KeyValueStore)
                            {
                                hash += (hash * 397) + kvp.Key.GetHashCode();
                                hash += (hash * 397) + kvp.Value.GetHashCode();
                            }
                        }

                        if (this.History != null)
                        {
                            foreach (var item in this.History)
                            {
                                hash += (hash * 397) + item.GetHashCode();
                            }
                        }

                        States.GetOrAdd(hash, true);

                        return hash;
                    }
                }
            }

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(WaitForRequest))]
            [OnEventDoAction(typeof(PredSucc), nameof(SetupPredSucc))]
            [DeferEvents(typeof(Client.Update), typeof(Client.Query),
                typeof(BackwardAck), typeof(ForwardUpdate))]
            private class Init : ActorState
            {
            }

            private void InitOnEntry()
            {
                Event e = this.ReceivedEvent;
                this.ServerId = (e as SetupEvent).Id;
                this.IsHead = (e as SetupEvent).IsHead;
                this.IsTail = (e as SetupEvent).IsTail;

                this.KeyValueStore = new Dictionary<int, int>();
                this.History = new List<int>();
                this.SentHistory = new List<SentLog>();

                this.NextSeqId = 0;
            }

            private void SetupPredSucc()
            {
                Event e = this.ReceivedEvent;
                this.Predecessor = (e as PredSucc).Predecessor;
                this.Successor = (e as PredSucc).Successor;
                this.Raise(new Local());
            }

            [OnEventGotoState(typeof(Client.Update), typeof(ProcessUpdate), nameof(ProcessUpdateAction))]
            [OnEventGotoState(typeof(ForwardUpdate), typeof(ProcessFwdUpdate))]
            [OnEventGotoState(typeof(BackwardAck), typeof(ProcessBckAck))]
            [OnEventDoAction(typeof(Client.Query), nameof(ProcessQueryAction))]
            [OnEventDoAction(typeof(NewPredecessor), nameof(UpdatePredecessor))]
            [OnEventDoAction(typeof(NewSuccessor), nameof(UpdateSuccessor))]
            [OnEventDoAction(typeof(ChainReplicationMaster.BecomeHead), nameof(ProcessBecomeHead))]
            [OnEventDoAction(typeof(ChainReplicationMaster.BecomeTail), nameof(ProcessBecomeTail))]
            [OnEventDoAction(typeof(FailureDetector.Ping), nameof(SendPong))]
            private class WaitForRequest : ActorState
            {
            }

            private void ProcessUpdateAction()
            {
                this.NextSeqId++;
                this.Assert(this.IsHead, "Server {0} is not head", this.ServerId);
            }

            private void ProcessQueryAction()
            {
                Event e = this.ReceivedEvent;
                var client = (e as Client.Query).Client;
                var key = (e as Client.Query).Key;

                this.Assert(this.IsTail, "Server {0} is not tail", this.Id);

                if (this.KeyValueStore.ContainsKey(key))
                {
                    this.Monitor<ServerResponseSeqMonitor>(new ServerResponseSeqMonitor.ResponseToQuery(
                        this.Id, key, this.KeyValueStore[key]));

                    this.Send(client, new ResponseToQuery(this.KeyValueStore[key]));
                }
                else
                {
                    this.Send(client, new ResponseToQuery(-1));
                }
            }

            private void ProcessBecomeHead()
            {
                Event e = this.ReceivedEvent;
                this.IsHead = true;
                this.Predecessor = this.Id;

                var target = (e as ChainReplicationMaster.BecomeHead).Target;
                this.Send(target, new ChainReplicationMaster.HeadChanged());
            }

            private void ProcessBecomeTail()
            {
                Event e = this.ReceivedEvent;
                this.IsTail = true;
                this.Successor = this.Id;

                for (int i = 0; i < this.SentHistory.Count; i++)
                {
                    this.Monitor<ServerResponseSeqMonitor>(new ServerResponseSeqMonitor.ResponseToUpdate(
                        this.Id, this.SentHistory[i].Key, this.SentHistory[i].Value));

                    this.Send(this.SentHistory[i].Client, new ResponseToUpdate());
                    this.Send(this.Predecessor, new BackwardAck(this.SentHistory[i].NextSeqId));
                }

                var target = (e as ChainReplicationMaster.BecomeTail).Target;
                this.Send(target, new ChainReplicationMaster.TailChanged());
            }

            private void SendPong()
            {
                Event e = this.ReceivedEvent;
                var target = (e as FailureDetector.Ping).Target;
                this.Send(target, new FailureDetector.Pong());
            }

            private void UpdatePredecessor()
            {
                Event e = this.ReceivedEvent;
                var main = (e as NewPredecessor).Main;
                this.Predecessor = (e as NewPredecessor).Predecessor;

                if (this.History.Count > 0)
                {
                    if (this.SentHistory.Count > 0)
                    {
                        this.Send(main, new NewSuccInfo(
                            this.History[this.History.Count - 1],
                            this.SentHistory[0].NextSeqId));
                    }
                    else
                    {
                        this.Send(main, new NewSuccInfo(
                            this.History[this.History.Count - 1],
                            this.History[this.History.Count - 1]));
                    }
                }
            }

            private void UpdateSuccessor()
            {
                Event e = this.ReceivedEvent;
                var main = (e as NewSuccessor).Main;
                this.Successor = (e as NewSuccessor).Successor;
                var lastUpdateReceivedSucc = (e as NewSuccessor).LastUpdateReceivedSucc;
                var lastAckSent = (e as NewSuccessor).LastAckSent;

                if (this.SentHistory.Count > 0)
                {
                    for (int i = 0; i < this.SentHistory.Count; i++)
                    {
                        if (this.SentHistory[i].NextSeqId > lastUpdateReceivedSucc)
                        {
                            this.Send(this.Successor, new ForwardUpdate(this.Id, this.SentHistory[i].NextSeqId,
                                this.SentHistory[i].Client, this.SentHistory[i].Key, this.SentHistory[i].Value));
                        }
                    }

                    int tempIndex = -1;
                    for (int i = this.SentHistory.Count - 1; i >= 0; i--)
                    {
                        if (this.SentHistory[i].NextSeqId == lastAckSent)
                        {
                            tempIndex = i;
                        }
                    }

                    for (int i = 0; i < tempIndex; i++)
                    {
                        this.Send(this.Predecessor, new BackwardAck(this.SentHistory[0].NextSeqId));
                        this.SentHistory.RemoveAt(0);
                    }
                }

                this.Send(main, new ChainReplicationMaster.Success());
            }

            [OnEntry(nameof(ProcessUpdateOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(WaitForRequest))]
            private class ProcessUpdate : ActorState
            {
            }

            private void ProcessUpdateOnEntry()
            {
                Event e = this.ReceivedEvent;
                var client = (e as Client.Update).Client;
                var key = (e as Client.Update).Key;
                var value = (e as Client.Update).Value;

                if (this.KeyValueStore.ContainsKey(key))
                {
                    this.KeyValueStore[key] = value;
                }
                else
                {
                    this.KeyValueStore.Add(key, value);
                }

                this.History.Add(this.NextSeqId);

                this.Monitor<InvariantMonitor>(
                    new InvariantMonitor.HistoryUpdate(this.Id, new List<int>(this.History)));

                this.SentHistory.Add(new SentLog(this.NextSeqId, client, key, value));
                this.Monitor<InvariantMonitor>(
                    new InvariantMonitor.SentUpdate(this.Id, new List<SentLog>(this.SentHistory)));

                this.Send(this.Successor, new ForwardUpdate(this.Id, this.NextSeqId, client, key, value));

                this.Raise(new Local());
            }

            [OnEntry(nameof(ProcessFwdUpdateOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(WaitForRequest))]
            private class ProcessFwdUpdate : ActorState
            {
            }

            private void ProcessFwdUpdateOnEntry()
            {
                Event e = this.ReceivedEvent;
                var pred = (e as ForwardUpdate).Predecessor;
                var nextSeqId = (e as ForwardUpdate).NextSeqId;
                var client = (e as ForwardUpdate).Client;
                var key = (e as ForwardUpdate).Key;
                var value = (e as ForwardUpdate).Value;

                if (pred.Equals(this.Predecessor))
                {
                    this.NextSeqId = nextSeqId;

                    if (this.KeyValueStore.ContainsKey(key))
                    {
                        this.KeyValueStore[key] = value;
                    }
                    else
                    {
                        this.KeyValueStore.Add(key, value);
                    }

                    if (!this.IsTail)
                    {
                        this.History.Add(nextSeqId);

                        this.Monitor<InvariantMonitor>(
                            new InvariantMonitor.HistoryUpdate(this.Id, new List<int>(this.History)));

                        this.SentHistory.Add(new SentLog(this.NextSeqId, client, key, value));
                        this.Monitor<InvariantMonitor>(
                            new InvariantMonitor.SentUpdate(this.Id, new List<SentLog>(this.SentHistory)));

                        this.Send(this.Successor, new ForwardUpdate(this.Id, this.NextSeqId, client, key, value));
                    }
                    else
                    {
                        if (!this.IsHead)
                        {
                            this.History.Add(nextSeqId);
                        }

                        this.Monitor<ServerResponseSeqMonitor>(new ServerResponseSeqMonitor.ResponseToUpdate(
                            this.Id, key, value));

                        this.Send(client, new ResponseToUpdate());
                        this.Send(this.Predecessor, new BackwardAck(nextSeqId));
                    }
                }

                this.Raise(new Local());
            }

            [OnEntry(nameof(ProcessBckAckOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(WaitForRequest))]
            private class ProcessBckAck : ActorState
            {
            }

            private void ProcessBckAckOnEntry()
            {
                Event e = this.ReceivedEvent;
                var nextSeqId = (e as BackwardAck).NextSeqId;

                this.RemoveItemFromSent(nextSeqId);

                if (!this.IsHead)
                {
                    this.Send(this.Predecessor, new BackwardAck(nextSeqId));
                }

                this.Raise(new Local());
            }

            private void RemoveItemFromSent(int seqId)
            {
                int removeIdx = -1;

                for (int i = this.SentHistory.Count - 1; i >= 0; i--)
                {
                    if (seqId == this.SentHistory[i].NextSeqId)
                    {
                        removeIdx = i;
                    }
                }

                if (removeIdx != -1)
                {
                    this.SentHistory.RemoveAt(removeIdx);
                }
            }
        }

        private class Client : Actor
        {
            internal class SetupEvent : Event
            {
                public int Id;
                public ActorId HeadNode;
                public ActorId TailNode;
                public int Value;

                public SetupEvent(int id, ActorId head, ActorId tail, int val)
                    : base()
                {
                    this.Id = id;
                    this.HeadNode = head;
                    this.TailNode = tail;
                    this.Value = val;
                }
            }

            internal class UpdateHeadTail : Event
            {
                public ActorId Head;
                public ActorId Tail;

                public UpdateHeadTail(ActorId head, ActorId tail)
                    : base()
                {
                    this.Head = head;
                    this.Tail = tail;
                }
            }

            internal class Update : Event
            {
                public ActorId Client;
                public int Key;
                public int Value;

                public Update(ActorId client, int key, int value)
                    : base()
                {
                    this.Client = client;
                    this.Key = key;
                    this.Value = value;
                }
            }

            internal class Query : Event
            {
                public ActorId Client;
                public int Key;

                public Query(ActorId client, int key)
                    : base()
                {
                    this.Client = client;
                    this.Key = key;
                }
            }

            private class Local : Event
            {
            }

            private class Done : Event
            {
            }

            private ActorId HeadNode;
            private ActorId TailNode;

            private int StartIn;
            private int Next;

            private Dictionary<int, int> KeyValueStore;

            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(PumpUpdateRequests))]
            private class Init : ActorState
            {
            }

            private void InitOnEntry()
            {
                Event e = this.ReceivedEvent;
                this.HeadNode = (e as SetupEvent).HeadNode;
                this.TailNode = (e as SetupEvent).TailNode;

                this.StartIn = (e as SetupEvent).Value;
                this.Next = 1;

                this.KeyValueStore = new Dictionary<int, int>
                {
                    { 1 * this.StartIn, 100 },
                    { 2 * this.StartIn, 200 },
                    { 3 * this.StartIn, 300 },
                    { 4 * this.StartIn, 400 }
                };

                this.Raise(new Local());
            }

            [OnEntry(nameof(PumpUpdateRequestsOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(PumpUpdateRequests), nameof(PumpRequestsLocalAction))]
            [OnEventGotoState(typeof(Done), typeof(PumpQueryRequests), nameof(PumpRequestsDoneAction))]
            [IgnoreEvents(typeof(ChainReplicationServer.ResponseToUpdate), typeof(ChainReplicationServer.ResponseToQuery))]
            private class PumpUpdateRequests : ActorState
            {
            }

            private void PumpUpdateRequestsOnEntry()
            {
                this.Send(this.HeadNode, new Update(this.Id, this.Next * this.StartIn,
                    this.KeyValueStore[this.Next * this.StartIn]));

                if (this.Next >= 3)
                {
                    this.Raise(new Done());
                }
                else
                {
                    this.Raise(new Local());
                }
            }

            [OnEntry(nameof(PumpQueryRequestsOnEntry))]
            [OnEventGotoState(typeof(Local), typeof(PumpQueryRequests), nameof(PumpRequestsLocalAction))]
            [IgnoreEvents(typeof(ChainReplicationServer.ResponseToUpdate), typeof(ChainReplicationServer.ResponseToQuery))]
            private class PumpQueryRequests : ActorState
            {
            }

            private void PumpQueryRequestsOnEntry()
            {
                this.Send(this.TailNode, new Query(this.Id, this.Next * this.StartIn));

                if (this.Next >= 3)
                {
                    this.Raise(new Halt());
                }
                else
                {
                    this.Raise(new Local());
                }
            }

            private void PumpRequestsLocalAction()
            {
                this.Next++;
            }

            private void PumpRequestsDoneAction()
            {
                this.Next = 1;
            }
        }

        private class InvariantMonitor : Monitor
        {
            internal class SetupEvent : Event
            {
                public List<ActorId> Servers;

                public SetupEvent(List<ActorId> servers)
                    : base()
                {
                    this.Servers = servers;
                }
            }

            internal class UpdateServers : Event
            {
                public List<ActorId> Servers;

                public UpdateServers(List<ActorId> servers)
                    : base()
                {
                    this.Servers = servers;
                }
            }

            internal class HistoryUpdate : Event
            {
                public ActorId Server;
                public List<int> History;

                public HistoryUpdate(ActorId server, List<int> history)
                    : base()
                {
                    this.Server = server;
                    this.History = history;
                }
            }

            internal class SentUpdate : Event
            {
                public ActorId Server;
                public List<SentLog> SentHistory;

                public SentUpdate(ActorId server, List<SentLog> sentHistory)
                    : base()
                {
                    this.Server = server;
                    this.SentHistory = sentHistory;
                }
            }

            private class Local : Event
            {
            }

            private List<ActorId> Servers;

            private Dictionary<ActorId, List<int>> History;
            private Dictionary<ActorId, List<int>> SentHistory;
            private List<int> TempSeq;

            private ActorId Next;
            private ActorId Prev;

            [Start]
            [OnEventGotoState(typeof(Local), typeof(WaitForUpdateMessage))]
            [OnEventDoAction(typeof(SetupEvent), nameof(Setup))]
            private class Init : MonitorState
            {
            }

            private void Setup()
            {
                Event e = this.ReceivedEvent;
                this.Servers = (e as SetupEvent).Servers;
                this.History = new Dictionary<ActorId, List<int>>();
                this.SentHistory = new Dictionary<ActorId, List<int>>();
                this.TempSeq = new List<int>();
                this.Raise(new Local());
            }

            [OnEventDoAction(typeof(HistoryUpdate), nameof(CheckUpdatePropagationInvariant))]
            [OnEventDoAction(typeof(SentUpdate), nameof(CheckInprocessRequestsInvariant))]
            [OnEventDoAction(typeof(UpdateServers), nameof(ProcessUpdateServers))]
            private class WaitForUpdateMessage : MonitorState
            {
            }

            private void CheckUpdatePropagationInvariant()
            {
                Event e = this.ReceivedEvent;
                var server = (e as HistoryUpdate).Server;
                var history = (e as HistoryUpdate).History;

                this.IsSorted(history);

                if (this.History.ContainsKey(server))
                {
                    this.History[server] = history;
                }
                else
                {
                    this.History.Add(server, history);
                }

                // HIST(i+1) <= HIST(i)
                this.GetNext(server);
                if (this.Next != null && this.History.ContainsKey(this.Next))
                {
                    this.CheckLessOrEqualThan(this.History[this.Next], this.History[server]);
                }

                // HIST(i) <= HIST(i-1)
                this.GetPrev(server);
                if (this.Prev != null && this.History.ContainsKey(this.Prev))
                {
                    this.CheckLessOrEqualThan(this.History[server], this.History[this.Prev]);
                }
            }

            private void CheckInprocessRequestsInvariant()
            {
                Event e = this.ReceivedEvent;
                this.ClearTempSeq();

                var server = (e as SentUpdate).Server;
                var sentHistory = (e as SentUpdate).SentHistory;

                this.ExtractSeqId(sentHistory);

                if (this.SentHistory.ContainsKey(server))
                {
                    this.SentHistory[server] = this.TempSeq;
                }
                else
                {
                    this.SentHistory.Add(server, this.TempSeq);
                }

                this.ClearTempSeq();

                // HIST(i) == HIST(i+1) + SENT(i)
                this.GetNext(server);
                if (this.Next != null && this.History.ContainsKey(this.Next))
                {
                    this.MergeSeq(this.History[this.Next], this.SentHistory[server]);
                    this.CheckEqual(this.History[server], this.TempSeq);
                }

                this.ClearTempSeq();

                // HIST(i-1) == HIST(i) + SENT(i-1)
                this.GetPrev(server);
                if (this.Prev != null && this.History.ContainsKey(this.Prev))
                {
                    this.MergeSeq(this.History[server], this.SentHistory[this.Prev]);
                    this.CheckEqual(this.History[this.Prev], this.TempSeq);
                }

                this.ClearTempSeq();
            }

            private void GetNext(ActorId curr)
            {
                this.Next = null;

                for (int i = 1; i < this.Servers.Count; i++)
                {
                    if (this.Servers[i - 1].Equals(curr))
                    {
                        this.Next = this.Servers[i];
                    }
                }
            }

            private void GetPrev(ActorId curr)
            {
                this.Prev = null;

                for (int i = 1; i < this.Servers.Count; i++)
                {
                    if (this.Servers[i].Equals(curr))
                    {
                        this.Prev = this.Servers[i - 1];
                    }
                }
            }

            private void ExtractSeqId(List<SentLog> seq)
            {
                this.ClearTempSeq();

                for (int i = seq.Count - 1; i >= 0; i--)
                {
                    if (this.TempSeq.Count > 0)
                    {
                        this.TempSeq.Insert(0, seq[i].NextSeqId);
                    }
                    else
                    {
                        this.TempSeq.Add(seq[i].NextSeqId);
                    }
                }

                this.IsSorted(this.TempSeq);
            }

            private void MergeSeq(List<int> seq1, List<int> seq2)
            {
                this.ClearTempSeq();
                this.IsSorted(seq1);

                if (seq1.Count is 0)
                {
                    this.TempSeq = seq2;
                }
                else if (seq2.Count is 0)
                {
                    this.TempSeq = seq1;
                }
                else
                {
                    for (int i = 0; i < seq1.Count; i++)
                    {
                        if (seq1[i] < seq2[0])
                        {
                            this.TempSeq.Add(seq1[i]);
                        }
                    }

                    for (int i = 0; i < seq2.Count; i++)
                    {
                        this.TempSeq.Add(seq2[i]);
                    }
                }

                this.IsSorted(this.TempSeq);
            }

            private void IsSorted(List<int> seq)
            {
                for (int i = 0; i < seq.Count - 1; i++)
                {
                    if (seq[i] >= seq[i + 1])
                    {
                        lock (BugsFoundLock)
                        {
                            BugsFound++;
                        }
                    }

                    this.Assert(seq[i] < seq[i + 1], "Sequence is not sorted.");
                }
            }

            private void CheckLessOrEqualThan(List<int> seq1, List<int> seq2)
            {
                this.IsSorted(seq1);
                this.IsSorted(seq2);

                for (int i = 0; i < seq1.Count; i++)
                {
                    if ((i == seq1.Count) || (i == seq2.Count))
                    {
                        break;
                    }

                    this.Assert(seq1[i] <= seq2[i], "{0} not less or equal than {1}.", seq1[i], seq2[i]);
                }
            }

            private void CheckEqual(List<int> seq1, List<int> seq2)
            {
                this.IsSorted(seq1);
                this.IsSorted(seq2);

                for (int i = 0; i < seq1.Count; i++)
                {
                    if ((i == seq1.Count) || (i == seq2.Count))
                    {
                        break;
                    }

                    this.Assert(seq1[i] == seq2[i], "{0} not equal with {1}.", seq1[i], seq2[i]);
                }
            }

            private void ClearTempSeq()
            {
                this.Assert(this.TempSeq.Count <= 6, "Temp sequence has more than 6 elements.");
                this.TempSeq.Clear();
                this.Assert(this.TempSeq.Count is 0, "Temp sequence is not cleared.");
            }

            private void ProcessUpdateServers()
            {
                Event e = this.ReceivedEvent;
                this.Servers = (e as UpdateServers).Servers;
            }
        }

        private class ServerResponseSeqMonitor : Monitor
        {
            internal class SetupEvent : Event
            {
                public List<ActorId> Servers;

                public SetupEvent(List<ActorId> servers)
                    : base()
                {
                    this.Servers = servers;
                }
            }

            internal class UpdateServers : Event
            {
                public List<ActorId> Servers;

                public UpdateServers(List<ActorId> servers)
                    : base()
                {
                    this.Servers = servers;
                }
            }

            internal class ResponseToUpdate : Event
            {
                public ActorId Tail;
                public int Key;
                public int Value;

                public ResponseToUpdate(ActorId tail, int key, int val)
                    : base()
                {
                    this.Tail = tail;
                    this.Key = key;
                    this.Value = val;
                }
            }

            internal class ResponseToQuery : Event
            {
                public ActorId Tail;
                public int Key;
                public int Value;

                public ResponseToQuery(ActorId tail, int key, int val)
                    : base()
                {
                    this.Tail = tail;
                    this.Key = key;
                    this.Value = val;
                }
            }

            private class Local : Event
            {
            }

            private List<ActorId> Servers;
            private Dictionary<int, int> LastUpdateResponse;

            [Start]
            [OnEventGotoState(typeof(Local), typeof(Wait))]
            [OnEventDoAction(typeof(SetupEvent), nameof(Setup))]
            private class Init : MonitorState
            {
            }

            private void Setup()
            {
                Event e = this.ReceivedEvent;
                this.Servers = (e as SetupEvent).Servers;
                this.LastUpdateResponse = new Dictionary<int, int>();
                this.Raise(new Local());
            }

            [OnEventDoAction(typeof(ResponseToUpdate), nameof(ResponseToUpdateAction))]
            [OnEventDoAction(typeof(ResponseToQuery), nameof(ResponseToQueryAction))]
            [OnEventDoAction(typeof(UpdateServers), nameof(ProcessUpdateServers))]
            private class Wait : MonitorState
            {
            }

            private void ResponseToUpdateAction()
            {
                Event e = this.ReceivedEvent;
                var tail = (e as ResponseToUpdate).Tail;
                var key = (e as ResponseToUpdate).Key;
                var value = (e as ResponseToUpdate).Value;

                if (this.Servers.Contains(tail))
                {
                    if (this.LastUpdateResponse.ContainsKey(key))
                    {
                        this.LastUpdateResponse[key] = value;
                    }
                    else
                    {
                        this.LastUpdateResponse.Add(key, value);
                    }
                }
            }

            private void ResponseToQueryAction()
            {
                Event e = this.ReceivedEvent;
                var tail = (e as ResponseToQuery).Tail;
                var key = (e as ResponseToQuery).Key;
                var value = (e as ResponseToQuery).Value;

                if (this.Servers.Contains(tail))
                {
                    this.Assert(value == this.LastUpdateResponse[key], "Value {0} is not " +
                        "equal to {1}", value, this.LastUpdateResponse[key]);
                }
            }

            private void ProcessUpdateServers()
            {
                Event e = this.ReceivedEvent;
                this.Servers = (e as UpdateServers).Servers;
            }
        }
    }
}
