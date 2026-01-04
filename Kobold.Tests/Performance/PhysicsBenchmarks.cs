using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using System;
using System.Numerics;
using Tests.Helpers;
using Kobold.Extensions.Physics.Systems;
using Kobold.Extensions.Physics.Components;
namespace Tests.Performance
{
    /// <summary>
    /// Detailed physics system benchmarks.
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MarkdownExporter]
    public class PhysicsBenchmarks
    {
        private TestWorld _testWorldNoGravity;
        private TestWorld _testWorldWithGravity;
        private PhysicsSystem _physicsSystemNoGravity;
        private PhysicsSystem _physicsSystemWithGravity;

        [Params(100, 500, 1000)]
        public int EntityCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            SetupNoGravity();
            SetupWithGravity();
        }

        private void SetupNoGravity()
        {
            _testWorldNoGravity = new TestWorld();
            _physicsSystemNoGravity = new PhysicsSystem(_testWorldNoGravity.World, new PhysicsConfig
            {
                EnableGravity = false,
                EnableDamping = false
            });

            var random = new Random(42);
            for (int i = 0; i < EntityCount; i++)
            {
                _testWorldNoGravity.World.Create(
                    new Transform(new Vector2(random.Next(0, 800), random.Next(0, 600))),
                    new Velocity(new Vector2(random.Next(-100, 100), random.Next(-100, 100))),
                    new Physics()
                );
            }
        }

        private void SetupWithGravity()
        {
            _testWorldWithGravity = new TestWorld();
            _physicsSystemWithGravity = new PhysicsSystem(_testWorldWithGravity.World, new PhysicsConfig
            {
                EnableGravity = true,
                GlobalGravity = new Vector2(0, 100),
                EnableDamping = true,
                DefaultLinearDamping = 0.98f
            });

            var random = new Random(43);
            for (int i = 0; i < EntityCount; i++)
            {
                _testWorldWithGravity.World.Create(
                    new Transform(new Vector2(random.Next(0, 800), random.Next(0, 600))),
                    new Velocity(new Vector2(random.Next(-100, 100), random.Next(-100, 100))),
                    new Physics()
                );
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _testWorldNoGravity?.Dispose();
            _testWorldWithGravity?.Dispose();
        }

        [Benchmark(Baseline = true)]
        public void Physics_NoGravity_NoDamping()
        {
            _physicsSystemNoGravity.Update(0.016f);
        }

        [Benchmark]
        public void Physics_WithGravity_WithDamping()
        {
            _physicsSystemWithGravity.Update(0.016f);
        }
    }
}
