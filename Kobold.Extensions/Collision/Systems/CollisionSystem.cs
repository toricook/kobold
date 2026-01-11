using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.Collision.Components;
using PhysicsComponent = Kobold.Extensions.Physics.Components.Physics;
using Kobold.Extensions.Physics.Components;
using Kobold.Extensions.Destruction.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Extensions.Collision.Systems
{
    /// <summary>
    /// Enhanced collision system with filtering, layers, and circle collision support
    /// </summary>
    public class CollisionSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly CollisionConfig _config;

        public CollisionSystem(World world, EventBus eventBus, CollisionConfig config = null)
        {
            _world = world;
            _eventBus = eventBus;
            _config = config ?? new CollisionConfig();
        }

        private void ApplyCollisionResponse(Entity entity1, Entity entity2, Vector2 collisionNormal, float penetrationDepth)
        {
            // Get physics properties
            bool isStatic1 = _world.Has<PhysicsComponent>(entity1) && _world.Get<PhysicsComponent>(entity1).IsStatic;
            bool isStatic2 = _world.Has<PhysicsComponent>(entity2) && _world.Get<PhysicsComponent>(entity2).IsStatic;

            if (isStatic1 && isStatic2)
                return;

            // Get masses (default to 1 if no physics component)
            float mass1 = _world.Has<PhysicsComponent>(entity1) ? _world.Get<PhysicsComponent>(entity1).Mass : 1f;
            float mass2 = _world.Has<PhysicsComponent>(entity2) ? _world.Get<PhysicsComponent>(entity2).Mass : 1f;

            // POSITION CORRECTION: Separate the entities to resolve overlap
            if (_world.Has<Transform>(entity1) && _world.Has<Transform>(entity2) && penetrationDepth > 0)
            {
                ref var transform1 = ref _world.Get<Transform>(entity1);
                ref var transform2 = ref _world.Get<Transform>(entity2);

                // Calculate separation based on mass ratio
                float totalMass = mass1 + mass2;
                float percent = 0.8f; // Penetration percentage to correct (80% prevents jitter)
                float slop = 0.01f; // Small allowable penetration (prevents jitter from tiny overlaps)

                float correctionMagnitude = Math.Max(penetrationDepth - slop, 0.0f) * percent;
                Vector2 correction = correctionMagnitude * collisionNormal;

                // Apply position correction based on mass
                if (!isStatic1 && !isStatic2)
                {
                    // Both dynamic - split correction based on mass ratio
                    transform1.Position -= correction * (mass2 / totalMass);
                    transform2.Position += correction * (mass1 / totalMass);
                }
                else if (!isStatic1)
                {
                    // Only entity1 is dynamic
                    transform1.Position -= correction;
                }
                else if (!isStatic2)
                {
                    // Only entity2 is dynamic
                    transform2.Position += correction;
                }
            }

            // VELOCITY RESPONSE: Apply impulses if both have velocity
            if (!_world.Has<Velocity>(entity1) || !_world.Has<Velocity>(entity2))
                return;

            ref var velocity1 = ref _world.Get<Velocity>(entity1);
            ref var velocity2 = ref _world.Get<Velocity>(entity2);

            // Get restitution (bounciness)
            float restitution1 = _world.Has<PhysicsComponent>(entity1) ? _world.Get<PhysicsComponent>(entity1).Restitution : 0.0f;
            float restitution2 = _world.Has<PhysicsComponent>(entity2) ? _world.Get<PhysicsComponent>(entity2).Restitution : 0.0f;
            float combinedRestitution = (restitution1 + restitution2) / 2f;

            // Calculate relative velocity
            Vector2 relativeVelocity = velocity2.Value - velocity1.Value;
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, collisionNormal);

            // Don't resolve if velocities are separating
            if (velocityAlongNormal > 0)
                return;

            // Calculate impulse magnitude
            float impulseMagnitude = -(1 + combinedRestitution) * velocityAlongNormal;
            impulseMagnitude /= (1 / mass1) + (1 / mass2);

            Vector2 impulse = impulseMagnitude * collisionNormal;

            // Apply impulse
            if (!isStatic1)
            {
                velocity1.Value -= impulse / mass1;
            }
            if (!isStatic2)
            {
                velocity2.Value += impulse / mass2;
            }
        }

        public void Update(float deltaTime)
        {
            // Collect all colliders with position and layer information
            var colliders = new List<ColliderInfo>();

            // Collect box colliders
            var boxQuery = new QueryDescription().WithAll<Transform, BoxCollider>().WithNone<PendingDestruction>(); // Ignore entities marked for destruction
            _world.Query(in boxQuery, (Entity entity, ref Transform transform, ref BoxCollider collider) =>
            {
                // Safety check - make sure entity is still alive
                if (!_world.IsAlive(entity))
                    return;

                var layer = GetCollisionLayer(entity);
                colliders.Add(new ColliderInfo
                {
                    Entity = entity,
                    Position = transform.Position + collider.Offset,
                    Size = collider.Size,
                    Layer = layer
                });
            });

            // Check all pairs for collisions
            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = i + 1; j < colliders.Count; j++)
                {
                    var collider1 = colliders[i];
                    var collider2 = colliders[j];

                    // Check if these layers can collide
                    if (!CanLayersCollide(collider1.Layer, collider2.Layer))
                        continue;

                    // Check for box-box collision
                    if (IsBoxBoxCollision(collider1, collider2, out var collisionPoint, out var collisionNormal, out var penetrationDepth))
                    {
                        // Double-check entities are still alive before publishing event
                        if (!_world.IsAlive(collider1.Entity) || !_world.IsAlive(collider2.Entity))
                        {
                            continue; // Skip this collision
                        }

                        // Create detailed collision event
                        var collisionEvent = new CollisionEvent(
                            collider1.Entity,
                            collider2.Entity,
                            collisionPoint,
                            collisionNormal,
                            collider1.Layer,
                            collider2.Layer
                        );

                        _eventBus.Publish(collisionEvent);

                        // Apply collision response if enabled
                        if (_config.EnableCollisionResponse)
                        {
                            ApplyCollisionResponse(collider1.Entity, collider2.Entity, collisionNormal, penetrationDepth);
                        }
                    }
                }
            }
        }

        private CollisionLayer GetCollisionLayer(Entity entity)
        {
            if (_world.Has<CollisionLayerComponent>(entity))
            {
                return _world.Get<CollisionLayerComponent>(entity).Layer;
            }

            // Auto-detect layer based on components
            if (_world.Has<Player>(entity))
                return CollisionLayer.Player;
            if (_world.Has<Enemy>(entity))
                return CollisionLayer.Enemy;
            if (_world.Has<Projectile>(entity))
                return CollisionLayer.Projectile;
            if (_world.Has<Static>(entity))
                return CollisionLayer.Environment;

            return CollisionLayer.Default;
        }

        private bool CanLayersCollide(CollisionLayer layer1, CollisionLayer layer2)
        {
            return _config.CollisionMatrix.CanCollide(layer1, layer2);
        }

        private bool IsBoxBoxCollision(ColliderInfo box1, ColliderInfo box2, out Vector2 collisionPoint, out Vector2 collisionNormal, out float penetrationDepth)
        {
            collisionPoint = Vector2.Zero;
            collisionNormal = Vector2.Zero;
            penetrationDepth = 0f;

            bool collision = box1.Position.X < box2.Position.X + box2.Size.X &&
                           box1.Position.X + box1.Size.X > box2.Position.X &&
                           box1.Position.Y < box2.Position.Y + box2.Size.Y &&
                           box1.Position.Y + box1.Size.Y > box2.Position.Y;

            if (collision)
            {
                // Calculate overlap amounts
                float overlapLeft = Math.Max(box1.Position.X, box2.Position.X);
                float overlapRight = Math.Min(box1.Position.X + box1.Size.X, box2.Position.X + box2.Size.X);
                float overlapTop = Math.Max(box1.Position.Y, box2.Position.Y);
                float overlapBottom = Math.Min(box1.Position.Y + box1.Size.Y, box2.Position.Y + box2.Size.Y);

                float overlapX = overlapRight - overlapLeft;
                float overlapY = overlapBottom - overlapTop;

                collisionPoint = new Vector2((overlapLeft + overlapRight) / 2, (overlapTop + overlapBottom) / 2);

                // Calculate collision normal based on minimum penetration axis
                Vector2 center1 = box1.Position + box1.Size / 2;
                Vector2 center2 = box2.Position + box2.Size / 2;
                Vector2 direction = center2 - center1;

                // Use the axis with minimum overlap as the collision normal
                if (overlapX < overlapY)
                {
                    // Horizontal collision
                    penetrationDepth = overlapX;
                    collisionNormal = direction.X > 0 ? Vector2.UnitX : -Vector2.UnitX;
                }
                else
                {
                    // Vertical collision
                    penetrationDepth = overlapY;
                    collisionNormal = direction.Y > 0 ? Vector2.UnitY : -Vector2.UnitY;
                }
            }

            return collision;
        }

        /// <summary>
        /// Check if two specific entities are colliding
        /// </summary>
        public bool AreEntitiesColliding(Entity entity1, Entity entity2)
        {
            if (!_world.Has<Transform>(entity1) || !_world.Has<Transform>(entity2))
                return false;

            if (!_world.Has<BoxCollider>(entity1) || !_world.Has<BoxCollider>(entity2))
                return false;

            var transform1 = _world.Get<Transform>(entity1);
            var transform2 = _world.Get<Transform>(entity2);
            var box1 = _world.Get<BoxCollider>(entity1);
            var box2 = _world.Get<BoxCollider>(entity2);

            var pos1 = transform1.Position + box1.Offset;
            var pos2 = transform2.Position + box2.Offset;

            return pos1.X < pos2.X + box2.Size.X &&
                   pos1.X + box1.Size.X > pos2.X &&
                   pos1.Y < pos2.Y + box2.Size.Y &&
                   pos1.Y + box1.Size.Y > pos2.Y;
        }
    }

    /// <summary>
    /// Configuration for collision detection and response
    /// </summary>
    public class CollisionConfig
    {
        public bool EnableCollisionResponse { get; set; } = false; // Usually handled by game-specific systems
        public CollisionMatrix CollisionMatrix { get; set; } = new CollisionMatrix();
    }

    /// <summary>
    /// Component to assign collision layers to entities
    /// </summary>
    public struct CollisionLayerComponent
    {
        public CollisionLayer Layer;

        public CollisionLayerComponent(CollisionLayer layer)
        {
            Layer = layer;
        }
    }

    /// <summary>
    /// Collision layers for filtering
    /// </summary>
    public enum CollisionLayer
    {
        Default = 0,
        Player = 1,
        Enemy = 2,
        Projectile = 3,
        Environment = 4,
        PlayerProjectile = 5,
        EnemyProjectile = 6,
        Pickup = 7,
        Trigger = 8
    }

    /// <summary>
    /// Matrix defining which layers can collide with each other
    /// </summary>
    public class CollisionMatrix
    {
        private readonly bool[,] _matrix;
        private const int LayerCount = 9; // Update if adding more layers

        public CollisionMatrix()
        {
            _matrix = new bool[LayerCount, LayerCount];
            SetupDefaultCollisions();
        }

        private void SetupDefaultCollisions()
        {
            // Default collides with everything except projectiles
            SetCollision(CollisionLayer.Default, CollisionLayer.Default, true);
            SetCollision(CollisionLayer.Default, CollisionLayer.Player, true);
            SetCollision(CollisionLayer.Default, CollisionLayer.Enemy, true);
            SetCollision(CollisionLayer.Default, CollisionLayer.Environment, true);

            // Player collides with enemies, environment, enemy projectiles, and pickups
            SetCollision(CollisionLayer.Player, CollisionLayer.Enemy, true);
            SetCollision(CollisionLayer.Player, CollisionLayer.Environment, true);
            SetCollision(CollisionLayer.Player, CollisionLayer.EnemyProjectile, true);
            SetCollision(CollisionLayer.Player, CollisionLayer.Pickup, true);
            SetCollision(CollisionLayer.Player, CollisionLayer.Trigger, true);

            // Enemies collide with player, environment, and player projectiles
            SetCollision(CollisionLayer.Enemy, CollisionLayer.Environment, true);
            SetCollision(CollisionLayer.Enemy, CollisionLayer.PlayerProjectile, true);

            // Player projectiles collide with enemies and environment
            SetCollision(CollisionLayer.PlayerProjectile, CollisionLayer.Enemy, true);
            SetCollision(CollisionLayer.PlayerProjectile, CollisionLayer.Environment, true);

            // Enemy projectiles collide with player and environment
            SetCollision(CollisionLayer.EnemyProjectile, CollisionLayer.Player, true);
            SetCollision(CollisionLayer.EnemyProjectile, CollisionLayer.Environment, true);

            // Environment collides with everything physical
            SetCollision(CollisionLayer.Environment, CollisionLayer.Projectile, true);

            // Pickups only collide with player
            SetCollision(CollisionLayer.Pickup, CollisionLayer.Player, true);

            // Triggers can collide with player and enemies
            SetCollision(CollisionLayer.Trigger, CollisionLayer.Player, true);
            SetCollision(CollisionLayer.Trigger, CollisionLayer.Enemy, true);
        }

        public void SetCollision(CollisionLayer layer1, CollisionLayer layer2, bool canCollide)
        {
            _matrix[(int)layer1, (int)layer2] = canCollide;
            _matrix[(int)layer2, (int)layer1] = canCollide; // Symmetric
        }

        public bool CanCollide(CollisionLayer layer1, CollisionLayer layer2)
        {
            return _matrix[(int)layer1, (int)layer2];
        }
    }

    /// <summary>
    /// Enhanced collision event with detailed information
    /// </summary>
    public class CollisionEvent : BaseEvent
    {
        public Entity Entity1 { get; }
        public Entity Entity2 { get; }
        public Vector2 CollisionPoint { get; }
        public Vector2 CollisionNormal { get; }
        public CollisionLayer Layer1 { get; }
        public CollisionLayer Layer2 { get; }

        public CollisionEvent(Entity entity1, Entity entity2, Vector2 collisionPoint, Vector2 collisionNormal,
            CollisionLayer layer1, CollisionLayer layer2)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            CollisionPoint = collisionPoint;
            CollisionNormal = collisionNormal;
            Layer1 = layer1;
            Layer2 = layer2;
        }

        // Legacy constructor for backward compatibility
        public CollisionEvent(Entity entity1, Entity entity2) : this(entity1, entity2, Vector2.Zero, Vector2.Zero, CollisionLayer.Default, CollisionLayer.Default)
        {
        }
    }

    /// <summary>
    /// Internal structure for collision detection (simplified for box-only)
    /// </summary>
    internal struct ColliderInfo
    {
        public Entity Entity;
        public Vector2 Position;
        public Vector2 Size;
        public CollisionLayer Layer;
    }
}