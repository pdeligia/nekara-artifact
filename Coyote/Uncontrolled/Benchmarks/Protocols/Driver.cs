using Microsoft.Coyote;

namespace Benchmarks.Protocols
{
    public class Driver
    {
        [Test]
        public static void Test_FailureDetector(IActorRuntime runtime)
        {
            FailureDetector.Execute(runtime);
        }

        [Test]
        public static void Test_Raft(IActorRuntime runtime)
        {
            Raft.Execute(runtime);
        }

        [Test]
        public static void Test_Paxos(IActorRuntime runtime)
        {
            Paxos.Execute(runtime);
        }

        [Test]
        public static void Test_ChainReplication(IActorRuntime runtime)
        {
            ChainReplication.Execute(runtime);
        }
    }
}
