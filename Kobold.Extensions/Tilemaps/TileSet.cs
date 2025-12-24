using System;
using System.Collections.Generic;

namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// Represents a collection of tiles with their properties.
    /// Maps tile IDs to their visual and gameplay properties.
    /// </summary>
    public class TileSet
    {
        private readonly Dictionary<int, TileProperties> _tileProperties;

        public int TileWidth { get; }
        public int TileHeight { get; }
        public int TileCount { get; private set; }
        public int Spacing { get; }
        public int Margin { get; }

        /// <summary>
        /// Optional texture/sprite sheet name or path for rendering.
        /// The actual texture loading is handled by the platform-specific renderer.
        /// </summary>
        public string? TexturePath { get; set; }

        /// <summary>
        /// Creates a new tileset.
        /// </summary>
        /// <param name="tileWidth">Width of each tile in pixels</param>
        /// <param name="tileHeight">Height of each tile in pixels</param>
        /// <param name="spacing">Spacing between tiles in the tileset image (default: 0)</param>
        /// <param name="margin">Margin around the tileset image (default: 0)</param>
        public TileSet(int tileWidth, int tileHeight, int spacing = 0, int margin = 0)
        {
            if (tileWidth <= 0) throw new ArgumentException("TileWidth must be greater than 0", nameof(tileWidth));
            if (tileHeight <= 0) throw new ArgumentException("TileHeight must be greater than 0", nameof(tileHeight));
            if (spacing < 0) throw new ArgumentException("Spacing cannot be negative", nameof(spacing));
            if (margin < 0) throw new ArgumentException("Margin cannot be negative", nameof(margin));

            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Spacing = spacing;
            Margin = margin;
            TileCount = 0;

            _tileProperties = new Dictionary<int, TileProperties>();
        }

        /// <summary>
        /// Sets properties for a specific tile ID.
        /// </summary>
        /// <param name="tileId">Tile ID</param>
        /// <param name="properties">Tile properties</param>
        public void SetTileProperties(int tileId, TileProperties properties)
        {
            if (tileId < 0)
                throw new ArgumentException("Tile ID cannot be negative", nameof(tileId));

            _tileProperties[tileId] = properties;

            // Update tile count
            if (tileId >= TileCount)
                TileCount = tileId + 1;
        }

        /// <summary>
        /// Gets properties for a specific tile ID.
        /// Returns default properties if the tile ID hasn't been configured.
        /// </summary>
        /// <param name="tileId">Tile ID</param>
        /// <returns>Tile properties</returns>
        public TileProperties GetTileProperties(int tileId)
        {
            if (tileId < 0)
                return new TileProperties(); // Empty tile

            if (_tileProperties.TryGetValue(tileId, out var properties))
                return properties;

            return new TileProperties(); // Default properties
        }

        /// <summary>
        /// Checks if a tile is solid (blocks movement).
        /// </summary>
        public bool IsSolid(int tileId)
        {
            return GetTileProperties(tileId).IsSolid;
        }

        /// <summary>
        /// Gets the collision layer for a tile.
        /// </summary>
        public TileCollisionLayer GetCollisionLayer(int tileId)
        {
            return GetTileProperties(tileId).CollisionLayer;
        }

        /// <summary>
        /// Calculates the source rectangle for a tile in a tileset image.
        /// Useful for sprite sheet rendering.
        /// </summary>
        /// <param name="tileId">Tile ID</param>
        /// <param name="tilesPerRow">Number of tiles per row in the tileset image</param>
        /// <returns>Source rectangle (x, y, width, height)</returns>
        public (int x, int y, int width, int height) GetTileSourceRect(int tileId, int tilesPerRow)
        {
            if (tileId < 0 || tilesPerRow <= 0)
                return (0, 0, 0, 0);

            int column = tileId % tilesPerRow;
            int row = tileId / tilesPerRow;

            int x = Margin + column * (TileWidth + Spacing);
            int y = Margin + row * (TileHeight + Spacing);

            return (x, y, TileWidth, TileHeight);
        }

        /// <summary>
        /// Creates a tileset from a texture with automatic tile calculation.
        /// </summary>
        /// <param name="texturePath">Path to the tileset texture</param>
        /// <param name="textureWidth">Width of the texture in pixels</param>
        /// <param name="textureHeight">Height of the texture in pixels</param>
        /// <param name="tileWidth">Width of each tile</param>
        /// <param name="tileHeight">Height of each tile</param>
        /// <param name="spacing">Spacing between tiles</param>
        /// <param name="margin">Margin around the tileset</param>
        /// <returns>New tileset</returns>
        public static TileSet FromTexture(
            string texturePath,
            int textureWidth,
            int textureHeight,
            int tileWidth,
            int tileHeight,
            int spacing = 0,
            int margin = 0)
        {
            var tileset = new TileSet(tileWidth, tileHeight, spacing, margin)
            {
                TexturePath = texturePath
            };

            // Calculate number of tiles
            int tilesPerRow = (textureWidth - 2 * margin + spacing) / (tileWidth + spacing);
            int tilesPerColumn = (textureHeight - 2 * margin + spacing) / (tileHeight + spacing);
            tileset.TileCount = tilesPerRow * tilesPerColumn;

            return tileset;
        }

        /// <summary>
        /// Configures multiple tiles with the same properties.
        /// </summary>
        /// <param name="startTileId">Starting tile ID</param>
        /// <param name="count">Number of tiles to configure</param>
        /// <param name="properties">Properties to apply</param>
        public void SetTileRange(int startTileId, int count, TileProperties properties)
        {
            for (int i = 0; i < count; i++)
            {
                SetTileProperties(startTileId + i, properties);
            }
        }
    }

    /// <summary>
    /// Properties associated with a tile.
    /// </summary>
    public struct TileProperties
    {
        /// <summary>
        /// Whether this tile blocks movement (collision).
        /// </summary>
        public bool IsSolid { get; set; }

        /// <summary>
        /// Collision layer for this tile.
        /// </summary>
        public TileCollisionLayer CollisionLayer { get; set; }

        /// <summary>
        /// Friction coefficient (0 = no friction, 1 = full friction).
        /// </summary>
        public float Friction { get; set; }

        /// <summary>
        /// Damage dealt to entities that touch this tile.
        /// </summary>
        public int Damage { get; set; }

        /// <summary>
        /// Type identifier for special tile behaviors (e.g., "water", "lava", "ice").
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Whether this tile is animated.
        /// </summary>
        public bool IsAnimated { get; set; }

        /// <summary>
        /// Animation frame IDs if this tile is animated.
        /// </summary>
        public int[]? AnimationFrames { get; set; }

        /// <summary>
        /// Animation speed in frames per second.
        /// </summary>
        public float AnimationSpeed { get; set; }

        /// <summary>
        /// Custom properties for game-specific data.
        /// </summary>
        public Dictionary<string, object>? CustomProperties { get; set; }

        /// <summary>
        /// Creates default tile properties (non-solid, no special behavior).
        /// </summary>
        public TileProperties()
        {
            IsSolid = false;
            CollisionLayer = TileCollisionLayer.None;
            Friction = 1.0f;
            Damage = 0;
            Type = null;
            IsAnimated = false;
            AnimationFrames = null;
            AnimationSpeed = 0;
            CustomProperties = null;
        }

        /// <summary>
        /// Creates a solid tile with default properties.
        /// </summary>
        public static TileProperties Solid()
        {
            return new TileProperties
            {
                IsSolid = true,
                CollisionLayer = TileCollisionLayer.Solid
            };
        }

        /// <summary>
        /// Creates a platform tile (one-way collision from above).
        /// </summary>
        public static TileProperties Platform()
        {
            return new TileProperties
            {
                IsSolid = false,
                CollisionLayer = TileCollisionLayer.Platform
            };
        }

        /// <summary>
        /// Creates a damage tile.
        /// </summary>
        public static TileProperties WithDamage(int damage, string type = "damage")
        {
            return new TileProperties
            {
                IsSolid = false,
                Damage = damage,
                Type = type
            };
        }
    }

    /// <summary>
    /// Collision layers for tiles.
    /// </summary>
    public enum TileCollisionLayer
    {
        /// <summary>No collision</summary>
        None = 0,

        /// <summary>Solid tile (blocks from all directions)</summary>
        Solid = 1,

        /// <summary>Platform (one-way collision from above)</summary>
        Platform = 2,

        /// <summary>Trigger/sensor (detects but doesn't block)</summary>
        Trigger = 3,

        /// <summary>Water (special movement behavior)</summary>
        Water = 4,

        /// <summary>Ice (reduced friction)</summary>
        Ice = 5,

        /// <summary>Ladder (climbable)</summary>
        Ladder = 6
    }
}
