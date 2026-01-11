using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// Render system specifically for tilemaps.
    /// Handles efficient rendering of visible tiles based on camera viewport.
    /// </summary>
    public class TilemapRenderSystem : IRenderSystem
    {
        private readonly IRenderer _renderer;
        private readonly World _world;

        public TilemapRenderSystem(IRenderer renderer, World world)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public void Render()
        {
            // Get camera (if exists)
            Camera? camera = GetCamera();

            // Collect and render all tilemaps (render at layer 0 by default)
            var tilemapQuery = new QueryDescription().WithAll<TileMapComponent>();
            _world.Query(in tilemapQuery, (Entity entity, ref TileMapComponent tileMapComponent) =>
            {
                RenderTilemap(ref tileMapComponent, camera);
            });
        }

        private void RenderTilemap(ref TileMapComponent tilemapComponent, Camera? camera)
        {
            var tileMap = tilemapComponent.TileMap;
            var tileSet = tilemapComponent.TileSet;
            var texture = tilemapComponent.Texture;
            var spriteSheet = tilemapComponent.SpriteSheet;

            if (tileMap == null || tileSet == null || texture == null)
                return;

            // Calculate visible tiles based on camera (or render all if no camera)
            int startX = 0, startY = 0;
            int endX = tileMap.Width, endY = tileMap.Height;

            if (camera.HasValue)
            {
                // Get viewport bounds in world space
                var bounds = camera.Value.GetViewportBounds();

                // Convert world bounds to tile coordinates
                // Add 1 tile padding to ensure smooth scrolling without gaps
                startX = Math.Max(0, (int)(bounds.left / tileSet.TileWidth) - 1);
                startY = Math.Max(0, (int)(bounds.top / tileSet.TileHeight) - 1);
                endX = Math.Min(tileMap.Width, (int)(bounds.right / tileSet.TileWidth) + 2);
                endY = Math.Min(tileMap.Height, (int)(bounds.bottom / tileSet.TileHeight) + 2);
            }

            // Collect Y-sorted and non-Y-sorted layers
            var ySortedTiles = new List<(float yPos, int layer, int x, int y, int tileId)>();

            // Render all layers
            for (int layer = 0; layer < tileMap.LayerCount; layer++)
            {
                // Check if this layer has Y-sorting enabled
                bool isYSorted = false;
                if (tilemapComponent.TileLayers != null && layer < tilemapComponent.TileLayers.Count)
                {
                    isYSorted = tilemapComponent.TileLayers[layer].YSort;
                }

                if (isYSorted)
                {
                    // Collect tiles for Y-sorting
                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            int tileId = tileMap.GetTile(layer, x, y);
                            if (tileId < 0) continue; // Empty tile

                            // Calculate Y position for sorting (bottom of tile)
                            float yPos = (y + 1) * tileMap.TileHeight;
                            ySortedTiles.Add((yPos, layer, x, y, tileId));
                        }
                    }
                }
                else
                {
                    // Render non-Y-sorted layer immediately
                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            int tileId = tileMap.GetTile(layer, x, y);
                            if (tileId < 0) continue; // Empty tile

                            RenderTile(ref tilemapComponent, camera, x, y, tileId, tileMap, tileSet, texture, spriteSheet);
                        }
                    }
                }
            }

            // Sort and render Y-sorted tiles
            if (ySortedTiles.Count > 0)
            {
                ySortedTiles.Sort((a, b) => a.yPos.CompareTo(b.yPos));

                foreach (var (yPos, layer, x, y, tileId) in ySortedTiles)
                {
                    RenderTile(ref tilemapComponent, camera, x, y, tileId, tileMap, tileSet, texture, spriteSheet);
                }
            }
        }

        private void RenderTile(ref TileMapComponent tilemapComponent, Camera? camera, int x, int y, int tileId,
            TileMap tileMap, TileSet tileSet, ITexture texture, SpriteSheet spriteSheet)
        {
            // Find the correct tileset for this tile ID
            TilesetInfo? tilesetToUse = null;
            int localTileId = tileId;

            if (tilemapComponent.Tilesets != null && tilemapComponent.Tilesets.Count > 0)
            {
                // Find which tileset this tile belongs to
                for (int i = tilemapComponent.Tilesets.Count - 1; i >= 0; i--)
                {
                    var ts = tilemapComponent.Tilesets[i];
                    if (tileId >= ts.FirstGid)
                    {
                        tilesetToUse = ts;
                        localTileId = tileId - ts.FirstGid; // Convert to local tile ID
                        break;
                    }
                }

                if (tilesetToUse == null || tilesetToUse.Texture == null)
                    return; // Skip if no valid tileset found
            }
            else
            {
                // Fall back to single tileset mode
                if (texture == null)
                    return;
            }

            // Get tile dimensions from the appropriate tileset
            int tileDimensionWidth = tilesetToUse?.TileWidth ?? tileSet.TileWidth;
            int tileDimensionHeight = tilesetToUse?.TileHeight ?? tileSet.TileHeight;

            // Calculate world position
            Vector2 worldPos = new Vector2(
                x * tileMap.TileWidth,
                y * tileMap.TileHeight
            );

            // Convert to screen position if camera exists
            Vector2 screenPos = camera.HasValue
                ? camera.Value.WorldToScreen(worldPos)
                : worldPos;

            // Get the texture and sprite sheet to use
            var textureToUse = tilesetToUse?.Texture ?? texture;
            var spriteSheetToUse = tilesetToUse?.SpriteSheet ?? spriteSheet;

            // Calculate source rectangle from sprite sheet or fallback to manual calculation
            Rectangle sourceRect;
            if (spriteSheetToUse != null && localTileId < spriteSheetToUse.TotalFrames)
            {
                sourceRect = spriteSheetToUse.GetFrame(localTileId);
            }
            else
            {
                // Fallback: calculate from tile ID
                int tilesPerRow = textureToUse.Width / tileDimensionWidth;
                int srcX = (localTileId % tilesPerRow) * tileDimensionWidth;
                int srcY = (localTileId / tilesPerRow) * tileDimensionHeight;
                sourceRect = new Rectangle(srcX, srcY, tileDimensionWidth, tileDimensionHeight);
            }

            // Draw the tile
            _renderer.DrawSprite(
                textureToUse,
                screenPos,
                sourceRect,
                new Vector2(1, 1), // Scale
                0f, // Rotation
                Color.White // Tint (white = no tint)
            );
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
    }
}
