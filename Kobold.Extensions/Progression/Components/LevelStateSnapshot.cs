using Kobold.Extensions.Tilemaps;
using System.Numerics;

namespace Kobold.Extensions.Progression
{
    /// <summary>
    /// Captures the complete state of a level for later restoration.
    /// Stores tilemap data, camera bounds, and stair positions to recreate the exact same level
    /// when the player returns.
    /// </summary>
    public class LevelStateSnapshot
    {
        /// <summary>
        /// Cloned tilemap containing all tile data for this level
        /// </summary>
        public TileMap TileMap { get; set; }

        /// <summary>
        /// TileSet reference for tile properties (immutable, can share reference)
        /// </summary>
        public TileSet TileSet { get; set; }

        /// <summary>
        /// Camera viewport width
        /// </summary>
        public float CameraWidth { get; set; }

        /// <summary>
        /// Camera viewport height
        /// </summary>
        public float CameraHeight { get; set; }

        /// <summary>
        /// Position where the stairUp is located (for player spawn when descending back)
        /// </summary>
        public Vector2 StairUpPosition { get; set; }

        /// <summary>
        /// Position where the stairDown is located (for player spawn when ascending back)
        /// </summary>
        public Vector2 StairDownPosition { get; set; }

        /// <summary>
        /// Depth of this level (for validation)
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Identifier for this level (e.g., "depth_0", "depth_5")
        /// </summary>
        public string LevelId { get; set; }

        public LevelStateSnapshot()
        {
            LevelId = string.Empty;
        }
    }
}
