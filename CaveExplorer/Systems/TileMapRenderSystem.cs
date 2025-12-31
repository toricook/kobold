using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Components;
using Kobold.Extensions.Tilemaps;
using System;
using System.Drawing;
using System.Numerics;

namespace CaveExplorer.Systems
{
    /// <summary>
    /// Renders tilemap entities by drawing each tile using the sprite sheet.
    /// </summary>
    public class TileMapRenderSystem : IRenderSystem
    {
        private readonly IRenderer _renderer;
        private readonly World _world;

        public TileMapRenderSystem(IRenderer renderer, World world)
        {
            _renderer = renderer;
            _world = world;
        }

        public void Render()
        {
            _renderer.Begin();

            // Get camera (if exists)
            Camera? camera = GetCamera();

            var query = new QueryDescription().WithAll<TileMapComponent>();

            int entityCount = 0;
            _world.Query(in query, (Entity entity, ref TileMapComponent tileMapComponent) =>
            {
                entityCount++;
                RenderTileMap(tileMapComponent, camera);
            });

            // Debug: Print once to verify system is running
            if (entityCount == 0)
            {
                Console.WriteLine("Warning: No tilemap entities found!");
            }

            _renderer.End();
        }

        private Camera? GetCamera()
        {
            var cameraQuery = new QueryDescription().WithAll<Camera>();
            Camera? result = null;

            _world.Query(in cameraQuery, (ref Camera camera) =>
            {
                result = camera;
            });

            return result;
        }

        private void RenderTileMap(TileMapComponent tileMapComponent, Camera? camera)
        {
            var tileMap = tileMapComponent.TileMap;

            // Calculate visible tile range for culling
            int startX = 0;
            int startY = 0;
            int endX = tileMap.Width;
            int endY = tileMap.Height;

            if (camera.HasValue)
            {
                var cam = camera.Value;
                var (left, top, right, bottom) = cam.GetViewportBounds();

                // Convert viewport bounds to tile coordinates (with padding for safety)
                startX = Math.Max(0, (int)(left / tileMap.TileWidth) - 1);
                startY = Math.Max(0, (int)(top / tileMap.TileHeight) - 1);
                endX = Math.Min(tileMap.Width, (int)(right / tileMap.TileWidth) + 2);
                endY = Math.Min(tileMap.Height, (int)(bottom / tileMap.TileHeight) + 2);
            }

            // Iterate through visible tiles only
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    // Get tile ID from layer 0
                    int tileId = tileMap.GetTile(0, x, y);

                    // Skip empty tiles
                    if (tileId < 0)
                        continue;

                    // Map tile ID to color with FULL ALPHA (255)
                    Color color;
                    if (tileId == 1) // Wall
                    {
                        color = Color.FromArgb(255, 128, 128, 128); // Gray with full alpha
                    }
                    else if (tileId == 0) // Floor
                    {
                        color = Color.FromArgb(255, 50, 50, 50); // Dark gray with full alpha
                    }
                    else
                    {
                        // Default to bright color for debugging
                        color = Color.FromArgb(255, 255, 0, 255); // Magenta for unknown tiles
                    }

                    // Convert tile coordinates to world position (top-left of tile)
                    var (worldX, worldY) = tileMap.TileToWorld(x, y);
                    var worldPosition = new Vector2(worldX, worldY);

                    // Convert to screen position if camera exists
                    Vector2 screenPosition = camera.HasValue
                        ? camera.Value.WorldToScreen(worldPosition)
                        : worldPosition;

                    // Round to integer pixel coordinates to prevent sub-pixel rendering artifacts
                    screenPosition = new Vector2(MathF.Round(screenPosition.X), MathF.Round(screenPosition.Y));

                    var size = new Vector2(tileMap.TileWidth, tileMap.TileHeight);

                    // Render the tile as a colored rectangle
                    _renderer.DrawRectangle(screenPosition, size, color);
                }
            }
        }
    }
}
