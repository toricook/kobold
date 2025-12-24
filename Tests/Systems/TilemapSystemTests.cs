using FluentAssertions;
using Kobold.Core.Components;
using Kobold.Extensions.Tilemaps;
using NUnit.Framework;
using System.Numerics;
using Tests.Helpers;

namespace Tests.Systems
{
    [TestFixture]
    public class TilemapSystemTests
    {
        private TestWorld _testWorld;
        private TilemapSystem _tilemapSystem;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new TestWorld();
            _tilemapSystem = new TilemapSystem(_testWorld.World);
        }

        [TearDown]
        public void TearDown()
        {
            _testWorld.Dispose();
        }

        [Test]
        public void Update_WithVisibleTilemap_Succeeds()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet)
                {
                    Visible = true
                }
            );

            // Act - should not throw
            _tilemapSystem.Update(0.016f);

            // Assert
            entity.ShouldBeAlive(_testWorld.World);
        }

        [Test]
        public void Update_WithInvisibleTilemap_SkipsProcessing()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet)
                {
                    Visible = false
                }
            );

            // Act - should not throw
            _tilemapSystem.Update(0.016f);

            // Assert
            entity.ShouldBeAlive(_testWorld.World);
        }

        [Test]
        public void GetVisibleTiles_ReturnsOnlyTilesInViewport()
        {
            // Arrange
            var tileMap = new TileMap(100, 100, 16, 16);

            // Fill some tiles
            tileMap.Fill(layer: 0, x: 0, y: 0, width: 10, height: 10, tileId: 1);
            tileMap.Fill(layer: 0, x: 50, y: 50, width: 10, height: 10, tileId: 2);

            // Act - Get tiles visible in a small viewport at origin
            var visibleTiles = TilemapSystem.GetVisibleTiles(
                tileMap,
                layer: 0,
                cameraX: 0,
                cameraY: 0,
                viewportWidth: 160, // 10 tiles wide
                viewportHeight: 160  // 10 tiles tall
            );

            // Assert - Should only see the first filled region
            visibleTiles.Should().NotBeEmpty();
            visibleTiles.Should().AllSatisfy(tile =>
            {
                tile.x.Should().BeLessThan(11);
                tile.y.Should().BeLessThan(11);
                tile.tileId.Should().Be(1);
            });
        }

        [Test]
        public void GetVisibleTiles_WithOffsetCamera_ReturnsCorrectTiles()
        {
            // Arrange
            var tileMap = new TileMap(100, 100, 16, 16);
            tileMap.Fill(layer: 0, x: 50, y: 50, width: 10, height: 10, tileId: 42);

            // Act - Move camera to see the filled region
            var visibleTiles = TilemapSystem.GetVisibleTiles(
                tileMap,
                layer: 0,
                cameraX: 800, // 50 * 16
                cameraY: 800, // 50 * 16
                viewportWidth: 160,
                viewportHeight: 160
            );

            // Assert
            visibleTiles.Should().NotBeEmpty();
            visibleTiles.Should().AllSatisfy(tile =>
            {
                tile.tileId.Should().Be(42);
            });
        }

        [Test]
        public void IsTilemapVisible_ReturnsTrueForVisibleTilemap()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet)
                {
                    Visible = true
                }
            );

            // Act
            var isVisible = _tilemapSystem.IsTilemapVisible(entity);

            // Assert
            isVisible.Should().BeTrue();
        }

        [Test]
        public void IsTilemapVisible_ReturnsFalseForInvisibleTilemap()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet)
                {
                    Visible = false
                }
            );

            // Act
            var isVisible = _tilemapSystem.IsTilemapVisible(entity);

            // Assert
            isVisible.Should().BeFalse();
        }

        [Test]
        public void SetTilemapVisibility_ChangesVisibility()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet)
                {
                    Visible = true
                }
            );

            // Act
            _tilemapSystem.SetTilemapVisibility(entity, false);

            // Assert
            var component = entity.GetComponent<TilemapComponent>(_testWorld.World);
            component.Visible.Should().BeFalse();
        }

        [Test]
        public void SetTilemapOpacity_ChangesOpacity()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet)
                {
                    Opacity = 1.0f
                }
            );

            // Act
            _tilemapSystem.SetTilemapOpacity(entity, 0.5f);

            // Assert
            var component = entity.GetComponent<TilemapComponent>(_testWorld.World);
            component.Opacity.Should().Be(0.5f);
        }

        [Test]
        public void SetTilemapOpacity_ClampsToValidRange()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            var tileSet = new TileSet(16, 16);

            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new TilemapComponent(tileMap, tileSet)
            );

            // Act - Try to set opacity outside valid range
            _tilemapSystem.SetTilemapOpacity(entity, 1.5f);
            var component1 = entity.GetComponent<TilemapComponent>(_testWorld.World);
            component1.Opacity.Should().Be(1.0f, "opacity should be clamped to 1.0");

            _tilemapSystem.SetTilemapOpacity(entity, -0.5f);
            var component2 = entity.GetComponent<TilemapComponent>(_testWorld.World);
            component2.Opacity.Should().Be(0.0f, "opacity should be clamped to 0.0");
        }
    }
}
