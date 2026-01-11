using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;

namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// Information about a single tileset used in a tilemap.
    /// Multiple tilesets can be used in a single map.
    /// </summary>
    public class TilesetInfo
    {
        /// <summary>
        /// First global ID - tiles with IDs >= FirstGid use this tileset
        /// </summary>
        public int FirstGid { get; set; }

        /// <summary>
        /// Name of the tileset
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Width of each tile in pixels
        /// </summary>
        public int TileWidth { get; set; }

        /// <summary>
        /// Height of each tile in pixels
        /// </summary>
        public int TileHeight { get; set; }

        /// <summary>
        /// Spacing between tiles in pixels
        /// </summary>
        public int Spacing { get; set; }

        /// <summary>
        /// Margin around the tileset in pixels
        /// </summary>
        public int Margin { get; set; }

        /// <summary>
        /// The texture for this tileset
        /// </summary>
        public ITexture? Texture { get; set; }

        /// <summary>
        /// Optional sprite sheet for this tileset
        /// </summary>
        public SpriteSheet? SpriteSheet { get; set; }

        public TilesetInfo(int firstGid, string name, int tileWidth, int tileHeight, int spacing = 0, int margin = 0)
        {
            FirstGid = firstGid;
            Name = name;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Spacing = spacing;
            Margin = margin;
        }
    }
}
