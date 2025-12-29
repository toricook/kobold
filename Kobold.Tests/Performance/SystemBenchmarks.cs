using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using System;
using System.Numerics;
using Tests.Helpers;

namespace Tests.Performance
{
    /// <summary>
    /// BenchmarkDotNet benchmarks for core game systems.
    /// Run via: dotnet run -c Release --project Tests --filter "*SystemBenchmarks*"
    /// Or use the PerformanceProfiler tool to generate markdown reports.
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MarkdownExporter]
    public class SystemBenchmarks
    {
        private TestWorld _testWorld;
        private CollisionSystem _collisionSystem;
        private PhysicsSystem _physicsSystem;

        [Params(10, 50, 100, 200, 500)]
        public int EntityCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _testWorld = new TestWorld();
            _collisionSystem = new CollisionSystem(_testWorld.World, _testWorld.EventBus);
            _physicsSystem = new PhysicsSystem(_testWorld.World, new PhysicsConfig
            {
                EnableGravity = false,
                EnableDamping = false
            });

            // Create entities for testing
            var random = new Random(42);
            for (int i = 0; i < EntityCount; i++)
            {
                _testWorld.World.Create(
                    new Transform(new Vector2(random.Next(0, 800), random.Next(0, 600))),
                    new BoxCollider(new Vector2(20, 20)),
                    new CollisionLayerComponent(CollisionLayer.Default),
                    new Velocity(new Vector2(random.Next(-100, 100), random.Next(-100, 100))),
                    new Physics()
                );
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _testWorld?.Dispose();
        }

        [Benchmark]
        public void CollisionDetection()
        {
            _collisionSystem.Update(0.016f);
        }

        [Benchmark]
        public void PhysicsUpdate()
        {
            _physicsSystem.Update(0.016f);
        }
    }
}
