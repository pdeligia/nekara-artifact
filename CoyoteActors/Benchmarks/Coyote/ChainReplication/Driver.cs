using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.CoyoteActors;
using Microsoft.CoyoteActors.TestingServices;

namespace Benchmarks
{
    public class Driver
    {
        [Test]
        public static void Test_ChainReplication(IActorRuntime runtime)
        {
            ChainReplication.Execute(runtime);
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

            var tester = TestingEngineFactory.CreateBugFindingEngine(config, Test_ChainReplication);
            tester.Run();

            stopwatch.Stop();
            Console.WriteLine($"... Found {tester.TestReport.NumOfFoundBugs} bugs in {stopwatch.Elapsed.TotalMilliseconds}ms");

            var fileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
              "results", "chainreplication_coyote.json");
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
