using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Coyote;

namespace Benchmarks.Protocols
{
    public class Driver
    {
        [Test]
        public static void Test_Raft(IActorRuntime runtime)
        {
            Raft.Execute(runtime);
        }

        public static async Task Main()
        {
            var config = Configuration.Create();
            config.EnableMonitorsInProduction = true;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int bugsFound = 0;
            for (int i = 0; i < 100000; i++)
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
        }
    }
}
