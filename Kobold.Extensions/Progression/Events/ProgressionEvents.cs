using Kobold.Core.Events;
using Kobold.Extensions.Tilemaps;

namespace Kobold.Extensions.Progression
{
    /// <summary>
    /// Published by LevelProgressionSystem when a new level needs to be generated.
    /// Game code subscribes to this event and generates the level using any method
    /// (procedural, hand-crafted, loaded from disk, etc.).
    /// </summary>
    public class GenerateNewLevelEvent : BaseEvent
    {
        /// <summary>
        /// Depth of the level to generate (0-based)
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// True if this level has been visited before (rare - usually we restore from snapshot).
        /// Most games can ignore this flag.
        /// </summary>
        public bool IsRevisit { get; }

        public GenerateNewLevelEvent(int depth, bool isRevisit = false)
        {
            Depth = depth;
            IsRevisit = isRevisit;
        }
    }

    /// <summary>
    /// Published by game code when level generation is complete.
    /// LevelProgressionSystem subscribes to this event to create stairs and spawn the player.
    /// </summary>
    public class LevelReadyEvent : BaseEvent
    {
        /// <summary>
        /// Depth of the generated level (for validation)
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// The generated tilemap (needed for finding valid stair positions).
        /// Can be null for games without tilemaps (like Asteroids).
        /// </summary>
        public TileMap? TileMap { get; }

        public LevelReadyEvent(int depth, TileMap? tileMap)
        {
            Depth = depth;
            TileMap = tileMap;
        }
    }
}
