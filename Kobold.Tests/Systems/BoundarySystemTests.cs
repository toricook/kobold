using Kobold.Extensions.Gameplay.Components;
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
using FluentAssertions;
using Kobold.Extensions.Boundaries.Systems;
using Kobold.Extensions.Boundaries.Components;
using Kobold.Extensions.Collision.Components;
using Kobold.Extensions.Destruction.Systems;
using Kobold.Extensions.Gameplay.Components;
namespace Tests.Systems
{
    [TestFixture]
    public class BoundarySystemTests
    {
        private TestWorld _testWorld;
        private BoundarySystem _boundarySystem;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new TestWorld();
            _boundarySystem = new BoundarySystem(
                _testWorld.World,
                _testWorld.EventBus,
                new BoundaryConfig(800, 600)
                {
                    DefaultBehavior = BoundaryBehavior.Clamp
                }
            );
        }

        [TearDown]
        public void TearDown()
        {
            _testWorld.Dispose();
        }

        [Test]
        public void Update_WithCustomBoundaryBehavior_UsesCustomBehavior()
        {
            // Arrange - entity with custom wrap behavior
            var entity = _testWorld.World.Create(
                new Transform(new Vector2(-10, 300)), // Off left edge
                new BoxCollider(new Vector2(20, 20)),
                new CustomBoundaryBehavior(BoundaryBehavior.Wrap)
            );

            // Act
            _boundarySystem.Update(deltaTime: 0.016f);

            // Assert - should wrap to right side
            var transform = entity.GetComponent<Transform>(_testWorld.World);
            transform.Position.X.Should().BeApproximately(800, 1f,
                "entity should wrap to opposite side");
        }

        [Test]
        public void Update_WithDestroyBehavior_DestroysEntity()
        {
            // This test will FAIL with current code - that's the bug!

            // Arrange
            var boundarySystem = new BoundarySystem(
                _testWorld.World,
                _testWorld.EventBus,
                new BoundaryConfig(800, 600)
                {
                    ProjectileBehavior = BoundaryBehavior.Destroy
                }
            );

            var bullet = _testWorld.World.Create(
                new Transform(new Vector2(-10, 300)), // Off screen
                new BoxCollider(new Vector2(5, 5)),
                new Projectile() // Projectiles should be destroyed
            );

            var destructionSystem = new DestructionSystem(_testWorld.World, _testWorld.EventBus);

            // Act
            boundarySystem.Update(deltaTime: 0.016f);
            destructionSystem.Update(deltaTime: 0.016f);

            // Assert
            bullet.ShouldBeDestroyed(_testWorld.World,
                "projectile should be destroyed at boundary");
        }
    }
}
