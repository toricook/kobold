using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.Destruction.Components;
using Kobold.Extensions.Gameplay.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Extensions.Destruction.Systems
{
    /// <summary>
    /// Unified system that handles both lifetime expiration and pending destruction
    /// Processes all entity destruction at the end of the frame for safety
    /// </summary>
    public class DestructionSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly List<EntityDestructionInfo> _entitiesToDestroy = new();

        public DestructionSystem(World world, EventBus eventBus = null)
        {
            _world = world;
            _eventBus = eventBus;
        }

        public void Update(float deltaTime)
        {
            _entitiesToDestroy.Clear();

            // Process lifetime expiration
            ProcessLifetimeExpiration(deltaTime);

            // Process pending destructions
            ProcessPendingDestruction(deltaTime);

            // Actually destroy all entities at the end
            DestroyMarkedEntities();
        }

        private void ProcessLifetimeExpiration(float deltaTime)
        {
            var lifetimeQuery = new QueryDescription().WithAll<Lifetime>().WithNone<PendingDestruction>();

            _world.Query(in lifetimeQuery, (Entity entity, ref Lifetime lifetime) =>
            {
                lifetime.RemainingTime -= deltaTime;
                if (lifetime.RemainingTime <= 0)
                {
                    _entitiesToDestroy.Add(new EntityDestructionInfo
                    {
                        Entity = entity,
                        Reason = DestructionReason.Lifetime,
                        Position = GetEntityPosition(entity)
                    });
                }
            });
        }

        private void ProcessPendingDestruction(float deltaTime)
        {
            var destructionQuery = new QueryDescription().WithAll<PendingDestruction>();

            _world.Query(in destructionQuery, (Entity entity, ref PendingDestruction pending) =>
            {
                // Update delay timer
                pending.TimeRemaining -= deltaTime;

                // If delay has expired, mark for destruction
                if (pending.TimeRemaining <= 0f)
                {
                    _entitiesToDestroy.Add(new EntityDestructionInfo
                    {
                        Entity = entity,
                        Reason = pending.Reason,
                        Position = GetEntityPosition(entity)
                    });
                }
            });
        }

        private void DestroyMarkedEntities()
        {
            foreach (var destructionInfo in _entitiesToDestroy)
            {
                if (_world.IsAlive(destructionInfo.Entity))
                {
                    // Publish destruction event before destroying
                    _eventBus?.Publish(new EntityDestroyedEvent(
                        destructionInfo.Entity,
                        destructionInfo.Reason,
                        destructionInfo.Position
                    ));

                    _world.Destroy(destructionInfo.Entity);
                }
            }
        }

        private Vector2 GetEntityPosition(Entity entity)
        {
            if (_world.Has<Transform>(entity))
            {
                return _world.Get<Transform>(entity).Position;
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Mark an entity for destruction (safer than immediate destruction)
        /// </summary>
        public static void MarkForDestruction(World world, Entity entity, DestructionReason reason = DestructionReason.Manual, float delay = 0f)
        {
            if (!world.IsAlive(entity))
                return;

            if (world.Has<PendingDestruction>(entity))
            {
                // Already marked for destruction
                return;
            }

            world.Add(entity, new PendingDestruction(reason, delay));
        }

        /// <summary>
        /// Create an entity with a lifetime (convenience method)
        /// </summary>
        public static void SetLifetime(World world, Entity entity, float lifetime)
        {
            if (world.IsAlive(entity))
            {
                if (world.Has<Lifetime>(entity))
                {
                    // Update existing lifetime
                    ref var existingLifetime = ref world.Get<Lifetime>(entity);
                    existingLifetime.RemainingTime = lifetime;
                }
                else
                {
                    // Add new lifetime
                    world.Add(entity, new Lifetime(lifetime));
                }
            }
        }

        /// <summary>
        /// Get destruction statistics for debugging
        /// </summary>
        public DestructionStats GetStats()
        {
            int lifetimeCount = 0;
            int pendingCount = 0;

            var lifetimeQuery = new QueryDescription().WithAll<Lifetime>();
            _world.Query(in lifetimeQuery, (Entity entity) => lifetimeCount++);

            var pendingQuery = new QueryDescription().WithAll<PendingDestruction>();
            _world.Query(in pendingQuery, (Entity entity) => pendingCount++);

            return new DestructionStats
            {
                EntitiesWithLifetime = lifetimeCount,
                EntitiesPendingDestruction = pendingCount,
                EntitiesDestroyedThisFrame = _entitiesToDestroy.Count
            };
        }
    }

    /// <summary>
    /// Internal structure for tracking entity destruction
    /// </summary>
    internal struct EntityDestructionInfo
    {
        public Entity Entity;
        public DestructionReason Reason;
        public Vector2 Position;
    }

    /// <summary>
    /// Debug information about the destruction system
    /// </summary>
    public struct DestructionStats
    {
        public int EntitiesWithLifetime;
        public int EntitiesPendingDestruction;
        public int EntitiesDestroyedThisFrame;

        public override string ToString()
        {
            return $"Destruction Stats: {EntitiesWithLifetime} with lifetime, {EntitiesPendingDestruction} pending, {EntitiesDestroyedThisFrame} destroyed this frame";
        }
    }

    /// <summary>
    /// Enhanced destruction event with position information
    /// </summary>
    public class EntityDestroyedEvent : BaseEvent
    {
        public Entity Entity { get; }
        public DestructionReason Reason { get; }
        public Vector2 Position { get; }

        public EntityDestroyedEvent(Entity entity, DestructionReason reason, Vector2 position = default)
        {
            Entity = entity;
            Reason = reason;
            Position = position;
        }
    }
}