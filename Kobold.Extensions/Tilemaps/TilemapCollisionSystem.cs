using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Core.Events;
using System;
using System.Numerics;
using Kobold.Extensions.Collision.Components;
using Kobold.Extensions.Physics.Components;

namespace Kobold.Extensions.Tilemaps
{
    /// <summary>
    /// System that handles collision detection between entities and tilemap tiles.
    /// Integrates with Kobold.Core's physics and collision systems.
    /// </summary>
    public class TilemapCollisionSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus? _eventBus;
        private readonly TilemapCollisionConfig _config;

        public TilemapCollisionSystem(World world, EventBus? eventBus = null, TilemapCollisionConfig? config = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _eventBus = eventBus;
            _config = config ?? new TilemapCollisionConfig();
        }

        public void Update(float deltaTime)
        {
            // Get all tilemaps with collision enabled
            var tilemapQuery = new QueryDescription().WithAll<TileMapComponent, Transform>();

            _world.Query(in tilemapQuery, (Entity tilemapEntity, ref TileMapComponent tilemapComp, ref Transform tilemapTransform) =>
            {
                // TODO: Add EnableCollision property to TileMapComponent if needed
                // if (!tilemapComp.EnableCollision)
                //     return;

                var tileMap = tilemapComp.TileMap;
                var tileSet = tilemapComp.TileSet;
                var tilemapOffset = tilemapTransform.Position;

                // Check all entities with collision components against this tilemap
                var entityQuery = new QueryDescription().WithAll<Transform, BoxCollider, Velocity>();

                _world.Query(in entityQuery, (Entity entity, ref Transform transform, ref BoxCollider collider, ref Velocity velocity) =>
                {
                    // Don't collide with self (if tilemap has a collider)
                    if (entity == tilemapEntity)
                        return;

                    ResolveEntityTileCollision(
                        entity,
                        ref transform,
                        ref collider,
                        ref velocity,
                        tileMap,
                        tileSet,
                        tilemapOffset,
                        deltaTime);
                });
            });
        }

        /// <summary>
        /// Resolves collision between an entity and tiles in the tilemap.
        /// </summary>
        private void ResolveEntityTileCollision(
            Entity entity,
            ref Transform transform,
            ref BoxCollider collider,
            ref Velocity velocity,
            TileMap tileMap,
            TileSet tileSet,
            Vector2 tilemapOffset,
            float deltaTime)
        {
            // Calculate entity bounds in world space
            var entityPos = transform.Position;
            var entitySize = collider.Size;
            var entityOffset = collider.Offset;

            var minX = entityPos.X + entityOffset.X;
            var minY = entityPos.Y + entityOffset.Y;
            var maxX = minX + entitySize.X;
            var maxY = minY + entitySize.Y;

            // Convert to tile coordinates (relative to tilemap offset)
            var relativeMinX = minX - tilemapOffset.X;
            var relativeMinY = minY - tilemapOffset.Y;
            var relativeMaxX = maxX - tilemapOffset.X;
            var relativeMaxY = maxY - tilemapOffset.Y;

            var (startTileX, startTileY) = tileMap.WorldToTile(relativeMinX, relativeMinY);
            var (endTileX, endTileY) = tileMap.WorldToTile(relativeMaxX, relativeMaxY);

            // Expand search by 1 tile in each direction for better collision detection
            startTileX = Math.Max(0, startTileX - 1);
            startTileY = Math.Max(0, startTileY - 1);
            endTileX = Math.Min(tileMap.Width - 1, endTileX + 1);
            endTileY = Math.Min(tileMap.Height - 1, endTileY + 1);

            // Check all nearby tiles across all layers
            for (int layer = 0; layer < tileMap.LayerCount; layer++)
            {
                for (int tileY = startTileY; tileY <= endTileY; tileY++)
                {
                    for (int tileX = startTileX; tileX <= endTileX; tileX++)
                    {
                        int tileId = tileMap.GetTile(layer, tileX, tileY);
                        if (tileId < 0) // Empty tile
                            continue;

                        var tileProps = tileSet.GetTileProperties(tileId);

                        // Handle different collision types
                        switch (tileProps.CollisionLayer)
                        {
                            case TileCollisionLayer.Solid:
                                ResolveSolidCollision(
                                    ref transform,
                                    ref velocity,
                                    collider,
                                    tileMap,
                                    tilemapOffset,
                                    tileX,
                                    tileY,
                                    tileProps);
                                break;

                            case TileCollisionLayer.Platform:
                                ResolvePlatformCollision(
                                    ref transform,
                                    ref velocity,
                                    collider,
                                    tileMap,
                                    tilemapOffset,
                                    tileX,
                                    tileY,
                                    tileProps);
                                break;

                            case TileCollisionLayer.Trigger:
                                HandleTriggerCollision(entity, tileX, tileY, tileProps);
                                break;

                            case TileCollisionLayer.Water:
                            case TileCollisionLayer.Ice:
                                ApplyTileEffects(ref velocity, tileProps, deltaTime);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resolves collision with solid tiles (AABB collision resolution).
        /// </summary>
        private void ResolveSolidCollision(
            ref Transform transform,
            ref Velocity velocity,
            BoxCollider collider,
            TileMap tileMap,
            Vector2 tilemapOffset,
            int tileX,
            int tileY,
            TileProperties tileProps)
        {
            if (!tileProps.IsSolid)
                return;

            // Get tile bounds in world space
            var (tilePosX, tilePosY) = tileMap.TileToWorld(tileX, tileY);
            tilePosX += tilemapOffset.X;
            tilePosY += tilemapOffset.Y;

            float tileLeft = tilePosX;
            float tileRight = tilePosX + tileMap.TileWidth;
            float tileTop = tilePosY;
            float tileBottom = tilePosY + tileMap.TileHeight;

            // Get entity bounds
            var entityPos = transform.Position + collider.Offset;
            float entityLeft = entityPos.X;
            float entityRight = entityPos.X + collider.Size.X;
            float entityTop = entityPos.Y;
            float entityBottom = entityPos.Y + collider.Size.Y;

            // Check for overlap
            bool overlaps = entityLeft < tileRight &&
                           entityRight > tileLeft &&
                           entityTop < tileBottom &&
                           entityBottom > tileTop;

            if (!overlaps)
                return;

            // Calculate overlap amounts
            float overlapLeft = entityRight - tileLeft;
            float overlapRight = tileRight - entityLeft;
            float overlapTop = entityBottom - tileTop;
            float overlapBottom = tileBottom - entityTop;

            // Find minimum overlap (shortest resolution)
            float minOverlap = Math.Min(
                Math.Min(overlapLeft, overlapRight),
                Math.Min(overlapTop, overlapBottom));

            // Resolve collision by moving entity out of tile
            if (Math.Abs(minOverlap - overlapLeft) < 0.01f)
            {
                // Push left
                transform.Position = new Vector2(
                    transform.Position.X - overlapLeft,
                    transform.Position.Y);
                velocity.Value = new Vector2(Math.Min(0, velocity.Value.X), velocity.Value.Y);
            }
            else if (Math.Abs(minOverlap - overlapRight) < 0.01f)
            {
                // Push right
                transform.Position = new Vector2(
                    transform.Position.X + overlapRight,
                    transform.Position.Y);
                velocity.Value = new Vector2(Math.Max(0, velocity.Value.X), velocity.Value.Y);
            }
            else if (Math.Abs(minOverlap - overlapTop) < 0.01f)
            {
                // Push up
                transform.Position = new Vector2(
                    transform.Position.X,
                    transform.Position.Y - overlapTop);
                velocity.Value = new Vector2(velocity.Value.X, Math.Min(0, velocity.Value.Y));
            }
            else if (Math.Abs(minOverlap - overlapBottom) < 0.01f)
            {
                // Push down
                transform.Position = new Vector2(
                    transform.Position.X,
                    transform.Position.Y + overlapBottom);
                velocity.Value = new Vector2(velocity.Value.X, Math.Max(0, velocity.Value.Y));
            }

            // Apply friction if configured
            if (_config.ApplyTileFriction && tileProps.Friction > 0)
            {
                velocity.Value *= tileProps.Friction;
            }
        }

        /// <summary>
        /// Resolves collision with platform tiles (one-way collision from above).
        /// </summary>
        private void ResolvePlatformCollision(
            ref Transform transform,
            ref Velocity velocity,
            BoxCollider collider,
            TileMap tileMap,
            Vector2 tilemapOffset,
            int tileX,
            int tileY,
            TileProperties tileProps)
        {
            // Platforms only collide when entity is moving downward and above the platform
            if (velocity.Value.Y <= 0)
                return;

            var (tilePosX, tilePosY) = tileMap.TileToWorld(tileX, tileY);
            tilePosX += tilemapOffset.X;
            tilePosY += tilemapOffset.Y;

            var entityPos = transform.Position + collider.Offset;
            float entityBottom = entityPos.Y + collider.Size.Y;
            float entityLeft = entityPos.X;
            float entityRight = entityPos.X + collider.Size.X;

            float tileTop = tilePosY;
            float tileLeft = tilePosX;
            float tileRight = tilePosX + tileMap.TileWidth;

            // Check if entity is horizontally aligned with platform
            if (entityRight > tileLeft && entityLeft < tileRight)
            {
                // Check if entity bottom is near platform top (with small threshold)
                float threshold = 5f; // pixels
                if (entityBottom >= tileTop && entityBottom <= tileTop + threshold)
                {
                    // Snap to platform top
                    transform.Position = new Vector2(
                        transform.Position.X,
                        tileTop - collider.Size.Y - collider.Offset.Y);
                    velocity.Value = new Vector2(velocity.Value.X, 0);
                }
            }
        }

        /// <summary>
        /// Handles trigger collision (publishes event but doesn't resolve).
        /// </summary>
        private void HandleTriggerCollision(Entity entity, int tileX, int tileY, TileProperties tileProps)
        {
            if (_eventBus == null)
                return;

            // Publish tile trigger event
            var triggerEvent = new TileTriggerEvent(entity, tileX, tileY, tileProps.Type ?? "trigger");
            _eventBus.Publish(triggerEvent);

            // Apply damage if tile has damage
            if (tileProps.Damage > 0)
            {
                var damageEvent = new TileDamageEvent(entity, tileProps.Damage, tileProps.Type ?? "tile");
                _eventBus.Publish(damageEvent);
            }
        }

        /// <summary>
        /// Applies tile effects like friction, water resistance, etc.
        /// </summary>
        private void ApplyTileEffects(ref Velocity velocity, TileProperties tileProps, float deltaTime)
        {
            // Apply friction/resistance
            if (tileProps.Friction != 1.0f)
            {
                velocity.Value *= tileProps.Friction;
            }

            // Handle damage over time (like lava)
            // This would need additional state tracking per entity
        }

        /// <summary>
        /// Checks if a specific world position collides with any solid tile.
        /// </summary>
        public bool IsPositionSolid(Vector2 worldPosition, TileMap tileMap, TileSet tileSet, Vector2 tilemapOffset)
        {
            var relativePos = worldPosition - tilemapOffset;
            var (tileX, tileY) = tileMap.WorldToTile(relativePos.X, relativePos.Y);

            if (!tileMap.IsValidPosition(tileX, tileY))
                return false;

            for (int layer = 0; layer < tileMap.LayerCount; layer++)
            {
                int tileId = tileMap.GetTile(layer, tileX, tileY);
                if (tileId >= 0)
                {
                    var props = tileSet.GetTileProperties(tileId);
                    if (props.IsSolid || props.CollisionLayer == TileCollisionLayer.Solid)
                        return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Configuration for tilemap collision system.
    /// </summary>
    public class TilemapCollisionConfig
    {
        /// <summary>
        /// Whether to apply tile friction to entities.
        /// </summary>
        public bool ApplyTileFriction { get; set; } = true;

        /// <summary>
        /// Whether to handle platform (one-way) collision.
        /// </summary>
        public bool EnablePlatformCollision { get; set; } = true;

        /// <summary>
        /// Whether to publish collision events.
        /// </summary>
        public bool PublishCollisionEvents { get; set; } = true;
    }

    /// <summary>
    /// Event published when an entity triggers a tile.
    /// </summary>
    public class TileTriggerEvent : BaseEvent
    {
        public Entity Entity { get; }
        public int TileX { get; }
        public int TileY { get; }
        public string TriggerType { get; }

        public TileTriggerEvent(Entity entity, int tileX, int tileY, string triggerType)
        {
            Entity = entity;
            TileX = tileX;
            TileY = tileY;
            TriggerType = triggerType;
        }
    }

    /// <summary>
    /// Event published when an entity takes damage from a tile.
    /// </summary>
    public class TileDamageEvent : BaseEvent
    {
        public Entity Entity { get; }
        public int Damage { get; }
        public string DamageType { get; }

        public TileDamageEvent(Entity entity, int damage, string damageType)
        {
            Entity = entity;
            Damage = damage;
            DamageType = damageType;
        }
    }
}
