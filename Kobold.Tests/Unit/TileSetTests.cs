using FluentAssertions;
using Kobold.Extensions.Tilemaps;
using NUnit.Framework;

namespace Tests.Unit
{
    [TestFixture]
    public class TileSetTests
    {
        [Test]
        public void Constructor_CreatesValidTileSet()
        {
            // Arrange & Act
            var tileSet = new TileSet(tileWidth: 16, tileHeight: 16, spacing: 1, margin: 2);

            // Assert
            tileSet.TileWidth.Should().Be(16);
            tileSet.TileHeight.Should().Be(16);
            tileSet.Spacing.Should().Be(1);
            tileSet.Margin.Should().Be(2);
            tileSet.TileCount.Should().Be(0);
        }

        [Test]
        public void SetTileProperties_StoresProperties()
        {
            // Arrange
            var tileSet = new TileSet(16, 16);
            var props = new TileProperties { IsSolid = true, Damage = 10 };

            // Act
            tileSet.SetTileProperties(5, props);

            // Assert
            var retrieved = tileSet.GetTileProperties(5);
            retrieved.IsSolid.Should().BeTrue();
            retrieved.Damage.Should().Be(10);
        }

        [Test]
        public void SetTileProperties_UpdatesTileCount()
        {
            // Arrange
            var tileSet = new TileSet(16, 16);

            // Act
            tileSet.SetTileProperties(0, TileProperties.Solid());
            tileSet.SetTileProperties(5, TileProperties.Solid());
            tileSet.SetTileProperties(10, TileProperties.Solid());

            // Assert
            tileSet.TileCount.Should().Be(11, "tile count should be highest ID + 1");
        }

        [Test]
        public void GetTileProperties_ReturnsDefaultForUnconfiguredTile()
        {
            // Arrange
            var tileSet = new TileSet(16, 16);

            // Act
            var props = tileSet.GetTileProperties(99);

            // Assert
            props.IsSolid.Should().BeFalse();
            props.Damage.Should().Be(0);
            props.Friction.Should().Be(1.0f);
        }

        [Test]
        public void IsSolid_ReturnsCorrectValue()
        {
            // Arrange
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, TileProperties.Solid());
            tileSet.SetTileProperties(2, new TileProperties { IsSolid = false });

            // Act & Assert
            tileSet.IsSolid(1).Should().BeTrue();
            tileSet.IsSolid(2).Should().BeFalse();
            tileSet.IsSolid(99).Should().BeFalse();
        }

        [Test]
        public void GetCollisionLayer_ReturnsCorrectLayer()
        {
            // Arrange
            var tileSet = new TileSet(16, 16);
            tileSet.SetTileProperties(1, new TileProperties
            {
                CollisionLayer = TileCollisionLayer.Solid
            });
            tileSet.SetTileProperties(2, new TileProperties
            {
                CollisionLayer = TileCollisionLayer.Platform
            });

            // Act & Assert
            tileSet.GetCollisionLayer(1).Should().Be(TileCollisionLayer.Solid);
            tileSet.GetCollisionLayer(2).Should().Be(TileCollisionLayer.Platform);
            tileSet.GetCollisionLayer(99).Should().Be(TileCollisionLayer.None);
        }

        [Test]
        public void TileProperties_Solid_CreatesSolidTile()
        {
            // Act
            var props = TileProperties.Solid();

            // Assert
            props.IsSolid.Should().BeTrue();
            props.CollisionLayer.Should().Be(TileCollisionLayer.Solid);
        }

        [Test]
        public void TileProperties_Platform_CreatesPlatformTile()
        {
            // Act
            var props = TileProperties.Platform();

            // Assert
            props.IsSolid.Should().BeFalse();
            props.CollisionLayer.Should().Be(TileCollisionLayer.Platform);
        }

        [Test]
        public void TileProperties_WithDamage_CreatesDamageTile()
        {
            // Act
            var props = TileProperties.WithDamage(15, "lava");

            // Assert
            props.IsSolid.Should().BeFalse();
            props.Damage.Should().Be(15);
            props.Type.Should().Be("lava");
        }

        [Test]
        public void GetTileSourceRect_CalculatesCorrectPosition()
        {
            // Arrange
            var tileSet = new TileSet(tileWidth: 16, tileHeight: 16, spacing: 1, margin: 2);

            // Act
            var (x, y, width, height) = tileSet.GetTileSourceRect(tileId: 0, tilesPerRow: 8);

            // Assert - First tile
            x.Should().Be(2); // margin
            y.Should().Be(2); // margin
            width.Should().Be(16);
            height.Should().Be(16);
        }

        [Test]
        public void GetTileSourceRect_CalculatesWithSpacing()
        {
            // Arrange
            var tileSet = new TileSet(tileWidth: 16, tileHeight: 16, spacing: 1, margin: 2);

            // Act - Tile 1 (second tile in first row)
            var (x, y, width, height) = tileSet.GetTileSourceRect(tileId: 1, tilesPerRow: 8);

            // Assert
            x.Should().Be(2 + 1 * (16 + 1)); // margin + column * (tileWidth + spacing)
            y.Should().Be(2); // margin
            width.Should().Be(16);
            height.Should().Be(16);
        }

        [Test]
        public void GetTileSourceRect_CalculatesMultipleRows()
        {
            // Arrange
            var tileSet = new TileSet(tileWidth: 16, tileHeight: 16, spacing: 1, margin: 2);

            // Act - Tile 8 (first tile in second row)
            var (x, y, width, height) = tileSet.GetTileSourceRect(tileId: 8, tilesPerRow: 8);

            // Assert
            x.Should().Be(2); // back to first column
            y.Should().Be(2 + 1 * (16 + 1)); // margin + row * (tileHeight + spacing)
            width.Should().Be(16);
            height.Should().Be(16);
        }

        [Test]
        public void FromTexture_CalculatesTileCount()
        {
            // Arrange & Act
            var tileSet = TileSet.FromTexture(
                texturePath: "test.png",
                textureWidth: 256,
                textureHeight: 128,
                tileWidth: 16,
                tileHeight: 16,
                spacing: 0,
                margin: 0);

            // Assert
            tileSet.TileCount.Should().Be(128); // (256/16) * (128/16) = 16 * 8
            tileSet.TexturePath.Should().Be("test.png");
        }

        [Test]
        public void FromTexture_WithSpacingAndMargin_CalculatesCorrectly()
        {
            // Arrange & Act
            var tileSet = TileSet.FromTexture(
                texturePath: "test.png",
                textureWidth: 256,
                textureHeight: 128,
                tileWidth: 16,
                tileHeight: 16,
                spacing: 1,
                margin: 2);

            // Assert
            // (256 - 2*2 + 1) / (16 + 1) = 253 / 17 = 14 tiles per row
            // (128 - 2*2 + 1) / (16 + 1) = 125 / 17 = 7 tiles per column
            tileSet.TileCount.Should().Be(98); // 14 * 7
        }

        [Test]
        public void SetTileRange_ConfiguresMultipleTiles()
        {
            // Arrange
            var tileSet = new TileSet(16, 16);
            var props = TileProperties.Solid();

            // Act
            tileSet.SetTileRange(startTileId: 10, count: 5, properties: props);

            // Assert
            for (int i = 10; i < 15; i++)
            {
                tileSet.IsSolid(i).Should().BeTrue($"tile {i} should be solid");
            }
            tileSet.IsSolid(9).Should().BeFalse("tile before range should not be affected");
            tileSet.IsSolid(15).Should().BeFalse("tile after range should not be affected");
        }
    }
}
