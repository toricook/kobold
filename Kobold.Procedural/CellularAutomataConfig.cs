namespace Procedural
{
    /// <summary>
    /// Configuration options for cellular automata generation.
    /// </summary>
    public class CellularAutomataConfig
    {
        /// <summary>
        /// Width of the map in tiles.
        /// </summary>
        public int Width { get; set; } = 100;

        /// <summary>
        /// Height of the map in tiles.
        /// </summary>
        public int Height { get; set; } = 100;

        /// <summary>
        /// Number of smoothing iterations to perform.
        /// Higher values create smoother, more organic shapes.
        /// </summary>
        public int Iterations { get; set; } = 8;

        /// <summary>
        /// Initial chance (0.0 to 1.0) that a cell is a wall.
        /// Higher values create more walls/obstacles.
        /// </summary>
        public float InitialWallProbability { get; set; } = 0.40f;

        /// <summary>
        /// Minimum number of neighboring walls for a cell to become/remain a wall.
        /// Used during smoothing iterations.
        /// </summary>
        public int BirthThreshold { get; set; } = 5;

        /// <summary>
        /// Maximum number of neighboring walls for a cell to remain alive.
        /// If neighbors exceed this, the cell dies (becomes floor).
        /// </summary>
        public int DeathThreshold { get; set; } = 2;

        /// <summary>
        /// Random seed for deterministic generation.
        /// If null, a random seed is used.
        /// </summary>
        public int? Seed { get; set; } = null;

        /// <summary>
        /// Tile ID to use for wall tiles.
        /// </summary>
        public int WallTileId { get; set; } = 1;

        /// <summary>
        /// Tile ID to use for floor/empty tiles.
        /// </summary>
        public int FloorTileId { get; set; } = 0;

        /// <summary>
        /// Width of each tile in pixels (for TileMap creation).
        /// </summary>
        public int TileWidth { get; set; } = 16;

        /// <summary>
        /// Height of each tile in pixels (for TileMap creation).
        /// </summary>
        public int TileHeight { get; set; } = 16;

        /// <summary>
        /// Whether to treat edges of the map as walls.
        /// When true, out-of-bounds cells count as walls for neighbor calculations.
        /// </summary>
        public bool EdgeIsWall { get; set; } = true;

        /// <summary>
        /// Whether to connect all cave regions with corridors.
        /// When true, uses flood fill + MST to ensure all caves are reachable.
        /// </summary>
        public bool ConnectCaves { get; set; } = true;

        /// <summary>
        /// Minimum size (in tiles) for a cave region to be kept.
        /// Smaller regions are filled in as walls.
        /// </summary>
        public int MinCaveSize { get; set; } = 50;

        /// <summary>
        /// Creates a default configuration for cave-like generation.
        /// </summary>
        public static CellularAutomataConfig Cave()
        {
            return new CellularAutomataConfig
            {
                Width = 64,
                Height = 64,
                Iterations = 5,
                InitialWallProbability = 0.45f,
                BirthThreshold = 4,
                DeathThreshold = 3,
                EdgeIsWall = true
            };
        }

        /// <summary>
        /// Creates a configuration for more open areas with scattered obstacles.
        /// </summary>
        public static CellularAutomataConfig OpenArea()
        {
            return new CellularAutomataConfig
            {
                Width = 64,
                Height = 64,
                Iterations = 3,
                InitialWallProbability = 0.3f,
                BirthThreshold = 5,
                DeathThreshold = 2,
                EdgeIsWall = false
            };
        }

        /// <summary>
        /// Creates a configuration for dense maze-like structures.
        /// </summary>
        public static CellularAutomataConfig Maze()
        {
            return new CellularAutomataConfig
            {
                Width = 64,
                Height = 64,
                Iterations = 2,
                InitialWallProbability = 0.5f,
                BirthThreshold = 4,
                DeathThreshold = 4,
                EdgeIsWall = true
            };
        }
    }
}
