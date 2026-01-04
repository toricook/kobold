using FluentAssertions;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tests.Helpers;
using Kobold.Extensions.Physics.Systems;
using Kobold.Extensions.Physics.Components;
namespace Tests.Systems
{
    [TestFixture]
    public class PhysicsSystemTests
    {
        private TestWorld _testWorld;
        private PhysicsSystem _physicsSystem;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new TestWorld();
            _physicsSystem = new PhysicsSystem(_testWorld.World, new PhysicsConfig
            {
                EnableGravity = false,
                EnableDamping = false
            });
        }

        [TearDown]
        public void TearDown()
        {
            _testWorld.Dispose();
        }

        [Test]
        public void Update_MovesEntityBasedOnVelocity()
        {
            // Arrange
            var entity = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new Velocity(new Vector2(100, 50)) // 100px/s right, 50px/s down
            );

            // Act
            _physicsSystem.Update(deltaTime: 1.0f); // 1 second

            // Assert
            entity.ShouldBeAt(_testWorld.World, new Vector2(100, 50));
        }

        [Test]
        public void Update_WithMultipleFrames_AccumulatesMovement()
        {
            // Arrange
            var entity = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new Velocity(new Vector2(60, 0)) // 60px/s
            );

            // Act - simulate 1 second at 60 FPS
            for (int i = 0; i < 60; i++)
            {
                _physicsSystem.Update(1f / 60f);
            }

            // Assert
            entity.ShouldBeAt(_testWorld.World, new Vector2(60, 0), tolerance: 0.1f);
        }

        [Test]
        public void Update_WithGravity_AppliesDownwardForce()
        {
            // Arrange
            var gravitySystem = new PhysicsSystem(_testWorld.World, new PhysicsConfig
            {
                EnableGravity = true,
                GlobalGravity = new Vector2(0, 100) // 100px/s² down
            });

            var entity = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new Velocity(Vector2.Zero),
                new Physics() // Has physics, so gravity applies
            );

            // Act
            gravitySystem.Update(deltaTime: 1.0f);

            // Assert
            var velocity = entity.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Y.Should().BeApproximately(100f, 0.01f);
        }
    }
}
