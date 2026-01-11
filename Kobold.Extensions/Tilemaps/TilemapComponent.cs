using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;
using Kobold.Extensions.Tilemaps.Loaders;
using System.Collections.Generic;

namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// Component that holds tilemap data for rendering and collision detection.
    /// </summary>
    public struct TileMapComponent
    {
        public TileMap TileMap;
        public TileSet TileSet;        // Primary tileset (for backward compatibility)
        public ITexture Texture;        // Primary texture (for backward compatibility)
        public SpriteSheet SpriteSheet; // Primary sprite sheet (for backward compatibility)
        public List<TilesetInfo>? Tilesets; // All tilesets for multi-tileset maps
        public List<TileLayerMetadata>? TileLayers; // Metadata for tile layers (including y_sort)

        public TileMapComponent(TileMap tileMap, TileSet tileSet, ITexture texture, SpriteSheet spriteSheet)
        {
            TileMap = tileMap;
            TileSet = tileSet;
            Texture = texture;
            SpriteSheet = spriteSheet;
            Tilesets = null;
            TileLayers = null;
        }

        public TileMapComponent(TileMap tileMap, List<TilesetInfo> tilesets, List<TileLayerMetadata>? tileLayers = null)
        {
            TileMap = tileMap;
            Tilesets = tilesets;
            TileLayers = tileLayers;

            // Set primary tileset info from first tileset for backward compatibility
            if (tilesets.Count > 0)
            {
                var first = tilesets[0];
                TileSet = new TileSet(first.TileWidth, first.TileHeight, first.Spacing, first.Margin);
                Texture = first.Texture!;
                SpriteSheet = first.SpriteSheet!;
            }
            else
            {
                TileSet = new TileSet(16, 16);
                Texture = null!;
                SpriteSheet = null!;
            }
        }
    }
}
