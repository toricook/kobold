using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using System;

namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// System that manages tilemap updates and animated tiles.
    /// Rendering is handled by platform-specific renderers that query TilemapComponent.
    /// </summary>
    public class TilemapSystem : ISystem
    {
        private readonly World _world;
        private float _animationTime;

        public TilemapSystem(World world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _animationTime = 0f;
        }

        public void Update(float deltaTime)
        {
            _animationTime += deltaTime;

            // Update animated tiles
            var query = new QueryDescription().WithAll<TileMapComponent>();

            _world.Query(in query, (ref TileMapComponent tilemapComponent) =>
            {
                // TODO: Add Visible property to TileMapComponent if needed
                UpdateAnimatedTiles(ref tilemapComponent, deltaTime);
            });
        }

        /// <summary>
        /// Updates animated tiles in the tilemap.
        /// </summary>
        private void UpdateAnimatedTiles(ref TileMapComponent tilemapComponent, float deltaTime)
        {
            var tileMap = tilemapComponent.TileMap;
            var tileSet = tilemapComponent.TileSet;

            // TODO: Implement animated tile tracking and frame updates
            // For now, this is a placeholder that would track which tiles are animated
            // and update their frame indices based on animation speed

            // In a full implementation, you would:
            // 1. Track animated tile instances (position + current frame index)
            // 2. Update frame indices based on AnimationSpeed and deltaTime
            // 3. Store current frame in a separate data structure
            // The actual rendering would use this frame index to pick the right tile ID
        }

        /// <summary>
        /// Gets all visible tiles for a tilemap at a given layer within the camera viewport.
        /// Used by renderers to efficiently draw only visible tiles.
        /// </summary>
        /// <param name="tileMap">The tilemap</param>
        /// <param name="layer">Layer to query</param>
        /// <param name="cameraX">Camera X position in world space</param>
        /// <param name="cameraY">Camera Y position in world space</param>
        /// <param name="viewportWidth">Viewport width in pixels</param>
        /// <param name="viewportHeight">Viewport height in pixels</param>
        /// <returns>List of visible tiles with their positions</returns>
        public static System.Collections.Generic.List<(int x, int y, int tileId)> GetVisibleTiles(
            TileMap tileMap,
            int layer,
            float cameraX,
            float cameraY,
            int viewportWidth,
            int viewportHeight)
        {
            // Calculate tile bounds for the viewport
            var (startTileX, startTileY) = tileMap.WorldToTile(cameraX, cameraY);
            var (endTileX, endTileY) = tileMap.WorldToTile(
                cameraX + viewportWidth,
                cameraY + viewportHeight);

            // Clamp to tilemap bounds
            startTileX = Math.Max(0, startTileX);
            startTileY = Math.Max(0, startTileY);
            endTileX = Math.Min(tileMap.Width - 1, endTileX);
            endTileY = Math.Min(tileMap.Height - 1, endTileY);

            // Calculate dimensions
            int width = endTileX - startTileX + 1;
            int height = endTileY - startTileY + 1;

            return tileMap.GetTilesInBounds(layer, startTileX, startTileY, width, height);
        }

        /// <summary>
        /// Helper method to check if a tilemap entity exists.
        /// </summary>
        public bool HasTilemap(Entity entity)
        {
            if (!_world.IsAlive(entity))
                return false;

            return _world.Has<TileMapComponent>(entity);
        }

        /// <summary>
        /// Gets the TileMapComponent for an entity.
        /// </summary>
        public TileMapComponent? GetTilemap(Entity entity)
        {
            if (!_world.IsAlive(entity) || !_world.Has<TileMapComponent>(entity))
                return null;

            return _world.Get<TileMapComponent>(entity);
        }
    }
}
