using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.CoyoteActors;

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
            public int States { get; set; }
        }

        public static async Task Main()
        {
            var config = Configuration.Create();
            config.EnableMonitorsInProduction = true;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int iterations = 10000;
            int bugsFound = 0;
            for (int i = 1; i <= iterations; i++)
            {
                var runtime = CoyoteRuntime.Create(config);
                Test_Raft(runtime);
                await runtime.WaitAsync();

                lock (Raft.BugsFoundLock)
                {
                    if (Raft.BugsFound > bugsFound)
                    {
                        Console.WriteLine($"==================> Found bug #{Raft.BugsFound}");
                        bugsFound = Raft.BugsFound;
                    }
                }

                if (i % 1000 is 0)
                {
                    Console.WriteLine($"==================> #{i} Custom States (size: {Raft.States.Count})");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"... Found {bugsFound} bugs in {stopwatch.Elapsed.TotalMilliseconds}ms");

            var fileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
              "results", "raft_uncontrolled.json");
            var results = JsonSerializer.Serialize(new Results()
            {
                BuggyIterations = Raft.BugsFound / (double)iterations,
                States = Raft.States.Count
            });
            File.WriteAllText(fileName, results);
        }
    }
}
