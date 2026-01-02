using Arch.Core;
using Kobold.Core.Assets;
using Kobold.Core.Components;
using Kobold.Core.Components.Gameplay;
using Kobold.Core.Systems;
using Kobold.Extensions.Portals;
using Kobold.Extensions.Tilemaps;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Kobold.Extensions.Progression
{
    /// <summary>
    /// Factory for creating stair entities and finding valid spawn positions.
    /// Stairs are portals that use LevelGenerationDestination to trigger level transitions.
    /// </summary>
    public static class StairFactory
    {
        /// <summary>
        /// Creates a stairUp portal entity that leads to the previous level.
        /// Requires player to step on it and press E to activate.
        /// </summary>
        public static Entity CreateStairUp(
            World world,
            Vector2 position,
            int currentDepth,
            SpriteSheet spriteSheet)
        {
            return world.Create(
                new Transform(position),
                new BoxCollider(32f, 32f, new Vector2(-16f, -16f)),  // Centered 32x32 trigger
                new SpriteRenderer(
                    spriteSheet.Texture,
                    spriteSheet.GetNamedRegion("stairUp"),
                    new Vector2(1f, 1f),
                    layer: -10  // Render below GameObjects layer (0)
                ),
                new Trigger(),  // Non-solid trigger zone
                new CollisionLayerComponent(CollisionLayer.Trigger),
                new TriggerComponent(
                    mode: TriggerMode.OnEnter | TriggerMode.RequiresButton,
                    activationLayers: CollisionLayer.Player,
                    triggerTag: "stair_up"
                ),
                new PortalComponent(
                    destination: new LevelGenerationDestination(
                        levelId: "depth_up",
                        generationParams: new Dictionary<string, object>
                        {
                            { "direction", "up" },
                            { "fromDepth", currentDepth }
                        }
                    ),
                    teleportCooldown: 1f,
                    portalTag: "stair_up"
                ),
                new StairComponent(StairDirection.Up, currentDepth - 1)
            );
        }

        /// <summary>
        /// Creates a stairDown portal entity that leads to the next level.
        /// Requires player to step on it and press E to activate.
        /// </summary>
        public static Entity CreateStairDown(
            World world,
            Vector2 position,
            int currentDepth,
            SpriteSheet spriteSheet)
        {
            return world.Create(
                new Transform(position),
                new BoxCollider(32f, 32f, new Vector2(-16f, -16f)),  // Centered 32x32 trigger
                new SpriteRenderer(
                    spriteSheet.Texture,
                    spriteSheet.GetNamedRegion("stairDown"),
                    new Vector2(1f, 1f),
                    layer: -10  // Render below GameObjects layer (0)
                ),
                new Trigger(),  // Non-solid trigger zone
                new CollisionLayerComponent(CollisionLayer.Trigger),
                new TriggerComponent(
                    mode: TriggerMode.OnEnter | TriggerMode.RequiresButton,
                    activationLayers: CollisionLayer.Player,
                    triggerTag: "stair_down"
                ),
                new PortalComponent(
                    destination: new LevelGenerationDestination(
                        levelId: "depth_down",
                        generationParams: new Dictionary<string, object>
                        {
                            { "direction", "down" },
                            { "fromDepth", currentDepth }
                        }
                    ),
                    teleportCooldown: 1f,
                    portalTag: "stair_down"
                ),
                new StairComponent(StairDirection.Down, currentDepth + 1)
            );
        }

        /// <summary>
        /// Finds a valid spawn position on a tilemap (floor tile, not wall).
        /// Searches in expanding radius from start position.
        /// </summary>
        public static Vector2 FindValidSpawnPosition(TileMap tileMap, int startX, int startY)
        {
            // Try specified position first
            if (tileMap.IsValidPosition(startX, startY) && tileMap.GetTile(0, startX, startY) == 0) // Floor tile
            {
                var (worldX, worldY) = tileMap.TileToWorldCenter(startX, startY);
                return new Vector2(worldX, worldY);
            }

            // Search in expanding radius
            for (int radius = 1; radius < 20; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int x = startX + dx;
                        int y = startY + dy;

                        if (tileMap.IsValidPosition(x, y) && tileMap.GetTile(0, x, y) == 0)
                        {
                            var (worldX, worldY) = tileMap.TileToWorldCenter(x, y);
                            return new Vector2(worldX, worldY);
                        }
                    }
                }
            }

            // Fallback: return requested position anyway
            var (fallbackX, fallbackY) = tileMap.TileToWorldCenter(startX, startY);
            return new Vector2(fallbackX, fallbackY);
        }

        /// <summary>
        /// Finds two valid positions far apart for stairs (up and down).
        /// Uses different quadrants to ensure separation.
        /// </summary>
        public static (Vector2 stairUpPos, Vector2 stairDownPos) FindStairPositions(TileMap tileMap)
        {
            var pos1 = FindValidSpawnPosition(tileMap, tileMap.Width / 4, tileMap.Height / 4);

            Vector2 pos2;
            int attempts = 0;
            do
            {
                pos2 = FindValidSpawnPosition(tileMap, (3 * tileMap.Width) / 4, (3 * tileMap.Height) / 4);
                attempts++;
            } while (Vector2.Distance(pos1, pos2) < 10 * tileMap.TileWidth && attempts < 10);

            return (pos1, pos2);
        }
    }
}
