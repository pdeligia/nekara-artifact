using Microsoft.Coyote;

namespace Benchmarks.Protocols
{
    public class Driver
    {
        [Test]
        public static void Test_FailureDetector(IMachineRuntime runtime)
        {
            FailureDetector.Execute(runtime);
        }

        [Test]
        public static void Test_CoffeeMachine(IMachineRuntime runtime)
        {
            CoffeeMachine.Execute(runtime);
        }

        [Test]
        public static void Test_Chord(IMachineRuntime runtime)
        {
            Chord.Execute(runtime);
        }

        [Test]
        public static void Test_Raft(IMachineRuntime runtime)
        {
            Raft.Execute(runtime);
        }

        [Test]
        public static void Test_Paxos(IMachineRuntime runtime)
        {
            Paxos.Execute(runtime);
        }

        [Test]
        public static void Test_ChainReplication(IMachineRuntime runtime)
        {
            ChainReplication.Execute(runtime);
        }
    }
}
