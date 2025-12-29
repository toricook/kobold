using FluentAssertions;
using Kobold.Core.Components;
using Kobold.Extensions.Tilemaps;
using NUnit.Framework;
using System.Numerics;
using Tests.Helpers;

namespace Tests.Systems
{
    [TestFixture]
    public class TilemapCollisionSystemTests
    {
        private TestWorld _testWorld;
        private TilemapCollisionSystem _collisionSystem;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new TestWorld();
            _collisionSystem = new TilemapCollisionSystem(_testWorld.World, _testWorld.EventBus);
        }

        [TearDown]
        public void TearDown()
        {
            _testWorld.Dispose();
        }

        [Test]
        public void Update_WithNoCollision_DoesNotChangePosition()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
            );

            var player = _testWorld.World.Create(
                new Transform(new Vector2(50, 50)),
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(Vector2.Zero)
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert
            player.ShouldBeAt(_testWorld.World, new Vector2(50, 50));
        }

        [Test]
        public void Update_WithSolidTileCollision_PushesEntityOut()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, TileProperties.Solid());

            // Place solid tile at (5, 5) - world position (80, 80)
            tileMap.SetTile(0, 5, 5, tileId: 1);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
            );

            // Place entity overlapping with the tile
            var player = _testWorld.World.Create(
                new Transform(new Vector2(75, 80)), // Overlapping from left
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(new Vector2(10, 0))
            );

            var initialPos = player.GetComponent<Transform>(_testWorld.World).Position;

            // Act
            _collisionSystem.Update(0.016f);

            // Assert - Entity should be pushed out
            var finalPos = player.GetComponent<Transform>(_testWorld.World).Position;
            finalPos.Should().NotBe(initialPos, "entity should be moved to resolve collision");
        }

        [Test]
        public void Update_WithSolidTile_StopsVelocityInCollisionDirection()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, TileProperties.Solid());
            tileMap.SetTile(0, 5, 5, tileId: 1);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
            );

            var player = _testWorld.World.Create(
                new Transform(new Vector2(75, 80)),
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(new Vector2(10, 5)) // Moving right and down
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert - Horizontal velocity should be stopped/reduced
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.X.Should().BeLessOrEqualTo(0, "horizontal velocity should be stopped");
        }

        [Test]
        public void Update_WithPlatformTile_OnlyCollidesFromAbove()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, TileProperties.Platform());

            // Platform at y=5 (world position y=80)
            tileMap.SetTile(0, 5, 5, tileId: 1);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
            );

            // Entity falling onto platform from above
            // Position entity so its bottom (y + height) is at or just past platform top (80)
            var player = _testWorld.World.Create(
                new Transform(new Vector2(80, 64)), // Bottom will be at 80 (64 + 16)
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(new Vector2(0, 5)) // Moving down
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert - Should land on platform
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Y.Should().Be(0, "downward velocity should be stopped");

            // Check that entity was snapped to platform top
            var finalPos = player.GetComponent<Transform>(_testWorld.World).Position;
            finalPos.Y.Should().Be(64, "entity should be snapped to platform top (80 - 16 collider height)");
        }

        [Test]
        public void Update_WithPlatformTile_DoesNotCollideFromBelow()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, TileProperties.Platform());
            tileMap.SetTile(0, 5, 5, tileId: 1);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
            );

            // Entity moving up into platform (should pass through)
            var player = _testWorld.World.Create(
                new Transform(new Vector2(80, 98)), // Below platform
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(new Vector2(0, -5)) // Moving up
            );

            var initialY = player.GetComponent<Transform>(_testWorld.World).Position.Y;

            // Act
            _collisionSystem.Update(0.016f);

            // Assert - Should pass through
            var finalPos = player.GetComponent<Transform>(_testWorld.World).Position;
            finalPos.Y.Should().Be(initialY, "entity should not collide when moving upward");
        }

        [Test]
        public void Update_WithTriggerTile_PublishesTriggerEvent()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, new TileProperties
            {
                CollisionLayer = TileCollisionLayer.Trigger,
                Type = "checkpoint"
            });
            tileMap.SetTile(0, 5, 5, tileId: 1);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
            );

            var player = _testWorld.World.Create(
                new Transform(new Vector2(80, 80)), // On trigger tile
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(Vector2.Zero)
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert
            var triggerEvent = _testWorld.GetLastEvent<TileTriggerEvent>();
            triggerEvent.Should().NotBeNull();
            triggerEvent.Entity.Should().Be(player);
            triggerEvent.TriggerType.Should().Be("checkpoint");
        }

        [Test]
        public void Update_WithDamageTile_PublishesDamageEvent()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, new TileProperties
            {
                CollisionLayer = TileCollisionLayer.Trigger,
                Damage = 15,
                Type = "spike"
            });
            tileMap.SetTile(0, 5, 5, tileId: 1);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
            );

            var player = _testWorld.World.Create(
                new Transform(new Vector2(80, 80)),
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(Vector2.Zero)
            );

            // Act
            _collisionSystem.Update(0.016f);

            // Assert
            var damageEvent = _testWorld.GetLastEvent<TileDamageEvent>();
            damageEvent.Should().NotBeNull();
            damageEvent.Entity.Should().Be(player);
            damageEvent.Damage.Should().Be(15);
            damageEvent.DamageType.Should().Be("spike");
        }

        [Test]
        public void Update_WithDisabledCollision_DoesNotProcessCollision()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, TileProperties.Solid());
            tileMap.SetTile(0, 5, 5, tileId: 1);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = false } // Disabled
            );

            var player = _testWorld.World.Create(
                new Transform(new Vector2(75, 80)),
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(new Vector2(10, 0))
            );

            var initialPos = player.GetComponent<Transform>(_testWorld.World).Position;

            // Act
            _collisionSystem.Update(0.016f);

            // Assert - Position should not change
            player.ShouldBeAt(_testWorld.World, initialPos);
        }

        [Test]
        public void IsPositionSolid_ReturnsTrueForSolidTile()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, TileProperties.Solid());
            tileMap.SetTile(0, 5, 5, tileId: 1);

            // Act
            var isSolid = _collisionSystem.IsPositionSolid(
                new Vector2(85, 85), // Middle of tile at (5,5)
                tileMap,
                tileSet,
                Vector2.Zero);

            // Assert
            isSolid.Should().BeTrue();
        }

        [Test]
        public void IsPositionSolid_ReturnsFalseForEmptyTile()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            // Act
            var isSolid = _collisionSystem.IsPositionSolid(
                new Vector2(85, 85),
                tileMap,
                tileSet,
                Vector2.Zero);

            // Assert
            isSolid.Should().BeFalse();
        }

        [Test]
        public void Update_WithMultipleLayers_ChecksAllLayers()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16, layerCount: 2);
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, TileProperties.Solid());

            // Solid tile only on layer 1
            tileMap.SetTile(1, 5, 5, tileId: 1);

            _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
            );

            var player = _testWorld.World.Create(
                new Transform(new Vector2(75, 80)),
                new BoxCollider(new Vector2(16, 16)),
                new Velocity(new Vector2(10, 0))
            );

            var initialPos = player.GetComponent<Transform>(_testWorld.World).Position;

            // Act
            _collisionSystem.Update(0.016f);

            // Assert - Should still collide even though tile is on layer 1
            var finalPos = player.GetComponent<Transform>(_testWorld.World).Position;
            finalPos.Should().NotBe(initialPos, "should collide with tiles on any layer");
        }
    }
}
