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
        public static void Test_ChainReplication(IActorRuntime runtime)
        {
            ChainReplication.Execute(runtime);
        }

        public class Results
        {
            public double BuggyIterations { get; set; }
        }

        public static async Task Main(string[] args)
        {
            var config = Configuration.Create();
            config.EnableMonitorsInProduction = true;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int iterations = int.Parse(args[0]);
            int bugsFound = 0;
            for (int i = 1; i <= iterations; i++)
            {
                var runtime = CoyoteRuntime.Create(config);
                Test_ChainReplication(runtime);
                await runtime.WaitAsync();

                lock (ChainReplication.BugsFoundLock)
                {
                    if (ChainReplication.BugsFound > bugsFound)
                    {
                        Console.WriteLine($"==================> Found bug #{ChainReplication.BugsFound}");
                        bugsFound = ChainReplication.BugsFound;
                    }
                }

                if (i % 1000 is 0)
                {
                    Console.WriteLine($"==================> #{i} Custom States (size: {ChainReplication.States.Count})");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"... Found {bugsFound} bugs in {stopwatch.Elapsed.TotalMilliseconds}ms");

            var fileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
              "Results", "chainreplication_uncontrolled.json");
            var results = JsonSerializer.Serialize(new Results()
            {
                BuggyIterations = ChainReplication.BugsFound / (double)iterations
            });
            File.WriteAllText(fileName, results);
        }
    }
}
