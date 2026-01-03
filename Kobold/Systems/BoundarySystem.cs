using Arch.Core;
using Kobold.Core.Abstractions.Engine;
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
    /// Handles boundary collisions and wrapping
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
            var query = new QueryDescription().WithAll<Transform, BoxCollider>();

            _world.Query(in query, (Entity entity, ref Transform transform, ref BoxCollider collider) =>
            {
                var bounds = GetEntityBounds(transform.Position, collider.Size);

                // Check each boundary
                if (bounds.Left < 0)
                    HandleBoundaryCollision(entity, ref transform, BoundaryType.Left, bounds);
                else if (bounds.Right > _config.Width)
                    HandleBoundaryCollision(entity, ref transform, BoundaryType.Right, bounds);

                if (bounds.Top < 0)
                    HandleBoundaryCollision(entity, ref transform, BoundaryType.Top, bounds);
                else if (bounds.Bottom > _config.Height)
                    HandleBoundaryCollision(entity, ref transform, BoundaryType.Bottom, bounds);
            });
        }

        private BoundaryBehavior GetBehaviorForEntity(Entity entity)
        {
            // Check for custom behavior component first
            if (_world.Has<Kobold.Core.Components.Gameplay.CustomBoundaryBehavior>(entity))
            {
                return _world.Get<Kobold.Core.Components.Gameplay.CustomBoundaryBehavior>(entity).Behavior;
            }

            // Check entity tags
            if (_world.Has<Projectile>(entity))
                return _config.ProjectileBehavior;

            if (_world.Has<Player>(entity))
                return _config.PlayerBehavior;

            if (_world.Has<Enemy>(entity))
                return _config.EnemyBehavior;

            // Fall back to default behavior
            return _config.DefaultBehavior;
        }

        private void HandleBoundaryCollision(Entity entity, ref Transform transform, BoundaryType boundary, EntityBounds bounds)
        {
            var behavior = GetBehaviorForEntity(entity);

            switch (behavior)
            {
                case BoundaryBehavior.Clamp:
                    ClampToBoundary(ref transform, boundary, bounds);
                    break;
                case BoundaryBehavior.Wrap:
                    WrapAroundBoundary(ref transform, boundary);
                    break;
                case BoundaryBehavior.Bounce:
                    BounceOffBoundary(entity, ref transform, boundary, bounds);
                    break;
                case BoundaryBehavior.Destroy:
                    DestructionSystem.MarkForDestruction(_world, entity, DestructionReason.BoundaryExit);
                    break;
            }

            _eventBus?.Publish(new BoundaryCollisionEvent(entity, boundary));
        }

        private void ClampToBoundary(ref Transform transform, BoundaryType boundary, EntityBounds bounds)
        {
            switch (boundary)
            {
                case BoundaryType.Left:
                    transform.Position = new Vector2(0, transform.Position.Y);
                    break;
                case BoundaryType.Right:
                    transform.Position = new Vector2(_config.Width - bounds.Width, transform.Position.Y);
                    break;
                case BoundaryType.Top:
                    transform.Position = new Vector2(transform.Position.X, 0);
                    break;
                case BoundaryType.Bottom:
                    transform.Position = new Vector2(transform.Position.X, _config.Height - bounds.Height);
                    break;
            }
        }

        private void WrapAroundBoundary(ref Transform transform, BoundaryType boundary)
        {
            switch (boundary)
            {
                case BoundaryType.Left:
                    transform.Position = new Vector2(_config.Width, transform.Position.Y);
                    break;
                case BoundaryType.Right:
                    transform.Position = new Vector2(0, transform.Position.Y);
                    break;
                case BoundaryType.Top:
                    transform.Position = new Vector2(transform.Position.X, _config.Height);
                    break;
                case BoundaryType.Bottom:
                    transform.Position = new Vector2(transform.Position.X, 0);
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
                switch (boundary)
                {
                    case BoundaryType.Left:
                    case BoundaryType.Right:
                        velocity.Value = new Vector2(-velocity.Value.X, velocity.Value.Y);
                        break;
                    case BoundaryType.Top:
                    case BoundaryType.Bottom:
                        velocity.Value = new Vector2(velocity.Value.X, -velocity.Value.Y);
                        break;
                }
            }
        }

        private EntityBounds GetEntityBounds(Vector2 position, Vector2 size)
        {
            return new EntityBounds
            {
                Left = position.X,
                Right = position.X + size.X,
                Top = position.Y,
                Bottom = position.Y + size.Y,
                Width = size.X,
                Height = size.Y
            };
        }
    }

    public class BoundaryConfig
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public BoundaryBehavior BoundaryBehavior { get; set; } = BoundaryBehavior.Clamp;

        // Per-entity-type behavior overrides
        public BoundaryBehavior DefaultBehavior { get; set; } = BoundaryBehavior.Clamp;
        public BoundaryBehavior PlayerBehavior { get; set; } = BoundaryBehavior.Clamp;
        public BoundaryBehavior ProjectileBehavior { get; set; } = BoundaryBehavior.Clamp;
        public BoundaryBehavior EnemyBehavior { get; set; } = BoundaryBehavior.Clamp;

        public BoundaryConfig()
        {
        }

        public BoundaryConfig(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }

    public enum BoundaryBehavior
    {
        Clamp,    // Stop at boundary
        Wrap,     // Teleport to opposite side
        Bounce,   // Reverse velocity
        Destroy   // Destroy entity at boundary
    }

    public enum BoundaryType
    {
        Top, Bottom, Left, Right
    }

    public class BoundaryCollisionEvent : BaseEvent
    {
        public Entity Entity { get; }
        public BoundaryType Boundary { get; }

        public BoundaryCollisionEvent(Entity entity, BoundaryType boundary)
        {
            Entity = entity;
            Boundary = boundary;
        }
    }

    internal struct EntityBounds
    {
        public float Left, Right, Top, Bottom, Width, Height;
    }
}
