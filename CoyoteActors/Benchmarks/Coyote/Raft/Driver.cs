using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Coyote;
using Microsoft.Coyote.TestingServices;

namespace Benchmarks.Protocols
{
    public class Driver
    {
        [Test]
        public static void Test_Raft(IActorRuntime runtime)
        {
            Raft.Execute(runtime);
        }

        public class Results
        {
            public double BuggyIterations { get; set; }
            public int States { get; set; }
            public double Time { get; set; }
        }

        public static void Main()
        {
            var config = Configuration.Create();
            config.SchedulingIterations = 10000;
            config.PerformFullExploration = true;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var tester = TestingEngineFactory.CreateBugFindingEngine(config, Test_Raft);
            tester.Run();

            stopwatch.Stop();
            Console.WriteLine($"... Found {tester.TestReport.NumOfFoundBugs} bugs in {stopwatch.Elapsed.TotalMilliseconds}ms");

            var fileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
              "raft_coyote.json");
            var results = JsonSerializer.Serialize(new Results()
            {
                BuggyIterations = tester.TestReport.NumOfFoundBugs / (double)config.SchedulingIterations,
                States = tester.TestReport.NumOfStates,
                Time = stopwatch.Elapsed.TotalMilliseconds
            });
            File.WriteAllText(fileName, results);
        }
    }
}
