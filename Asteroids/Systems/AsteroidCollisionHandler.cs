using Arch.Core;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Asteroids.Components;
using Asteroids.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;

using Kobold.Extensions.Collision.Systems;
namespace Asteroids.Systems
{
    /// <summary>
    /// Handles collisions between bullets, asteroids, and the player ship
    /// </summary>
    public class AsteroidCollisionHandler : ISystem, IEventHandler<CollisionEvent>
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        public AsteroidCollisionHandler(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;

            // Subscribe to collision events from the core collision system
            _eventBus.Subscribe<CollisionEvent>(this);
        }

        public void Update(float deltaTime)
        {
            // This system is event-driven, so main update loop is empty
            // All logic happens in collision event handlers
        }

        public void Handle(CollisionEvent eventData)
        {
            Console.WriteLine($"AsteroidCollisionHandler received collision: {eventData.Entity1} vs {eventData.Entity2}");

            var entity1 = eventData.Entity1;
            var entity2 = eventData.Entity2;

            // Safety check - make sure both entities are still alive
            if (!_world.IsAlive(entity1) || !_world.IsAlive(entity2))
            {
                Console.WriteLine($"WARNING: Collision event with destroyed entity - Entity1 alive: {_world.IsAlive(entity1)}, Entity2 alive: {_world.IsAlive(entity2)}");
                return;
            }

            try
            {
                // Check for different collision types
                if (IsBulletAsteroidCollision(entity1, entity2, out var bullet, out var asteroid))
                {
                    Console.WriteLine($"Bullet-Asteroid collision: Bullet {bullet}, Asteroid {asteroid}");
                    HandleBulletAsteroidCollision(bullet, asteroid, eventData.CollisionPoint);
                }
                else if (IsShipAsteroidCollision(entity1, entity2, out var ship, out var asteroidHit))
                {
                    Console.WriteLine($"Ship-Asteroid collision: Ship {ship}, Asteroid {asteroidHit}");
                    HandleShipAsteroidCollision(ship, asteroidHit, eventData.CollisionPoint);
                }
                else
                {
                    Console.WriteLine("Collision type not recognized");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in collision handling: {ex.Message}");
                Console.WriteLine($"Entity1: {entity1}, Entity2: {entity2}");
                Console.WriteLine($"Entity1 alive: {_world.IsAlive(entity1)}, Entity2 alive: {_world.IsAlive(entity2)}");
                // Don't rethrow - just log and continue
            }
        }

        private bool IsBulletAsteroidCollision(Entity entity1, Entity entity2, out Entity bullet, out Entity asteroid)
        {
            bullet = Entity.Null;
            asteroid = Entity.Null;

            // Safety checks
            if (!_world.IsAlive(entity1) || !_world.IsAlive(entity2))
                return false;

            try
            {
                // Check entity1 = bullet, entity2 = asteroid
                if (_world.Has<Projectile>(entity1) && _world.Has<Asteroid>(entity2))
                {
                    bullet = entity1;
                    asteroid = entity2;
                    return true;
                }
                // Check entity2 = bullet, entity1 = asteroid
                else if (_world.Has<Projectile>(entity2) && _world.Has<Asteroid>(entity1))
                {
                    bullet = entity2;
                    asteroid = entity1;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR checking bullet-asteroid collision: {ex.Message}");
                Console.WriteLine($"Entity1: {entity1}, Entity2: {entity2}");
                return false;
            }

            return false;
        }

        private bool IsShipAsteroidCollision(Entity entity1, Entity entity2, out Entity ship, out Entity asteroid)
        {
            ship = Entity.Null;
            asteroid = Entity.Null;

            // Safety checks
            if (!_world.IsAlive(entity1) || !_world.IsAlive(entity2))
                return false;

            try
            {
                // Debug: Check what components each entity has
                bool entity1HasShip = _world.Has<Ship>(entity1);
                bool entity1HasAsteroid = _world.Has<Asteroid>(entity1);
                bool entity2HasShip = _world.Has<Ship>(entity2);
                bool entity2HasAsteroid = _world.Has<Asteroid>(entity2);

                Console.WriteLine($"Entity1 {entity1}: Ship={entity1HasShip}, Asteroid={entity1HasAsteroid}");
                Console.WriteLine($"Entity2 {entity2}: Ship={entity2HasShip}, Asteroid={entity2HasAsteroid}");

                // Check entity1 = ship, entity2 = asteroid
                if (entity1HasShip && entity2HasAsteroid)
                {
                    ship = entity1;
                    asteroid = entity2;
                    Console.WriteLine($"Found Ship-Asteroid collision: Ship={ship}, Asteroid={asteroid}");
                    return true;
                }
                // Check entity2 = ship, entity1 = asteroid
                else if (entity2HasShip && entity1HasAsteroid)
                {
                    ship = entity2;
                    asteroid = entity1;
                    Console.WriteLine($"Found Ship-Asteroid collision: Ship={ship}, Asteroid={asteroid}");
                    return true;
                }
                else
                {
                    Console.WriteLine("Not a ship-asteroid collision");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR checking ship-asteroid collision: {ex.Message}");
                Console.WriteLine($"Entity1: {entity1}, Entity2: {entity2}");
                return false;
            }

            return false;
        }

        private void HandleBulletAsteroidCollision(Entity bullet, Entity asteroid, Vector2 collisionPoint)
        {
            // Only handle player bullets hitting asteroids
            // (Enemy bullets would be handled separately if we had enemy ships)
            if (!IsPlayerBullet(bullet))
                return;

            // Publish bullet hit event (AsteroidSystem will handle the destruction)
            _eventBus.Publish(new BulletHitEvent(bullet, asteroid, collisionPoint));

            // Optional: Create explosion effect at collision point
            // _eventBus.Publish(new ExplosionRequestedEvent(collisionPoint, ExplosionType.Small));
        }

        private void HandleShipAsteroidCollision(Entity ship, Entity asteroid, Vector2 collisionPoint)
        {
            Console.WriteLine($"Ship-Asteroid collision detected: Ship {ship}, Asteroid {asteroid}");

            // Check if ship is invulnerable (recently respawned)
            if (_world.Has<Invulnerable>(ship))
            {
                Console.WriteLine("Ship is invulnerable, ignoring collision");
                return;
            }

            Console.WriteLine("Ship is vulnerable, processing collision");

            // Publish ship hit event
            _eventBus.Publish(new ShipHitEvent(ship, asteroid, collisionPoint));

            // Optional: Create explosion effect
            // _eventBus.Publish(new ExplosionRequestedEvent(collisionPoint, ExplosionType.Large));
        }

        private bool IsPlayerBullet(Entity bullet)
        {
            if (!_world.Has<CollisionLayerComponent>(bullet))
                return false;

            var layer = _world.Get<CollisionLayerComponent>(bullet);
            return layer.Layer == CollisionLayer.PlayerProjectile;
        }
    }

    /// <summary>
    /// Event fired when the player ship is hit by an asteroid
    /// </summary>
    public class ShipHitEvent : BaseEvent
    {
        public Entity Ship { get; }
        public Entity Asteroid { get; }
        public Vector2 HitPosition { get; }

        public ShipHitEvent(Entity ship, Entity asteroid, Vector2 hitPosition)
        {
            Ship = ship;
            Asteroid = asteroid;
            HitPosition = hitPosition;
        }
    }
}