using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Extensions.Tilemaps;
using System;
using System.Numerics;

namespace CaveExplorer.Systems
{
    /// <summary>
    /// Prevents entities from moving through solid tiles in the tilemap.
    /// This system runs before PhysicsSystem to check collisions and block movement.
    /// </summary>
    public class TileMapCollisionSystem : ISystem
    {
        private readonly World _world;
        private TileMapComponent? _tileMapComponent;

        public TileMapCollisionSystem(World world)
        {
            _world = world;
        }

        public void Update(float deltaTime)
        {
            // Get the tilemap component (cache it for this frame)
            _tileMapComponent = GetTileMapComponent();

            if (_tileMapComponent == null)
                return; // No tilemap in the world yet

            // Query for all entities that have movement and collision
            var query = new QueryDescription().WithAll<Transform, Velocity, BoxCollider>();

            _world.Query(in query, (Entity entity, ref Transform transform, ref Velocity velocity, ref BoxCollider collider) =>
            {
                CheckAndResolveCollision(ref transform, ref velocity, ref collider, deltaTime);
            });
        }

        private TileMapComponent? GetTileMapComponent()
        {
            var query = new QueryDescription().WithAll<TileMapComponent>();
            TileMapComponent? result = null;

            _world.Query(in query, (Entity entity, ref TileMapComponent tileMapComponent) =>
            {
                result = tileMapComponent;
            });

            return result;
        }

        private void CheckAndResolveCollision(ref Transform transform, ref Velocity velocity, ref BoxCollider collider, float deltaTime)
        {
            if (_tileMapComponent == null)
                return;

            var tileMap = _tileMapComponent.Value.TileMap;
            var tileSet = _tileMapComponent.Value.TileSet;

            // If not moving, no collision check needed
            if (velocity.Value == Vector2.Zero)
                return;

            // Get current position and next position
            var currentPos = transform.Position;
            var nextPos = currentPos + velocity.Value * deltaTime;

            // Get the collision box bounds at the next position
            var colliderCenter = collider.GetWorldCenter(nextPos);

            // Calculate the bounds of the collision box to check multiple tiles
            var halfSize = collider.Size / 2f;
            var minX = colliderCenter.X - halfSize.X;
            var maxX = colliderCenter.X + halfSize.X;
            var minY = colliderCenter.Y - halfSize.Y;
            var maxY = colliderCenter.Y + halfSize.Y;

            // Check horizontal movement (X axis)
            bool blockedX = false;
            if (velocity.Value.X != 0)
            {
                // Check the leading edge in the X direction
                float checkX = velocity.Value.X > 0 ? maxX : minX;

                // Check multiple points along the Y axis of the leading edge
                for (float y = minY; y <= maxY; y += tileMap.TileHeight / 2f)
                {
                    var (tileX, tileY) = tileMap.WorldToTile(checkX, y);
                    if (tileMap.IsValidPosition(tileX, tileY))
                    {
                        int tileId = tileMap.GetTile(0, tileX, tileY);
                        if (tileId >= 0 && tileSet.IsSolid(tileId))
                        {
                            blockedX = true;
                            break;
                        }
                    }
                    else
                    {
                        // Out of bounds counts as blocked
                        blockedX = true;
                        break;
                    }
                }
            }

            // Check vertical movement (Y axis)
            bool blockedY = false;
            if (velocity.Value.Y != 0)
            {
                // Check the leading edge in the Y direction
                float checkY = velocity.Value.Y > 0 ? maxY : minY;

                // Check multiple points along the X axis of the leading edge
                for (float x = minX; x <= maxX; x += tileMap.TileWidth / 2f)
                {
                    var (tileX, tileY) = tileMap.WorldToTile(x, checkY);
                    if (tileMap.IsValidPosition(tileX, tileY))
                    {
                        int tileId = tileMap.GetTile(0, tileX, tileY);
                        if (tileId >= 0 && tileSet.IsSolid(tileId))
                        {
                            blockedY = true;
                            break;
                        }
                    }
                    else
                    {
                        // Out of bounds counts as blocked
                        blockedY = true;
                        break;
                    }
                }
            }

            // Resolve collisions by zeroing out blocked velocity components
            // This allows sliding along walls (e.g., can move horizontally even if blocked vertically)
            if (blockedX)
            {
                velocity.Value = new Vector2(0, velocity.Value.Y);
            }

            if (blockedY)
            {
                velocity.Value = new Vector2(velocity.Value.X, 0);
            }
        }
    }
}
