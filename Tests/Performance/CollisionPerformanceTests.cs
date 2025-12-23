using FluentAssertions;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tests.Helpers;

namespace Tests.Performance
{
    /// <summary>
    /// Performance benchmarks using BenchmarkDotNet.
    /// These are NOT run during regular CI/CD tests.
    /// Run separately using the PerformanceProfiler tool to generate markdown reports.
    /// </summary>
    [TestFixture]
    [Explicit("Performance benchmarks - run manually or via dedicated performance profiler")]
    public class CollisionPerformanceTests
    {
        [Test]
        [Category("Performance")]
        public void CollisionSystem_WithVariousEntityCounts_ReportsPerformance()
        {
            // This test just reports performance, doesn't fail
            var entityCounts = new[] { 10, 50, 100, 200, 500 };

            foreach (var count in entityCounts)
            {
                var testWorld = new TestWorld();
                var collisionSystem = new CollisionSystem(testWorld.World, testWorld.EventBus);

                // Create entities
                var random = new Random(42);
                for (int i = 0; i < count; i++)
                {
                    testWorld.World.Create(
                        new Transform(new Vector2(random.Next(0, 800), random.Next(0, 600))),
                        new BoxCollider(new Vector2(20, 20)),
                        new CollisionLayerComponent(CollisionLayer.Default)
                    );
                }

                // Warm up
                collisionSystem.Update(0.016f);

                // Measure multiple runs
                var runs = 100;
                var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < runs; i++)
                {
                    collisionSystem.Update(0.016f);
                }
                stopwatch.Stop();

                var avgMs = stopwatch.Elapsed.TotalMilliseconds / runs;
                TestContext.WriteLine($"Collision with {count,3} entities: {avgMs:F3}ms avg (over {runs} runs)");

                testWorld.Dispose();
            }
        }
    }
}
