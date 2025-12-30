using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;
using Kobold.Extensions.Tilemaps;

namespace CaveExplorer.Components
{
    /// <summary>
    /// Component that holds tilemap data for rendering and collision detection.
    /// </summary>
    public struct TileMapComponent
    {
        public TileMap TileMap;
        public TileSet TileSet;
        public ITexture Texture;        // Sprite sheet texture for rendering tiles
        public SpriteSheet SpriteSheet; // For named region lookup

        public TileMapComponent(TileMap tileMap, TileSet tileSet, ITexture texture, SpriteSheet spriteSheet)
        {
            TileMap = tileMap;
            TileSet = tileSet;
            Texture = texture;
            SpriteSheet = spriteSheet;
        }
    }
}
