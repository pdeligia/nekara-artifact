using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
                Test_FailureDetector(runtime);
                await runtime.WaitAsync();

                lock (FailureDetector.BugsFoundLock)
                {
                    if (FailureDetector.BugsFound > bugsFound)
                    {
                        Console.WriteLine($"==================> Found bug #{FailureDetector.BugsFound}");
                        bugsFound = FailureDetector.BugsFound;
                    }
                }

                if (i % 1000 is 0)
                {
                    Console.WriteLine($"==================> #{i} Custom States (size: {FailureDetector.States.Count})");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"... Found {bugsFound} bugs in {stopwatch.Elapsed.TotalMilliseconds}ms");
        }
    }
}
