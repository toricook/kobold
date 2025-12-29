using FluentAssertions;
using Kobold.Core.Components;
using Kobold.Core.Components.Gameplay;
using Kobold.Core.Systems;
using NUnit.Framework;
using System.Numerics;
using Tests.Helpers;

namespace Tests.Systems
{
    [TestFixture]
    public class CollisionSystemTests
    {
        private TestWorld _testWorld;
        private CollisionSystem _collisionSystem;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new TestWorld();
            _collisionSystem = new CollisionSystem(_testWorld.World, _testWorld.EventBus);
        }

        [TearDown]
        public void TearDown()
        {
            _testWorld.Dispose();
        }

        [Test]
        public void Update_WithOverlappingEntities_PublishesCollisionEvent()
        {
            // Arrange
            var entity1 = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Default)
            );

            var entity2 = _testWorld.World.Create(
                new Transform(new Vector2(10, 10)), // Overlapping
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Default)
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert
            var collisionEvent = _testWorld.GetLastEvent<CollisionEvent>();
            collisionEvent.Should().NotBeNull("collision should be detected");
            (collisionEvent.Entity1 == entity1 || collisionEvent.Entity1 == entity2).Should().BeTrue();
            (collisionEvent.Entity2 == entity1 || collisionEvent.Entity2 == entity2).Should().BeTrue();
        }

        [Test]
        public void Update_WithNonOverlappingEntities_NoCollisionEvent()
        {
            // Arrange
            var entity1 = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Default)
            );

            var entity2 = _testWorld.World.Create(
                new Transform(new Vector2(100, 100)), // Far apart
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Default)
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert
            var collisionEvent = _testWorld.GetLastEvent<CollisionEvent>();
            collisionEvent.Should().BeNull("no collision should occur");
        }

        [Test]
        public void Update_WithCollisionLayers_RespectsCollisionMatrix()
        {
            // Arrange - Player projectile and enemy
            var playerProjectile = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new BoxCollider(new Vector2(5, 5)),
                new CollisionLayerComponent(CollisionLayer.PlayerProjectile)
            );

            var enemy = _testWorld.World.Create(
                new Transform(new Vector2(2, 2)), // Overlapping
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Enemy)
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert - Player projectiles should collide with enemies
            var collisionEvent = _testWorld.GetLastEvent<CollisionEvent>();
            collisionEvent.Should().NotBeNull("player projectile should collide with enemy");
        }

        [Test]
        public void Update_WithNonCollidingLayers_NoCollisionEvent()
        {
            // Arrange - Two player projectiles (shouldn't collide with each other)
            var config = new CollisionConfig();
            config.CollisionMatrix.SetCollision(CollisionLayer.PlayerProjectile, CollisionLayer.PlayerProjectile, false);
            var collisionSystem = new CollisionSystem(_testWorld.World, _testWorld.EventBus, config);

            var projectile1 = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new BoxCollider(new Vector2(5, 5)),
                new CollisionLayerComponent(CollisionLayer.PlayerProjectile)
            );

            var projectile2 = _testWorld.World.Create(
                new Transform(new Vector2(2, 2)), // Overlapping
                new BoxCollider(new Vector2(5, 5)),
                new CollisionLayerComponent(CollisionLayer.PlayerProjectile)
            );

            // Act
            collisionSystem.Update(0.016f);

            // Assert - Player projectiles shouldn't collide with each other
            var collisionEvent = _testWorld.GetLastEvent<CollisionEvent>();
            collisionEvent.Should().BeNull("projectiles on same layer should not collide");
        }

        [Test]
        public void Update_WithCollisionResponse_ModifiesVelocities()
        {
            // Arrange
            var config = new CollisionConfig { EnableCollisionResponse = true };
            var collisionSystem = new CollisionSystem(_testWorld.World, _testWorld.EventBus, config);

            var entity1 = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Default),
                new Velocity(new Vector2(100, 0)), // Moving right
                new Physics()
            );

            var entity2 = _testWorld.World.Create(
                new Transform(new Vector2(10, 0)), // Overlapping
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Default),
                new Velocity(new Vector2(-100, 0)), // Moving left
                new Physics()
            );

            var initialVelocity1 = entity1.GetComponent<Velocity>(_testWorld.World).Value;
            var initialVelocity2 = entity2.GetComponent<Velocity>(_testWorld.World).Value;

            // Act
            collisionSystem.Update(0.016f);

            // Assert - Velocities should change due to collision response
            var finalVelocity1 = entity1.GetComponent<Velocity>(_testWorld.World).Value;
            var finalVelocity2 = entity2.GetComponent<Velocity>(_testWorld.World).Value;

            // At least one velocity should have changed
            bool velocitiesChanged = finalVelocity1 != initialVelocity1 || finalVelocity2 != initialVelocity2;
            velocitiesChanged.Should().BeTrue("collision response should modify velocities");
        }

        [Test]
        public void Update_WithPendingDestruction_IgnoresEntity()
        {
            // Arrange
            var entity1 = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Default),
                new PendingDestruction { TimeRemaining = 1.0f }
            );

            var entity2 = _testWorld.World.Create(
                new Transform(new Vector2(10, 10)), // Overlapping
                new BoxCollider(new Vector2(20, 20)),
                new CollisionLayerComponent(CollisionLayer.Default)
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert - No collision should be detected
            var collisionEvent = _testWorld.GetLastEvent<CollisionEvent>();
            collisionEvent.Should().BeNull("entities marked for destruction should be ignored");
        }

        [Test]
        public void AreEntitiesColliding_WithOverlappingEntities_ReturnsTrue()
        {
            // Arrange
            var entity1 = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new BoxCollider(new Vector2(20, 20))
            );

            var entity2 = _testWorld.World.Create(
                new Transform(new Vector2(10, 10)), // Overlapping
                new BoxCollider(new Vector2(20, 20))
            );

            // Act
            var areColliding = _collisionSystem.AreEntitiesColliding(entity1, entity2);

            // Assert
            areColliding.Should().BeTrue("entities are overlapping");
        }

        [Test]
        public void AreEntitiesColliding_WithNonOverlappingEntities_ReturnsFalse()
        {
            // Arrange
            var entity1 = _testWorld.World.Create(
                new Transform(new Vector2(0, 0)),
                new BoxCollider(new Vector2(20, 20))
            );

            var entity2 = _testWorld.World.Create(
                new Transform(new Vector2(100, 100)), // Far apart
                new BoxCollider(new Vector2(20, 20))
            );

            // Act
            var areColliding = _collisionSystem.AreEntitiesColliding(entity1, entity2);

            // Assert
            areColliding.Should().BeFalse("entities are not overlapping");
        }
    }
}
