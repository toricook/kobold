using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Asteroids.Components;
using Asteroids.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components.Gameplay;

namespace Asteroids.Systems
{
    /// <summary>
    /// Handles weapon firing, bullet creation, and bullet lifetime management
    /// </summary>
    public class WeaponSystem : ISystem
    {
        private readonly World _world;
        private readonly IInputManager _inputManager;
        private readonly EventBus _eventBus;
        private readonly WeaponConfig _config;

        public WeaponSystem(World world, IInputManager inputManager, EventBus eventBus, WeaponConfig config = null)
        {
            _world = world;
            _inputManager = inputManager;
            _eventBus = eventBus;
            _config = config ?? new WeaponConfig();
        }

        public void Update(float deltaTime)
        {
            // Handle firing input and create bullets
            HandleWeaponFiring(deltaTime);

            // Update bullet lifetimes and destroy expired bullets
            UpdateBulletLifetimes(deltaTime);
        }

        private void HandleWeaponFiring(float deltaTime)
        {
            // Find entities that can fire weapons (ships with Weapon component)
            var weaponQuery = new QueryDescription().WithAll<Transform, Weapon>();

            _world.Query(in weaponQuery, (Entity entity, ref Transform transform, ref Weapon weapon) =>
            {
                // Update fire rate timing
                weapon.LastFired += deltaTime;

                // Check if fire button is pressed and weapon is ready to fire
                bool firePressed = _inputManager.IsKeyPressed(_config.FireKey) || _inputManager.IsKeyPressed(_config.AlternateFireKey);
                bool canFire = weapon.LastFired >= (1f / weapon.FireRate);

                if (firePressed && canFire)
                {
                    FireBullet(entity, transform, weapon);
                    weapon.LastFired = 0f; // Reset fire timer
                }
            });
        }

        private void FireBullet(Entity shooter, Transform shooterTransform, Weapon weapon)
        {
            // Calculate bullet spawn position (at the tip of the ship)
            Vector2 bulletOffset = CalculateBulletSpawnOffset(shooterTransform.Rotation);
            Vector2 bulletPosition = shooterTransform.Position + bulletOffset;

            // Calculate bullet velocity (weapon direction + shooter velocity for momentum)
            Vector2 bulletDirection = new Vector2(
                MathF.Cos(shooterTransform.Rotation),
                MathF.Sin(shooterTransform.Rotation)
            );

            Vector2 bulletVelocity = bulletDirection * weapon.BulletSpeed;

            // Add shooter's velocity for realistic momentum
            if (_world.Has<Velocity>(shooter))
            {
                var shooterVelocity = _world.Get<Velocity>(shooter);
                bulletVelocity += shooterVelocity.Value;
            }

            // Create bullet entity
            var bullet = _world.Create(
                new Transform(bulletPosition),
                new Velocity(bulletVelocity),
                new Lifetime(weapon.BulletLifetime),
                new BoxCollider(new Vector2(_config.BulletSize, _config.BulletSize)),
                RectangleRenderer.GameObject(new Vector2(_config.BulletSize, _config.BulletSize), _config.BulletColor),
                new CustomBoundaryBehavior(BoundaryBehavior.Destroy), // Bullets are destroyed at screen edges
                GetBulletCollisionLayer(shooter),
                new Projectile() // Tag for identification
            );

            // Publish weapon fired event
            _eventBus.Publish(new WeaponFiredEvent(shooter, bulletPosition, bulletDirection, bullet));
        }

        private Vector2 CalculateBulletSpawnOffset(float rotation)
        {
            // Spawn bullet at the tip of the ship (slightly forward)
            float spawnDistance = _config.BulletSpawnDistance;
            return new Vector2(
                MathF.Cos(rotation) * spawnDistance,
                MathF.Sin(rotation) * spawnDistance
            );
        }

        private CollisionLayerComponent GetBulletCollisionLayer(Entity shooter)
        {
            // Determine bullet collision layer based on who fired it
            if (_world.Has<Player>(shooter))
            {
                return new CollisionLayerComponent(CollisionLayer.PlayerProjectile);
            }
            else if (_world.Has<Enemy>(shooter))
            {
                return new CollisionLayerComponent(CollisionLayer.EnemyProjectile);
            }
            else
            {
                return new CollisionLayerComponent(CollisionLayer.Projectile);
            }
        }

        private void UpdateBulletLifetimes(float deltaTime)
        {
            // This is handled by the LifetimeSystem, but we could add bullet-specific logic here
            // For example, fading bullets as they age, or special effects

            var bulletQuery = new QueryDescription().WithAll<Projectile, Lifetime, RectangleRenderer>();
            _world.Query(in bulletQuery, (Entity entity, ref Projectile projectile, ref Lifetime lifetime, ref RectangleRenderer renderer) =>
            {
                // Optional: Fade bullets as they age
                if (_config.FadeBullets)
                {
                    float ageRatio = 1f - (lifetime.RemainingTime / _config.DefaultBulletLifetime);
                    int alpha = (int)(255 * (1f - ageRatio * 0.5f)); // Fade to 50% transparency

                    var color = renderer.Color;
                    renderer.Color = Color.FromArgb(alpha, color.R, color.G, color.B);
                }
            });
        }

        /// <summary>
        /// Manually create a bullet (useful for testing or special weapons)
        /// </summary>
        public Entity CreateBullet(Vector2 position, Vector2 velocity, float lifetime, CollisionLayer layer)
        {
            return _world.Create(
                new Transform(position),
                new Velocity(velocity),
                new Lifetime(lifetime),
                new BoxCollider(new Vector2(_config.BulletSize, _config.BulletSize)),
                RectangleRenderer.GameObject(new Vector2(_config.BulletSize, _config.BulletSize), _config.BulletColor),
                new CustomBoundaryBehavior(BoundaryBehavior.Destroy),
                new CollisionLayerComponent(layer),
                new Projectile()
            );
        }
    }

    /// <summary>
    /// Configuration for weapon system behavior
    /// </summary>
    public class WeaponConfig
    {
        // Input configuration
        public KeyCode FireKey { get; set; } = KeyCode.Space;
        public KeyCode AlternateFireKey { get; set; } = KeyCode.Enter;

        // Bullet appearance
        public float BulletSize { get; set; } = 4f;
        public Color BulletColor { get; set; } = Color.Yellow;
        public bool FadeBullets { get; set; } = false;

        // Bullet physics
        public float BulletSpawnDistance { get; set; } = 15f; // How far from shooter center to spawn bullet
        public float DefaultBulletSpeed { get; set; } = 400f;
        public float DefaultBulletLifetime { get; set; } = 2.5f; // seconds

        // Weapon behavior
        public float DefaultFireRate { get; set; } = 6f; // bullets per second
        public int MaxBulletsOnScreen { get; set; } = 20; // Optional limit (not implemented yet)

        // Audio (for future use)
        public string FireSoundName { get; set; } = "laser_fire";
    }
}