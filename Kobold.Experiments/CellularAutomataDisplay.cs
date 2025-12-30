using Arch.Core;
using Kobold.Core;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Components;
using Kobold.Core.Configuration;
using Kobold.Core.Systems;
using Kobold.Extensions.Tilemaps;
using Kobold.Extensions.Procedural;
using System.Drawing;
using System.Numerics;

namespace Experiments
{
    /// <summary>
    /// Game that displays a cellular automata generated tilemap.
    /// </summary>
    public class CellularAutomataDisplay : GameEngineBase
    {
        private readonly CellularAutomataConfig _config;
        private TileMap? _tileMap;
        private TileSet? _tileSet;

        private const int TILE_PIXEL_SIZE = 6; // Size of each tile in pixels for rendering

        public CellularAutomataDisplay(CellularAutomataConfig config) : base()
        {
            _config = config;
        }

        public CellularAutomataDisplay(CellularAutomataConfig config, IRenderer renderer, IInputManager inputManager)
            : base(renderer, inputManager)
        {
            _config = config;
        }

        public override void Initialize()
        {
            base.Initialize();

            // Generate the cellular automata
            GenerateMap();

            // Create visual representation
            CreateVisuals();

            // Initialize systems
            InitializeSystems();
        }

        private void GenerateMap()
        {
            var generator = new CellularAutomataGenerator(_config);
            (_tileMap, _tileSet) = generator.GenerateWithTileSet(wallIsSolid: true);
        }

        private void CreateVisuals()
        {
            if (_tileMap == null || _tileSet == null)
                return;

            // Create a visual entity for each tile in the map
            for (int x = 0; x < _tileMap.Width; x++)
            {
                for (int y = 0; y < _tileMap.Height; y++)
                {
                    int tileId = _tileMap.GetTile(0, x, y);

                    if (tileId < 0)
                        continue; // Skip empty tiles

                    // Determine color based on tile type
                    Color color;
                    if (tileId == _config.WallTileId)
                    {
                        // Walls are dark gray
                        color = Color.FromArgb(255, 60, 60, 60);
                    }
                    else
                    {
                        // Floor is light gray
                        color = Color.FromArgb(255, 200, 200, 200);
                    }

                    // Create a rectangle for this tile
                    var position = new Vector2(x * TILE_PIXEL_SIZE, y * TILE_PIXEL_SIZE);
                    var size = new Vector2(TILE_PIXEL_SIZE, TILE_PIXEL_SIZE);

                    World.Create(
                        new Transform(position),
                        RectangleRenderer.Background(size, color)
                    );
                }
            }

            // Add title text showing configuration
            World.Create(
                new Transform(new Vector2(10, 10)),
                TextRenderer.UIText($"Cellular Automata - {_config.Width}x{_config.Height}, {_config.Iterations} iterations",
                    Color.White, 16f)
            );

            // Add legend
            World.Create(
                new Transform(new Vector2(10, 35)),
                TextRenderer.UIText($"Seed: {_config.Seed?.ToString() ?? "Random"}",
                    Color.White, 12f)
            );

            World.Create(
                new Transform(new Vector2(10, 55)),
                TextRenderer.UIText($"Wall Prob: {_config.InitialWallProbability:F2}, Birth: {_config.BirthThreshold}, Death: {_config.DeathThreshold}",
                    Color.White, 12f)
            );
        }

        private void InitializeSystems()
        {
            var renderSystem = new RenderSystem(Renderer, World);
            SystemManager.AddRenderSystem(renderSystem);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Render()
        {
            base.Render();
        }

        public static GameConfig CreateConfig(int width, int height)
        {
            return new GameConfig
            {
                ScreenWidth = width * TILE_PIXEL_SIZE + 20, // Add padding
                ScreenHeight = height * TILE_PIXEL_SIZE + 80, // Add padding for UI
                BackgroundColor = Color.Black
            };
        }
    }
}
