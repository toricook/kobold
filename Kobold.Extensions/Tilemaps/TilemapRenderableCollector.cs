using Arch.Core;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// Collects tilemap renderables for the unified render system.
    /// Allows tiles to participate in Y-sorting with other entities.
    /// </summary>
    public class TilemapRenderableCollector : IRenderableCollector
    {
        private readonly IRenderer _renderer;

        public TilemapRenderableCollector(IRenderer renderer)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public void CollectRenderables(World world, Camera? camera, List<RenderableItem> renderables)
        {
            var tilemapQuery = new QueryDescription().WithAll<TileMapComponent>();
            world.Query(in tilemapQuery, (Entity entity, ref TileMapComponent tileMapComponent) =>
            {
                var tileMap = tileMapComponent.TileMap;
                var tileSet = tileMapComponent.TileSet;

                if (tileMap == null || tileSet == null)
                    return;

                // Calculate visible tiles based on camera (or render all if no camera)
                int startX = 0, startY = 0;
                int endX = tileMap.Width, endY = tileMap.Height;

                if (camera.HasValue)
                {
                    var bounds = camera.Value.GetViewportBounds();
                    startX = Math.Max(0, (int)(bounds.left / tileSet.TileWidth) - 1);
                    startY = Math.Max(0, (int)(bounds.top / tileSet.TileHeight) - 1);
                    endX = Math.Min(tileMap.Width, (int)(bounds.right / tileSet.TileWidth) + 2);
                    endY = Math.Min(tileMap.Height, (int)(bounds.bottom / tileSet.TileHeight) + 2);
                }

                // Collect all tiles from all layers
                for (int layer = 0; layer < tileMap.LayerCount; layer++)
                {
                    // Check if this layer has Y-sorting enabled
                    bool isYSorted = false;
                    int renderLayer = RenderLayers.Background; // Default layer for non-Y-sorted tilemaps

                    if (tileMapComponent.TileLayers != null && layer < tileMapComponent.TileLayers.Count)
                    {
                        isYSorted = tileMapComponent.TileLayers[layer].YSort;

                        // IMPORTANT: Y-sorted layers must be on the same layer as game objects
                        // so they can sort together by Y position
                        if (isYSorted)
                        {
                            renderLayer = RenderLayers.GameObjects;
                        }
                    }

                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            int tileId = tileMap.GetTile(layer, x, y);
                            if (tileId < 0) continue; // Empty tile

                            // Calculate Y position for sorting (bottom of tile)
                            float yPos = (y + 1) * tileMap.TileHeight;

                            // Capture variables for lambda
                            var tilemapComponentCopy = tileMapComponent;
                            var cameraCopy = camera;
                            var xCopy = x;
                            var yCopy = y;
                            var tileIdCopy = tileId;

                            renderables.Add(new RenderableItem
                            {
                                Layer = renderLayer,
                                YSort = isYSorted,
                                YSortValue = yPos,
                                RenderAction = () => RenderTile(ref tilemapComponentCopy, cameraCopy, xCopy, yCopy, tileIdCopy)
                            });
                        }
                    }
                }
            });
        }

        private void RenderTile(ref TileMapComponent tilemapComponent, Camera? camera, int x, int y, int tileId)
        {
            var tileMap = tilemapComponent.TileMap;
            var tileSet = tilemapComponent.TileSet;

            if (tileMap == null || tileSet == null)
                return;

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
                if (tilemapComponent.Texture == null)
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
            var textureToUse = tilesetToUse?.Texture ?? tilemapComponent.Texture;
            var spriteSheetToUse = tilesetToUse?.SpriteSheet ?? tilemapComponent.SpriteSheet;

            if (textureToUse == null)
                return;

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
    }
}
