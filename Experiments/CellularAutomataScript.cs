using Kobold.Monogame;
using Procedural;
using Rapid.NET;

namespace Experiments
{
    [Script]
    [Documentation("Generate and visualize cellular automata with configurable parameters")]
    public class CellularAutomataScript
    {
        [Documentation("Configuration parameters for cellular automata generation")]
        public class Config
        {
            [Documentation("Width of the map in tiles (1-200)")]
            public int Width = 100;

            [Documentation("Height of the map in tiles (1-200)")]
            public int Height = 100;

            [Documentation("Number of smoothing iterations (0-20)")]
            public int Iterations = 8;

            [Documentation("Initial chance that a cell is a wall (0.0-1.0)")]
            public float InitialWallProbability = 0.40f;

            [Documentation("Neighbors needed for a cell to become a wall (0-8)")]
            public int BirthThreshold = 5;

            [Documentation("Max neighbors for a wall to stay alive (0-8)")]
            public int DeathThreshold = 2;

            [Documentation("Random seed (0 for random)")]
            public int Seed = 0;

            [Documentation("Whether map edges count as walls")]
            public bool EdgeIsWall = true;

            [Documentation("Tile width in pixels (for rendering)")]
            public int TileWidth = 16;

            [Documentation("Tile height in pixels (for rendering)")]
            public int TileHeight = 16;

            [Documentation("Tile ID to use for walls")]
            public int WallTileId = 1;

            [Documentation("Tile ID to use for floor")]
            public int FloorTileId = 0;

            [Documentation("Connect all caves with corridors for full reachability")]
            public bool ConnectCaves = true;

            [Documentation("Minimum size for a cave region (smaller ones are removed)")]
            public int MinCaveSize = 50;
        }

        public static void Run(Config cfg)
        {
            // Validate and create configuration
            var config = new CellularAutomataConfig
            {
                Width = Math.Clamp(cfg.Width, 1, 200),
                Height = Math.Clamp(cfg.Height, 1, 200),
                Iterations = Math.Clamp(cfg.Iterations, 0, 20),
                InitialWallProbability = Math.Clamp(cfg.InitialWallProbability, 0f, 1f),
                BirthThreshold = Math.Clamp(cfg.BirthThreshold, 0, 8),
                DeathThreshold = Math.Clamp(cfg.DeathThreshold, 0, 8),
                Seed = cfg.Seed == 0 ? null : cfg.Seed,
                EdgeIsWall = cfg.EdgeIsWall,
                TileWidth = Math.Max(1, cfg.TileWidth),
                TileHeight = Math.Max(1, cfg.TileHeight),
                WallTileId = cfg.WallTileId,
                FloorTileId = cfg.FloorTileId,
                ConnectCaves = cfg.ConnectCaves,
                MinCaveSize = Math.Max(1, cfg.MinCaveSize)
            };

            // Display configuration summary
            Console.WriteLine("\nGenerating cellular automata with configuration:");
            Console.WriteLine($"  Size: {config.Width}x{config.Height} tiles");
            Console.WriteLine($"  Iterations: {config.Iterations}");
            Console.WriteLine($"  Initial Wall Probability: {config.InitialWallProbability:F2}");
            Console.WriteLine($"  Birth Threshold: {config.BirthThreshold}");
            Console.WriteLine($"  Death Threshold: {config.DeathThreshold}");
            Console.WriteLine($"  Seed: {config.Seed?.ToString() ?? "Random"}");
            Console.WriteLine($"  Edge Is Wall: {config.EdgeIsWall}");
            Console.WriteLine($"  Connect Caves: {config.ConnectCaves}");
            Console.WriteLine($"  Min Cave Size: {config.MinCaveSize} tiles");
            Console.WriteLine($"  Tile Size: {config.TileWidth}x{config.TileHeight} pixels");
            Console.WriteLine("\nLaunching visualization...\n");

            // Launch MonoGame visualization
            var gameConfig = CellularAutomataDisplay.CreateConfig(config.Width, config.Height);
            var game = new CellularAutomataDisplay(config);
            using var host = new MonoGameHost(game, gameConfig);
            host.Run();
        }
    }
}
