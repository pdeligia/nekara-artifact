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
        public static void Test_Raft(IActorRuntime runtime)
        {
            Raft.Execute(runtime);
        }

        public class Results
        {
            public double BuggyIterations { get; set; }
            public double Time { get; set; }
        }

        public static void Main(string[] args)
        {
            var config = Configuration.Create();
            config.SchedulingIterations = int.Parse(args[0]);
            config.PerformFullExploration = true;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var tester = TestingEngineFactory.CreateBugFindingEngine(config, Test_Raft);
            tester.Run();

            stopwatch.Stop();
            Console.WriteLine($"... Found {tester.TestReport.NumOfFoundBugs} bugs in {stopwatch.Elapsed.TotalMilliseconds}ms");

            var fileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
              "Results", "raft_coyote_nekara.json");
            var results = JsonSerializer.Serialize(new Results()
            {
                BuggyIterations = tester.TestReport.NumOfFoundBugs / (double)config.SchedulingIterations,
                Time = stopwatch.Elapsed.TotalMilliseconds * 0.001
            });
            File.WriteAllText(fileName, results);
        }
    }
}
