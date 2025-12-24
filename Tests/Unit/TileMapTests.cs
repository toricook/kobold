using FluentAssertions;
using Kobold.Extensions.Tilemaps;
using NUnit.Framework;

namespace Tests.Unit
{
    [TestFixture]
    public class TileMapTests
    {
        [Test]
        public void Constructor_CreatesValidTileMap()
        {
            // Arrange & Act
            var tileMap = new TileMap(
                width: 10,
                height: 10,
                tileWidth: 16,
                tileHeight: 16,
                layerCount: 2);

            // Assert
            tileMap.Width.Should().Be(10);
            tileMap.Height.Should().Be(10);
            tileMap.TileWidth.Should().Be(16);
            tileMap.TileHeight.Should().Be(16);
            tileMap.LayerCount.Should().Be(2);
        }

        [Test]
        public void Constructor_InitializesAllTilesToEmpty()
        {
            // Arrange & Act
            var tileMap = new TileMap(5, 5, 16, 16);

            // Assert
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    tileMap.GetTile(0, x, y).Should().Be(-1, "all tiles should start empty");
                }
            }
        }

        [Test]
        public void SetTile_StoresAndReturnsTileId()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);

            // Act
            tileMap.SetTile(layer: 0, x: 5, y: 5, tileId: 42);

            // Assert
            tileMap.GetTile(layer: 0, x: 5, y: 5).Should().Be(42);
        }

        [Test]
        public void SetTile_OnMultipleLayers_StoresIndependently()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16, layerCount: 3);

            // Act
            tileMap.SetTile(layer: 0, x: 5, y: 5, tileId: 10);
            tileMap.SetTile(layer: 1, x: 5, y: 5, tileId: 20);
            tileMap.SetTile(layer: 2, x: 5, y: 5, tileId: 30);

            // Assert
            tileMap.GetTile(layer: 0, x: 5, y: 5).Should().Be(10);
            tileMap.GetTile(layer: 1, x: 5, y: 5).Should().Be(20);
            tileMap.GetTile(layer: 2, x: 5, y: 5).Should().Be(30);
        }

        [Test]
        public void GetTile_OutOfBounds_ReturnsMinusOne()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);

            // Act & Assert
            tileMap.GetTile(layer: 0, x: -1, y: 5).Should().Be(-1);
            tileMap.GetTile(layer: 0, x: 10, y: 5).Should().Be(-1);
            tileMap.GetTile(layer: 0, x: 5, y: -1).Should().Be(-1);
            tileMap.GetTile(layer: 0, x: 5, y: 10).Should().Be(-1);
        }

        [Test]
        public void Fill_FillsRectangularRegion()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);

            // Act
            tileMap.Fill(layer: 0, x: 2, y: 2, width: 3, height: 3, tileId: 5);

            // Assert
            for (int x = 2; x < 5; x++)
            {
                for (int y = 2; y < 5; y++)
                {
                    tileMap.GetTile(0, x, y).Should().Be(5, $"tile at ({x}, {y}) should be filled");
                }
            }

            // Outside the fill region should still be empty
            tileMap.GetTile(0, 1, 1).Should().Be(-1);
            tileMap.GetTile(0, 5, 5).Should().Be(-1);
        }

        [Test]
        public void ClearLayer_SetsAllTilesToEmpty()
        {
            // Arrange
            var tileMap = new TileMap(5, 5, 16, 16);
            tileMap.Fill(layer: 0, x: 0, y: 0, width: 5, height: 5, tileId: 1);

            // Act
            tileMap.ClearLayer(layer: 0);

            // Assert
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    tileMap.GetTile(0, x, y).Should().Be(-1);
                }
            }
        }

        [Test]
        public void Clear_ClearsAllLayers()
        {
            // Arrange
            var tileMap = new TileMap(5, 5, 16, 16, layerCount: 2);
            tileMap.Fill(layer: 0, x: 0, y: 0, width: 5, height: 5, tileId: 1);
            tileMap.Fill(layer: 1, x: 0, y: 0, width: 5, height: 5, tileId: 2);

            // Act
            tileMap.Clear();

            // Assert
            for (int layer = 0; layer < 2; layer++)
            {
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        tileMap.GetTile(layer, x, y).Should().Be(-1);
                    }
                }
            }
        }

        [Test]
        public void WorldToTile_ConvertsCorrectly()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);

            // Act & Assert
            var (tileX1, tileY1) = tileMap.WorldToTile(0f, 0f);
            tileX1.Should().Be(0);
            tileY1.Should().Be(0);

            var (tileX2, tileY2) = tileMap.WorldToTile(16f, 16f);
            tileX2.Should().Be(1);
            tileY2.Should().Be(1);

            var (tileX3, tileY3) = tileMap.WorldToTile(100f, 50f);
            tileX3.Should().Be(6);
            tileY3.Should().Be(3);
        }

        [Test]
        public void TileToWorld_ConvertsCorrectly()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);

            // Act & Assert
            var (worldX1, worldY1) = tileMap.TileToWorld(0, 0);
            worldX1.Should().Be(0f);
            worldY1.Should().Be(0f);

            var (worldX2, worldY2) = tileMap.TileToWorld(5, 3);
            worldX2.Should().Be(80f);
            worldY2.Should().Be(48f);
        }

        [Test]
        public void TileToWorldCenter_ReturnsCenterOfTile()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);

            // Act
            var (worldX, worldY) = tileMap.TileToWorldCenter(0, 0);

            // Assert
            worldX.Should().Be(8f); // 0 + 16/2
            worldY.Should().Be(8f); // 0 + 16/2
        }

        [Test]
        public void GetTilesInBounds_ReturnsOnlyNonEmptyTiles()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);
            tileMap.SetTile(0, 2, 2, 1);
            tileMap.SetTile(0, 3, 2, 2);
            tileMap.SetTile(0, 2, 3, 3);

            // Act
            var tiles = tileMap.GetTilesInBounds(layer: 0, x: 0, y: 0, width: 5, height: 5);

            // Assert
            tiles.Count.Should().Be(3);
            tiles.Should().Contain((2, 2, 1));
            tiles.Should().Contain((3, 2, 2));
            tiles.Should().Contain((2, 3, 3));
        }

        [Test]
        public void IsValidPosition_ChecksBounds()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16);

            // Act & Assert
            tileMap.IsValidPosition(0, 0).Should().BeTrue();
            tileMap.IsValidPosition(9, 9).Should().BeTrue();
            tileMap.IsValidPosition(-1, 5).Should().BeFalse();
            tileMap.IsValidPosition(10, 5).Should().BeFalse();
            tileMap.IsValidPosition(5, -1).Should().BeFalse();
            tileMap.IsValidPosition(5, 10).Should().BeFalse();
        }

        [Test]
        public void IsValidLayer_ChecksLayerBounds()
        {
            // Arrange
            var tileMap = new TileMap(10, 10, 16, 16, layerCount: 3);

            // Act & Assert
            tileMap.IsValidLayer(0).Should().BeTrue();
            tileMap.IsValidLayer(2).Should().BeTrue();
            tileMap.IsValidLayer(-1).Should().BeFalse();
            tileMap.IsValidLayer(3).Should().BeFalse();
        }

        [Test]
        public void GetPixelSize_ReturnsCorrectDimensions()
        {
            // Arrange
            var tileMap = new TileMap(10, 15, 16, 16);

            // Act
            var (width, height) = tileMap.GetPixelSize();

            // Assert
            width.Should().Be(160); // 10 * 16
            height.Should().Be(240); // 15 * 16
        }

        [Test]
        public void Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new TileMap(5, 5, 16, 16);
            original.SetTile(0, 2, 2, 42);

            // Act
            var clone = original.Clone();
            clone.SetTile(0, 3, 3, 99);

            // Assert
            clone.GetTile(0, 2, 2).Should().Be(42, "clone should have copied tiles");
            clone.GetTile(0, 3, 3).Should().Be(99, "clone should be independent");
            original.GetTile(0, 3, 3).Should().Be(-1, "original should be unchanged");
        }
    }
}
