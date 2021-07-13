﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.CoyoteActors;
using Microsoft.Coyote.SystematicTesting;

namespace Benchmarks
{
    public class Driver
    {
        [Microsoft.Coyote.SystematicTesting.Test]
        public static void Test_Paxos()
        {
            var config = Microsoft.CoyoteActors.Configuration.Create();
            config.EnableMonitorsInProduction = true;
            Paxos.Execute(CoyoteRuntime.Create(config));
        }

        public class Results
        {
            public double BuggyIterations { get; set; }
            public double Time { get; set; }
        }

        public static void Main(string[] args)
        {
            var config = Microsoft.Coyote.Configuration.Create()
              .WithTestingIterations(uint.Parse(args[0]))
              .WithProbabilisticStrategy(3);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var tester = TestingEngine.Create(config, Test_Paxos);
            tester.Run();

            stopwatch.Stop();
            Console.WriteLine($"... Found {Paxos.BugsFound} bugs in {stopwatch.Elapsed.TotalMilliseconds}ms");

            var fileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
              "Results", "paxos_tpl_nekara.json");
            var results = JsonSerializer.Serialize(new Results()
            {
                BuggyIterations = (Paxos.BugsFound / (double)config.TestingIterations) * 100,
                Time = stopwatch.Elapsed.TotalMilliseconds * 0.001
            });
            File.WriteAllText(fileName, results);
        }
    }
}
