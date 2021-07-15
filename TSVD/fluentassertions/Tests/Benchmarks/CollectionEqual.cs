using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using Microsoft.Coyote.SystematicTesting;
using Microsoft.Coyote.Tasks;
using Task = Microsoft.Coyote.Tasks.Task;

namespace Benchmarks
{
    [MemoryDiagnoser]
    [RyuJitX86Job]
    public class CollectionEqualBenchmarks
    {
        private int[] collection1;
        private int[] collection2;

        [Params(10, 100, 1_000, 5_000, 10_000)]
        public int N { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            collection1 = new int[N];
            collection2 = new int[N];
        }

        [Benchmark(Baseline = true)]
        public void CollectionEqual_NonGeneric()
        {
            ((IEnumerable)collection1).Should().Equal(collection2);
        }

        [Benchmark]
        public void CollectionEqual_Generic()
        {
            collection1.Should().Equal(collection2);
        }

        [Benchmark]
        public void CollectionEqual_Generic_IsSameOrEqualTo()
        {
            collection1.Should().Equal(collection2, (s, e) => ((object)s).IsSameOrEqualTo(e));
        }

        [Benchmark]
        public void CollectionEqual_Generic_Equality()
        {
            collection1.Should().Equal(collection2, (a, b) => a == b);
        }

        [Benchmark]
        [Microsoft.Coyote.SystematicTesting.Test]
        public static void TestMethod()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            Type type = typeof(IEnumerable);
            IEquivalencyAssertionOptions equivalencyAssertionOptions = new EquivalencyAssertionOptions();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            /*
            Action act = () => Parallel.For(
                0,
                10_000,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 8
                },
                e => equivalencyAssertionOptions.GetEqualityStrategy(type)
            );
            */

            var tasks = new List<Task>()
            {
                Task.Run( () => equivalencyAssertionOptions.GetEqualityStrategy(type) ),
                Task.Run( () => equivalencyAssertionOptions.GetEqualityStrategy(type) )
            };

            Task.WaitAll(tasks.ToArray());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            //act.Should().NotThrow();
        }
    }
}
