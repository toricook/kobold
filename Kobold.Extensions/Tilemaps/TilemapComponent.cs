namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// ECS component that attaches a tilemap to an entity.
    /// Used with TilemapSystem for rendering and collision.
    /// </summary>
    public struct TilemapComponent
    {
        /// <summary>
        /// The tilemap data (tile layout).
        /// </summary>
        public TileMap TileMap { get; set; }

        /// <summary>
        /// The tileset (visual and property data for tiles).
        /// </summary>
        public TileSet TileSet { get; set; }

        /// <summary>
        /// Whether to generate collision for solid tiles.
        /// </summary>
        public bool EnableCollision { get; set; }

        /// <summary>
        /// Render layer priority (higher = rendered on top).
        /// </summary>
        public int RenderLayer { get; set; }

        /// <summary>
        /// Opacity for rendering (0.0 = transparent, 1.0 = opaque).
        /// </summary>
        public float Opacity { get; set; }

        /// <summary>
        /// Whether this tilemap is currently visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Creates a new tilemap component.
        /// </summary>
        /// <param name="tileMap">The tilemap</param>
        /// <param name="tileSet">The tileset</param>
        public TilemapComponent(TileMap tileMap, TileSet tileSet)
        {
            TileMap = tileMap;
            TileSet = tileSet;
            EnableCollision = true;
            RenderLayer = 0;
            Opacity = 1.0f;
            Visible = true;
        }
    }
}
