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
    [TestFixture]
    public class CollisionPerformanceTests
    {
        [Test]
        [Category("Performance")]
        public void CollisionSystem_With100Entities_CompletesInUnder5ms()
        {
            // Arrange
            var testWorld = new TestWorld();
            var collisionSystem = new CollisionSystem(testWorld.World, testWorld.EventBus);

            // Create 100 entities
            var random = new Random(42);
            for (int i = 0; i < 100; i++)
            {
                testWorld.World.Create(
                    new Transform(new Vector2(random.Next(0, 800), random.Next(0, 600))),
                    new BoxCollider(new Vector2(20, 20)),
                    new CollisionLayerComponent(CollisionLayer.Default)
                );
            }

            // Act - measure with stopwatch
            var stopwatch = Stopwatch.StartNew();
            collisionSystem.Update(0.016f);
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5,
                "collision detection should complete within frame budget (16.6ms)");

            // Log for tracking
            TestContext.WriteLine($"Collision took {stopwatch.Elapsed.TotalMilliseconds:F2}ms for 100 entities");
        }
    }
}
