using Kobold.Core.Abstractions.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Kobold.Core.Assets
{
    /// <summary>
    /// Represents a tileset - a collection of tiles stored in a texture atlas.
    /// Tilesets are used for building tilemaps and tile-based levels.
    /// </summary>
    public class Tileset
    {
        /// <summary>
        /// The texture containing all tiles
        /// </summary>
        public ITexture Texture { get; }

        /// <summary>
        /// Configuration defining how the tileset is organized
        /// </summary>
        public TilesetConfig Config { get; }

        /// <summary>
        /// Width of each tile in pixels
        /// </summary>
        public int TileWidth => Config.TileWidth;

        /// <summary>
        /// Height of each tile in pixels
        /// </summary>
        public int TileHeight => Config.TileHeight;

        /// <summary>
        /// Number of tile columns in the texture
        /// </summary>
        public int Columns => Config.Columns;

        /// <summary>
        /// Number of tile rows in the texture
        /// </summary>
        public int Rows => Config.Rows;

        /// <summary>
        /// Total number of tiles in the tileset
        /// </summary>
        public int TileCount => Config.Columns * Config.Rows;

        /// <summary>
        /// Create a new tileset with a texture and configuration
        /// </summary>
        /// <param name="texture">The texture containing the tiles</param>
        /// <param name="config">Configuration for tile layout</param>
        public Tileset(ITexture texture, TilesetConfig config)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Get the source rectangle for a tile by its ID (0-based, left-to-right, top-to-bottom)
        /// </summary>
        /// <param name="tileId">The tile ID (0 = first tile, 1 = second tile, etc.)</param>
        /// <returns>Rectangle defining the tile's region in the texture</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if tileId is invalid</exception>
        public Rectangle GetTileRect(int tileId)
        {
            if (tileId < 0 || tileId >= TileCount)
            {
                throw new ArgumentOutOfRangeException(nameof(tileId),
                    $"Tile ID {tileId} is out of range. Tileset has {TileCount} tiles.");
            }

            int row = tileId / Columns;
            int col = tileId % Columns;

            return GetTileRectByRowCol(row, col);
        }

        /// <summary>
        /// Get the source rectangle for a tile by its row and column
        /// </summary>
        /// <param name="row">Row index (0-based)</param>
        /// <param name="col">Column index (0-based)</param>
        /// <returns>Rectangle defining the tile's region in the texture</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if row or column is invalid</exception>
        public Rectangle GetTileRectByRowCol(int row, int col)
        {
            if (row < 0 || row >= Rows)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (col < 0 || col >= Columns)
                throw new ArgumentOutOfRangeException(nameof(col));

            int x = Config.Margin + col * (TileWidth + Config.Spacing);
            int y = Config.Margin + row * (TileHeight + Config.Spacing);

            return new Rectangle(x, y, TileWidth, TileHeight);
        }

        /// <summary>
        /// Get tile metadata by tile ID
        /// </summary>
        /// <param name="tileId">The tile ID</param>
        /// <returns>Tile metadata if it exists, null otherwise</returns>
        public TileMetadata? GetTileMetadata(int tileId)
        {
            return Config.TileMetadata.TryGetValue(tileId, out var metadata) ? metadata : null;
        }

        /// <summary>
        /// Check if a tile ID has custom metadata defined
        /// </summary>
        /// <param name="tileId">The tile ID to check</param>
        /// <returns>True if metadata exists for this tile</returns>
        public bool HasMetadata(int tileId)
        {
            return Config.TileMetadata.ContainsKey(tileId);
        }
    }

    /// <summary>
    /// Configuration for how a tileset texture is organized
    /// </summary>
    public class TilesetConfig
    {
        /// <summary>
        /// Width of each tile in pixels
        /// </summary>
        public int TileWidth { get; set; }

        /// <summary>
        /// Height of each tile in pixels
        /// </summary>
        public int TileHeight { get; set; }

        /// <summary>
        /// Number of tile columns in the texture
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// Number of tile rows in the texture
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Spacing between tiles in pixels (default 0)
        /// </summary>
        public int Spacing { get; set; } = 0;

        /// <summary>
        /// Margin around the entire tileset in pixels (default 0)
        /// </summary>
        public int Margin { get; set; } = 0;

        /// <summary>
        /// Optional metadata for specific tiles (indexed by tile ID)
        /// </summary>
        public Dictionary<int, TileMetadata> TileMetadata { get; set; } = new Dictionary<int, TileMetadata>();

        /// <summary>
        /// Optional custom properties for the tileset
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Metadata for a specific tile in a tileset
    /// </summary>
    public class TileMetadata
    {
        /// <summary>
        /// Name or description of the tile
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Tags for categorizing the tile (e.g., "solid", "water", "hazard")
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Custom properties for this tile
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Animation frames if this tile is animated (list of tile IDs)
        /// </summary>
        public int[]? AnimationFrames { get; set; }

        /// <summary>
        /// Animation speed in frames per second (only used if AnimationFrames is set)
        /// </summary>
        public float AnimationFps { get; set; } = 10f;
    }
}
