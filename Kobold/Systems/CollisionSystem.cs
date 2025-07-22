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

        private void ApplyCollisionResponse(Entity entity1, Entity entity2, Vector2 collisionNormal)
        {
            // Only apply response if both entities have velocity and physics
            if (!_world.Has<Velocity>(entity1) || !_world.Has<Velocity>(entity2))
                return;

            ref var velocity1 = ref _world.Get<Velocity>(entity1);
            ref var velocity2 = ref _world.Get<Velocity>(entity2);

            // Get masses (default to 1 if no physics component)
            float mass1 = _world.Has<Physics>(entity1) ? _world.Get<Physics>(entity1).Mass : 1f;
            float mass2 = _world.Has<Physics>(entity2) ? _world.Get<Physics>(entity2).Mass : 1f;

            // Skip collision response for static objects
            bool isStatic1 = _world.Has<Physics>(entity1) && _world.Get<Physics>(entity1).IsStatic;
            bool isStatic2 = _world.Has<Physics>(entity2) && _world.Get<Physics>(entity2).IsStatic;

            if (isStatic1 && isStatic2)
                return;

            // Get restitution (bounciness)
            float restitution1 = _world.Has<Physics>(entity1) ? _world.Get<Physics>(entity1).Restitution : 0.5f;
            float restitution2 = _world.Has<Physics>(entity2) ? _world.Get<Physics>(entity2).Restitution : 0.5f;
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
            var boxQuery = new QueryDescription().WithAll<Transform, BoxCollider>();
            _world.Query(in boxQuery, (Entity entity, ref Transform transform, ref BoxCollider collider) =>
            {
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
                    if (IsBoxBoxCollision(collider1, collider2, out var collisionPoint, out var collisionNormal))
                    {
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
                            ApplyCollisionResponse(collider1.Entity, collider2.Entity, collisionNormal);
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

        private bool IsBoxBoxCollision(ColliderInfo box1, ColliderInfo box2, out Vector2 collisionPoint, out Vector2 collisionNormal)
        {
            collisionPoint = Vector2.Zero;
            collisionNormal = Vector2.Zero;

            bool collision = box1.Position.X < box2.Position.X + box2.Size.X &&
                           box1.Position.X + box1.Size.X > box2.Position.X &&
                           box1.Position.Y < box2.Position.Y + box2.Size.Y &&
                           box1.Position.Y + box1.Size.Y > box2.Position.Y;

            if (collision)
            {
                // Calculate collision point (center of overlap)
                float overlapLeft = Math.Max(box1.Position.X, box2.Position.X);
                float overlapRight = Math.Min(box1.Position.X + box1.Size.X, box2.Position.X + box2.Size.X);
                float overlapTop = Math.Max(box1.Position.Y, box2.Position.Y);
                float overlapBottom = Math.Min(box1.Position.Y + box1.Size.Y, box2.Position.Y + box2.Size.Y);

                collisionPoint = new Vector2((overlapLeft + overlapRight) / 2, (overlapTop + overlapBottom) / 2);

                // Calculate collision normal (from box1 to box2)
                Vector2 center1 = box1.Position + box1.Size / 2;
                Vector2 center2 = box2.Position + box2.Size / 2;
                Vector2 direction = center2 - center1;

                if (direction.LengthSquared() > 0)
                {
                    collisionNormal = Vector2.Normalize(direction);
                }
                else
                {
                    collisionNormal = Vector2.UnitX; // Default normal
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