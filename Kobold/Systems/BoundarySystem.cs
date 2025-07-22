using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    /// <summary>
    /// Enhanced boundary system with per-entity behavior and filtering
    /// </summary>
    public class BoundarySystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly BoundaryConfig _config;

        public BoundarySystem(World world, EventBus eventBus, BoundaryConfig config)
        {
            _world = world;
            _eventBus = eventBus;
            _config = config;
        }

        public void Update(float deltaTime)
        {
            // Check entities with box colliders
            var boxQuery = new QueryDescription().WithAll<Transform, BoxCollider>();
            _world.Query(in boxQuery, (Entity entity, ref Transform transform, ref BoxCollider collider) =>
            {
                var bounds = GetEntityBounds(transform.Position, collider.Size, collider.Offset);
                var behavior = GetBoundaryBehavior(entity);
                CheckAndHandleBoundaryCollision(entity, ref transform, bounds, behavior);
            });

            // Check entities with no collider but with a BoundarySize component (for entities that need boundary checking but no collision)
            var boundarySizeQuery = new QueryDescription().WithAll<Transform, BoundarySize>().WithNone<BoxCollider>();
            _world.Query(in boundarySizeQuery, (Entity entity, ref Transform transform, ref BoundarySize boundarySize) =>
            {
                var bounds = GetEntityBounds(transform.Position, boundarySize.Size, Vector2.Zero);
                var behavior = GetBoundaryBehavior(entity);
                CheckAndHandleBoundaryCollision(entity, ref transform, bounds, behavior);
            });
        }

        private void CheckAndHandleBoundaryCollision(Entity entity, ref Transform transform, EntityBounds bounds, BoundaryBehavior behavior)
        {
            // Check each boundary
            if (bounds.Left < _config.Bounds.Left)
                HandleBoundaryCollision(entity, ref transform, BoundaryType.Left, bounds, behavior);
            else if (bounds.Right > _config.Bounds.Right)
                HandleBoundaryCollision(entity, ref transform, BoundaryType.Right, bounds, behavior);

            if (bounds.Top < _config.Bounds.Top)
                HandleBoundaryCollision(entity, ref transform, BoundaryType.Top, bounds, behavior);
            else if (bounds.Bottom > _config.Bounds.Bottom)
                HandleBoundaryCollision(entity, ref transform, BoundaryType.Bottom, bounds, behavior);
        }

        private BoundaryBehavior GetBoundaryBehavior(Entity entity)
        {
            // Check if entity has custom boundary behavior
            if (_world.Has<CustomBoundaryBehavior>(entity))
            {
                ref var customBehavior = ref _world.Get<CustomBoundaryBehavior>(entity);
                return customBehavior.Behavior;
            }

            // Check for specific component types and apply defaults
            if (_world.Has<Player>(entity))
            {
                return _config.PlayerBehavior;
            }
            else if (_world.Has<Enemy>(entity))
            {
                return _config.EnemyBehavior;
            }
            else if (_world.Has<Projectile>(entity))
            {
                return _config.ProjectileBehavior;
            }

            // Default behavior
            return _config.DefaultBehavior;
        }

        private void HandleBoundaryCollision(Entity entity, ref Transform transform, BoundaryType boundary,
            EntityBounds bounds, BoundaryBehavior behavior)
        {
            switch (behavior)
            {
                case BoundaryBehavior.Clamp:
                    ClampToBoundary(ref transform, boundary, bounds);
                    break;
                case BoundaryBehavior.Wrap:
                    WrapAroundBoundary(ref transform, boundary, bounds);
                    break;
                case BoundaryBehavior.Bounce:
                    BounceOffBoundary(entity, ref transform, boundary, bounds);
                    break;
                case BoundaryBehavior.Destroy:
                    DestroyEntity(entity);
                    break;
                case BoundaryBehavior.Ignore:
                    // Do nothing
                    break;
            }

            // Publish boundary collision event (unless ignoring)
            if (behavior != BoundaryBehavior.Ignore)
            {
                _eventBus.Publish(new BoundaryCollisionEvent(entity, boundary, behavior));
            }
        }

        private void ClampToBoundary(ref Transform transform, BoundaryType boundary, EntityBounds bounds)
        {
            switch (boundary)
            {
                case BoundaryType.Left:
                    transform.Position = new Vector2(_config.Bounds.Left, transform.Position.Y);
                    break;
                case BoundaryType.Right:
                    transform.Position = new Vector2(_config.Bounds.Right - bounds.Width, transform.Position.Y);
                    break;
                case BoundaryType.Top:
                    transform.Position = new Vector2(transform.Position.X, _config.Bounds.Top);
                    break;
                case BoundaryType.Bottom:
                    transform.Position = new Vector2(transform.Position.X, _config.Bounds.Bottom - bounds.Height);
                    break;
            }
        }

        private void WrapAroundBoundary(ref Transform transform, BoundaryType boundary, EntityBounds bounds)
        {
            switch (boundary)
            {
                case BoundaryType.Left:
                    // When going off left side, appear on right side (inside the boundary)
                    transform.Position = new Vector2(_config.Bounds.Right - bounds.Width, transform.Position.Y);
                    break;
                case BoundaryType.Right:
                    // When going off right side, appear on left side (inside the boundary)
                    transform.Position = new Vector2(_config.Bounds.Left, transform.Position.Y);
                    break;
                case BoundaryType.Top:
                    // When going off top, appear at bottom (inside the boundary)
                    transform.Position = new Vector2(transform.Position.X, _config.Bounds.Bottom - bounds.Height);
                    break;
                case BoundaryType.Bottom:
                    // When going off bottom, appear at top (inside the boundary)
                    transform.Position = new Vector2(transform.Position.X, _config.Bounds.Top);
                    break;
            }
        }

        private void BounceOffBoundary(Entity entity, ref Transform transform, BoundaryType boundary, EntityBounds bounds)
        {
            // Clamp position first
            ClampToBoundary(ref transform, boundary, bounds);

            // Reverse velocity if entity has one
            if (_world.Has<Velocity>(entity))
            {
                ref var velocity = ref _world.Get<Velocity>(entity);

                // Get restitution (bounciness) factor
                float restitution = 1.0f; // Default perfect bounce
                if (_world.Has<Physics>(entity))
                {
                    ref var physics = ref _world.Get<Physics>(entity);
                    restitution = physics.Restitution;
                }

                switch (boundary)
                {
                    case BoundaryType.Left:
                    case BoundaryType.Right:
                        velocity.Value = new Vector2(-velocity.Value.X * restitution, velocity.Value.Y);
                        break;
                    case BoundaryType.Top:
                    case BoundaryType.Bottom:
                        velocity.Value = new Vector2(velocity.Value.X, -velocity.Value.Y * restitution);
                        break;
                }
            }
        }

        private void DestroyEntity(Entity entity)
        {
            // Safety check - make sure entity still exists before destroying
            if (!_world.IsAlive(entity))
            {
                Console.WriteLine($"WARNING: Attempted to destroy already dead entity: {entity}");
                return;
            }

            // Use safe destruction instead of immediate destruction
            DestructionSystem.MarkForDestruction(_world, entity, DestructionReason.BoundaryExit);
        }

        private EntityBounds GetEntityBounds(Vector2 position, Vector2 size, Vector2 offset)
        {
            var actualPosition = position + offset;
            return new EntityBounds
            {
                Left = actualPosition.X,
                Right = actualPosition.X + size.X,
                Top = actualPosition.Y,
                Bottom = actualPosition.Y + size.Y,
                Width = size.X,
                Height = size.Y
            };
        }

        /// <summary>
        /// Check if a position is within the boundary
        /// </summary>
        public bool IsWithinBounds(Vector2 position)
        {
            return position.X >= _config.Bounds.Left && position.X <= _config.Bounds.Right &&
                   position.Y >= _config.Bounds.Top && position.Y <= _config.Bounds.Bottom;
        }

        /// <summary>
        /// Get a random position within the boundary
        /// </summary>
        public Vector2 GetRandomPosition(Vector2 entitySize)
        {
            var availableWidth = _config.Bounds.Width - entitySize.X;
            var availableHeight = _config.Bounds.Height - entitySize.Y;

            return new Vector2(
                _config.Bounds.Left + Random.Shared.NextSingle() * availableWidth,
                _config.Bounds.Top + Random.Shared.NextSingle() * availableHeight
            );
        }
    }

    /// <summary>
    /// Enhanced boundary configuration with per-entity behavior
    /// </summary>
    public class BoundaryConfig
    {
        public BoundaryRect Bounds { get; set; } = new BoundaryRect(0, 0, 800, 600);

        // Default behaviors for different entity types
        public BoundaryBehavior DefaultBehavior { get; set; } = BoundaryBehavior.Clamp;
        public BoundaryBehavior PlayerBehavior { get; set; } = BoundaryBehavior.Wrap;
        public BoundaryBehavior EnemyBehavior { get; set; } = BoundaryBehavior.Wrap;
        public BoundaryBehavior ProjectileBehavior { get; set; } = BoundaryBehavior.Destroy;

        public BoundaryConfig(float width, float height)
        {
            Bounds = new BoundaryRect(0, 0, width, height);
        }

        public BoundaryConfig(BoundaryRect bounds)
        {
            Bounds = bounds;
        }
    }

    /// <summary>
    /// Component to override boundary behavior for specific entities
    /// </summary>
    public struct CustomBoundaryBehavior
    {
        public BoundaryBehavior Behavior;

        public CustomBoundaryBehavior(BoundaryBehavior behavior)
        {
            Behavior = behavior;
        }
    }

    /// <summary>
    /// Boundary rectangle definition
    /// </summary>
    public struct BoundaryRect
    {
        public float Left, Top, Right, Bottom;

        public BoundaryRect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public BoundaryRect(float x, float y, float width, float height, bool isWidthHeight = true)
        {
            if (isWidthHeight)
            {
                Left = x;
                Top = y;
                Right = x + width;
                Bottom = y + height;
            }
            else
            {
                Left = x;
                Top = y;
                Right = width;
                Bottom = height;
            }
        }

        public float Width => Right - Left;
        public float Height => Bottom - Top;
        public Vector2 Center => new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
    }

    public enum BoundaryBehavior
    {
        Clamp,    // Stop at boundary
        Wrap,     // Teleport to opposite side
        Bounce,   // Reverse velocity
        Destroy,  // Remove entity
        Ignore    // Pass through boundary
    }

    public enum BoundaryType
    {
        Top, Bottom, Left, Right
    }

    /// <summary>
    /// Enhanced boundary collision event with behavior information
    /// </summary>
    public class BoundaryCollisionEvent : BaseEvent
    {
        public Entity Entity { get; }
        public BoundaryType Boundary { get; }
        public BoundaryBehavior Behavior { get; }

        public BoundaryCollisionEvent(Entity entity, BoundaryType boundary, BoundaryBehavior behavior)
        {
            Entity = entity;
            Boundary = boundary;
            Behavior = behavior;
        }
    }

    /// <summary>
    /// Component for entities that need boundary checking without collision
    /// (e.g., visual effects, UI elements that should wrap)
    /// </summary>
    public struct BoundarySize
    {
        public Vector2 Size;

        public BoundarySize(Vector2 size)
        {
            Size = size;
        }

        public BoundarySize(float width, float height)
        {
            Size = new Vector2(width, height);
        }
    }

    internal struct EntityBounds
    {
        public float Left, Right, Top, Bottom, Width, Height;
    }
}