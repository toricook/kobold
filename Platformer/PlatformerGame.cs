using Kobold.Extensions.GameBases;
using Kobold.Extensions.Tilemaps;
using Kobold.Core.Components;
using System.Drawing;
using System.Numerics;
using Kobold.Core.Abstractions.Engine;

namespace Platformer
{
    public class PlatformerGame : PlatformerGameBase
    {
        public PlatformerGame() : base()
        {
        }

        protected override void RegisterAdditionalSystems()
        {
            // Add temporary debug system to track player position
            SystemManager.AddSystem(new PlayerPositionDebugSystem(World), 1000);
        }

        protected override void CreateGameEntities()
        {
            // Create a simple tilemap-based level
            var tileMap = CreateSimpleTileMap(40, 23, 32, 32); // 1280x736 pixels
            var tileSet = CreateSimpleTileSet();

            // Build the level (0 = air, 1 = solid)
            // Ground at bottom (complete floor)
            for (int x = 0; x < 40; x++)
            {
                tileMap.SetTile(0, x, 22, 1); // Ground
                tileMap.SetTile(0, x, 21, 1); // Ground (2 tiles thick)
            }

            // Platform 1 (left side)
            for (int x = 8; x < 14; x++)
                tileMap.SetTile(0, x, 16, 1);

            // Platform 2 (middle)
            for (int x = 18; x < 24; x++)
                tileMap.SetTile(0, x, 13, 1);

            // Platform 3 (right side)
            for (int x = 28; x < 34; x++)
                tileMap.SetTile(0, x, 10, 1);

            // Create tilemap entity (no texture needed - only used for collision)
            var tilemapEntity = World.Create(
                new Transform(Vector2.Zero),
                new TileMapComponent(tileMap, tileSet, null, null)
            );

            // Create visual rectangles for each solid tile
            CreateTileVisuals(tileMap, tileSet);

            // Player starts on Platform 1 (safe starting position)
            // Platform 1 is at tiles (8-14, 16) = world coords (256-448 pixels X, 512 pixels Y top)
            // Place player at X=320 (center of platform), Y=480 (above platform top at 512)
            var playerStartPosition = new Vector2(320, 480);

            System.Console.WriteLine($"===== GAME STARTING =====");
            System.Console.WriteLine($"Creating player at position: ({playerStartPosition.X}, {playerStartPosition.Y})");
            System.Console.WriteLine($"Platform 1: X=256-448, Y=512 (player should land here)");
            System.Console.WriteLine($"Ground is at Y={22 * 32} pixels (tiles 21-22)");
            System.Console.WriteLine($"Death zone is at Y={ScreenHeight + 100} pixels");
            System.Console.WriteLine($"=======================");

            // Create camera
            CreateCamera(playerStartPosition);
            SetCameraBounds(1280, 736);

            // Create player
            CreateSimplePlayer(playerStartPosition);

            System.Console.WriteLine($"Player created successfully");

            System.Console.WriteLine("Platformer initialized with tilemap!");
            System.Console.WriteLine($"Level size: {tileMap.Width}x{tileMap.Height} tiles ({tileMap.Width * tileMap.TileWidth}x{tileMap.Height * tileMap.TileHeight} pixels)");
        }

        // Helper to create visual rectangles for tiles
        private void CreateTileVisuals(TileMap tileMap, TileSet tileSet)
        {
            for (int y = 0; y < tileMap.Height; y++)
            {
                for (int x = 0; x < tileMap.Width; x++)
                {
                    int tileId = tileMap.GetTile(0, x, y);
                    if (tileId > 0) // Only render non-empty tiles
                    {
                        var (tileX, tileY) = tileMap.TileToWorld(x, y);
                        var color = tileId == 1 ? Color.Green : Color.Brown;

                        World.Create(
                            new Transform(new Vector2(tileX, tileY)),
                            new RectangleRenderer(new Vector2(tileMap.TileWidth, tileMap.TileHeight), color)
                        );
                    }
                }
            }
        }

        // Helper to create a simple player with a colored rectangle
        private void CreateSimplePlayer(Vector2 position)
        {
            PlayerEntity = World.Create(
                new Transform(position),
                new Velocity(Vector2.Zero),
                new RectangleRenderer(new Vector2(28, 28), Color.Blue),
                new BoxCollider(28f, 28f, new Vector2(-14f, -14f)),
                PlayerControlled.CreateHorizontalOnly(200f),
                new Physics(damping: 0.0f),
                new Player()
            );
        }
    }

    // Temporary debug system to track player position
    public class PlayerPositionDebugSystem : ISystem
    {
        private readonly Arch.Core.World _world;
        private int _frameCount = 0;

        public PlayerPositionDebugSystem(Arch.Core.World world)
        {
            _world = world;
        }

        public void Update(float deltaTime)
        {
            _frameCount++;

            // Log every 60 frames (roughly once per second)
            if (_frameCount % 60 == 0)
            {
                var query = new Arch.Core.QueryDescription().WithAll<Player, Transform, Velocity>();
                _world.Query(in query, (ref Transform transform, ref Velocity velocity) =>
                {
                    System.Console.WriteLine($"[Frame {_frameCount}] Player at ({transform.Position.X:F1}, {transform.Position.Y:F1}), Velocity: ({velocity.Value.X:F1}, {velocity.Value.Y:F1})");
                });
            }
        }
    }
}
