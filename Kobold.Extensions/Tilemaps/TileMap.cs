using System;
using System.Collections.Generic;

namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// Represents a grid-based tilemap with support for multiple layers.
    /// </summary>
    public class TileMap
    {
        private readonly int[,][] _tiles; // [x, y][layer]

        public int Width { get; }
        public int Height { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public int LayerCount { get; }

        /// <summary>
        /// Creates a new tilemap with the specified dimensions.
        /// </summary>
        /// <param name="width">Width in tiles</param>
        /// <param name="height">Height in tiles</param>
        /// <param name="tileWidth">Width of each tile in pixels</param>
        /// <param name="tileHeight">Height of each tile in pixels</param>
        /// <param name="layerCount">Number of layers (default: 1)</param>
        public TileMap(int width, int height, int tileWidth, int tileHeight, int layerCount = 1)
        {
            if (width <= 0) throw new ArgumentException("Width must be greater than 0", nameof(width));
            if (height <= 0) throw new ArgumentException("Height must be greater than 0", nameof(height));
            if (tileWidth <= 0) throw new ArgumentException("TileWidth must be greater than 0", nameof(tileWidth));
            if (tileHeight <= 0) throw new ArgumentException("TileHeight must be greater than 0", nameof(tileHeight));
            if (layerCount <= 0) throw new ArgumentException("LayerCount must be greater than 0", nameof(layerCount));

            Width = width;
            Height = height;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            LayerCount = layerCount;

            _tiles = new int[width, height][];

            // Initialize all tiles to empty (-1)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _tiles[x, y] = new int[layerCount];
                    for (int layer = 0; layer < layerCount; layer++)
                    {
                        _tiles[x, y][layer] = -1; // -1 = empty tile
                    }
                }
            }
        }

        /// <summary>
        /// Gets the tile ID at the specified position and layer.
        /// </summary>
        /// <param name="layer">Layer index (0-based)</param>
        /// <param name="x">X coordinate in tile units</param>
        /// <param name="y">Y coordinate in tile units</param>
        /// <returns>Tile ID, or -1 if empty or out of bounds</returns>
        public int GetTile(int layer, int x, int y)
        {
            if (!IsValidPosition(x, y) || !IsValidLayer(layer))
                return -1;

            return _tiles[x, y][layer];
        }

        /// <summary>
        /// Sets the tile ID at the specified position and layer.
        /// </summary>
        /// <param name="layer">Layer index (0-based)</param>
        /// <param name="x">X coordinate in tile units</param>
        /// <param name="y">Y coordinate in tile units</param>
        /// <param name="tileId">Tile ID to set (-1 for empty)</param>
        public void SetTile(int layer, int x, int y, int tileId)
        {
            if (!IsValidPosition(x, y))
                throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds");

            if (!IsValidLayer(layer))
                throw new ArgumentOutOfRangeException(nameof(layer), $"Layer {layer} is out of bounds");

            _tiles[x, y][layer] = tileId;
        }

        /// <summary>
        /// Fills a rectangular region with the specified tile ID.
        /// </summary>
        /// <param name="layer">Layer index (0-based)</param>
        /// <param name="x">Starting X coordinate</param>
        /// <param name="y">Starting Y coordinate</param>
        /// <param name="width">Width in tiles</param>
        /// <param name="height">Height in tiles</param>
        /// <param name="tileId">Tile ID to fill with</param>
        public void Fill(int layer, int x, int y, int width, int height, int tileId)
        {
            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    int px = x + dx;
                    int py = y + dy;

                    if (IsValidPosition(px, py))
                    {
                        SetTile(layer, px, py, tileId);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all tiles on the specified layer (sets them to -1).
        /// </summary>
        /// <param name="layer">Layer index to clear</param>
        public void ClearLayer(int layer)
        {
            if (!IsValidLayer(layer))
                throw new ArgumentOutOfRangeException(nameof(layer));

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _tiles[x, y][layer] = -1;
                }
            }
        }

        /// <summary>
        /// Clears all tiles on all layers.
        /// </summary>
        public void Clear()
        {
            for (int layer = 0; layer < LayerCount; layer++)
            {
                ClearLayer(layer);
            }
        }

        /// <summary>
        /// Gets all tiles within the specified rectangular bounds.
        /// </summary>
        /// <param name="layer">Layer index</param>
        /// <param name="x">Starting X coordinate</param>
        /// <param name="y">Starting Y coordinate</param>
        /// <param name="width">Width in tiles</param>
        /// <param name="height">Height in tiles</param>
        /// <returns>List of tile positions and IDs</returns>
        public List<(int x, int y, int tileId)> GetTilesInBounds(int layer, int x, int y, int width, int height)
        {
            var tiles = new List<(int x, int y, int tileId)>();

            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    int px = x + dx;
                    int py = y + dy;

                    if (IsValidPosition(px, py))
                    {
                        int tileId = GetTile(layer, px, py);
                        if (tileId >= 0) // Only include non-empty tiles
                        {
                            tiles.Add((px, py, tileId));
                        }
                    }
                }
            }

            return tiles;
        }

        /// <summary>
        /// Converts world position (in pixels) to tile coordinates.
        /// </summary>
        /// <param name="worldX">X position in pixels</param>
        /// <param name="worldY">Y position in pixels</param>
        /// <returns>Tile coordinates</returns>
        public (int tileX, int tileY) WorldToTile(float worldX, float worldY)
        {
            int tileX = (int)Math.Floor(worldX / TileWidth);
            int tileY = (int)Math.Floor(worldY / TileHeight);
            return (tileX, tileY);
        }

        /// <summary>
        /// Converts tile coordinates to world position (in pixels, top-left corner of tile).
        /// </summary>
        /// <param name="tileX">X coordinate in tiles</param>
        /// <param name="tileY">Y coordinate in tiles</param>
        /// <returns>World position in pixels</returns>
        public (float worldX, float worldY) TileToWorld(int tileX, int tileY)
        {
            float worldX = tileX * TileWidth;
            float worldY = tileY * TileHeight;
            return (worldX, worldY);
        }

        /// <summary>
        /// Gets the center position of a tile in world coordinates.
        /// </summary>
        public (float worldX, float worldY) TileToWorldCenter(int tileX, int tileY)
        {
            var (worldX, worldY) = TileToWorld(tileX, tileY);
            return (worldX + TileWidth / 2f, worldY + TileHeight / 2f);
        }

        /// <summary>
        /// Checks if the specified tile coordinates are within bounds.
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// Checks if the specified layer index is valid.
        /// </summary>
        public bool IsValidLayer(int layer)
        {
            return layer >= 0 && layer < LayerCount;
        }

        /// <summary>
        /// Gets the total size of the tilemap in pixels.
        /// </summary>
        public (int pixelWidth, int pixelHeight) GetPixelSize()
        {
            return (Width * TileWidth, Height * TileHeight);
        }

        /// <summary>
        /// Creates a copy of this tilemap.
        /// </summary>
        public TileMap Clone()
        {
            var clone = new TileMap(Width, Height, TileWidth, TileHeight, LayerCount);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int layer = 0; layer < LayerCount; layer++)
                    {
                        clone._tiles[x, y][layer] = _tiles[x, y][layer];
                    }
                }
            }

            return clone;
        }
    }
}
