using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Kobold.Core;
using Asteroids.Components;
using Asteroids.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids.Systems
{
    /// <summary>
    /// Manages asteroid spawning, movement, destruction, and wave progression
    /// </summary>
    public class AsteroidSystem : ISystem, IEventHandler<BulletHitEvent>, IEventHandler<GameRestartEvent>
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly AsteroidConfig _config;
        private readonly float _screenWidth;
        private readonly float _screenHeight;

        private int _currentWave = 1;
        private List<Entity> _activeAsteroids = new List<Entity>();

        public AsteroidSystem(World world, EventBus eventBus, float screenWidth, float screenHeight, AsteroidConfig config = null)
        {
            _world = world;
            _eventBus = eventBus;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _config = config ?? new AsteroidConfig();

            // Subscribe to events
            _eventBus.Subscribe<BulletHitEvent>(this);
            _eventBus.Subscribe<GameRestartEvent>(this);
        }

        public void Update(float deltaTime)
        {
            // Update asteroid rotation (spinning effect)
            UpdateAsteroidRotation(deltaTime);

            // Check if all asteroids are destroyed and spawn new wave
            CheckForWaveCompletion();

            // Clean up destroyed asteroids from our tracking list
            CleanupDestroyedAsteroids();
        }

        private void UpdateAsteroidRotation(float deltaTime)
        {
            var asteroidQuery = new QueryDescription().WithAll<Asteroid, AngularVelocity>();
            _world.Query(in asteroidQuery, (ref Asteroid asteroid, ref AngularVelocity angularVelocity) =>
            {
                // Asteroids spin at their defined rotation speed
                // This is already handled by the physics system, but we could add variation here
            });
        }

        private void CheckForWaveCompletion()
        {
            // Count remaining asteroids
            int asteroidCount = 0;
            var asteroidQuery = new QueryDescription().WithAll<Asteroid>();
            _world.Query(in asteroidQuery, (Entity entity) => asteroidCount++);

            // If no asteroids remain, spawn new wave
            if (asteroidCount == 0)
            {
                SpawnWave(_currentWave);
                _currentWave++;
            }
        }

        private void CleanupDestroyedAsteroids()
        {
            _activeAsteroids.RemoveAll(asteroid => !_world.IsAlive(asteroid));
        }

        /// <summary>
        /// Spawn a wave of asteroids
        /// </summary>
        public void SpawnWave(int waveNumber)
        {
            int asteroidCount = _config.BaseAsteroidCount + (waveNumber - 1) * _config.AsteroidsPerWave;

            _eventBus.Publish(new WaveStartedEvent(waveNumber, asteroidCount));

            for (int i = 0; i < asteroidCount; i++)
            {
                SpawnAsteroid(AsteroidSize.Large, GetSafeSpawnPosition());
            }
        }

        /// <summary>
        /// Spawn a single asteroid at a specific position
        /// </summary>
        public Entity SpawnAsteroid(AsteroidSize size, Vector2 position, Vector2? velocity = null)
        {
            var asteroidData = _config.GetAsteroidData(size);

            // Generate random velocity if not provided
            Vector2 asteroidVelocity = velocity ?? GenerateRandomVelocity(asteroidData.Speed);

            // Generate random rotation speed
            float rotationSpeed = MathUtils.RandomRange(-asteroidData.MaxRotationSpeed, asteroidData.MaxRotationSpeed);

            var asteroid = _world.Create(
                new Transform(position),
                new Velocity(asteroidVelocity),
                new AngularVelocity(rotationSpeed),
                new Asteroid(size, asteroidData.ScoreValue, rotationSpeed),
                new BoxCollider(new Vector2(asteroidData.Size, asteroidData.Size)),
                RectangleRenderer.GameObject(new Vector2(asteroidData.Size, asteroidData.Size), asteroidData.Color),
                new CustomBoundaryBehavior(BoundaryBehavior.Wrap), // Asteroids wrap around screen
                new CollisionLayerComponent(CollisionLayer.Enemy),
                new Enemy() // Tag for identification
            );

            _activeAsteroids.Add(asteroid);
            _eventBus.Publish(new AsteroidSpawnedEvent(asteroid, size, position));

            return asteroid;
        }

        /// <summary>
        /// Handle bullet hitting asteroid
        /// </summary>
        public void Handle(BulletHitEvent eventData)
        {
            // Safety check - make sure entities are still alive
            if (!_world.IsAlive(eventData.Target) || !_world.IsAlive(eventData.Bullet))
            {
                Console.WriteLine($"WARNING: BulletHitEvent with destroyed entities - Target alive: {_world.IsAlive(eventData.Target)}, Bullet alive: {_world.IsAlive(eventData.Bullet)}");
                return;
            }

            // Check if the target is an asteroid
            if (!_world.Has<Asteroid>(eventData.Target))
            {
                Console.WriteLine($"WARNING: BulletHitEvent target is not an asteroid");
                return;
            }

            var asteroid = _world.Get<Asteroid>(eventData.Target);
            var asteroidTransform = _world.Get<Transform>(eventData.Target);

            // Publish asteroid destroyed event
            _eventBus.Publish(new AsteroidDestroyedEvent(
                eventData.Target,
                asteroid.Size,
                asteroidTransform.Position,
                asteroid.ScoreValue
            ));

            // Destroy the bullet first (safer to destroy smaller entity first)
            if (_world.IsAlive(eventData.Bullet))
            {
                DestructionSystem.MarkForDestruction(_world, eventData.Bullet, DestructionReason.Collision);
            }

            // Split asteroid if it's large enough (do this before destroying the original)
            if (asteroid.Size != AsteroidSize.Small)
            {
                SplitAsteroid(eventData.Target, asteroidTransform.Position);
            }

            // Destroy the asteroid last
            if (_world.IsAlive(eventData.Target))
            {
                DestructionSystem.MarkForDestruction(_world, eventData.Target, DestructionReason.Collision);
            }
        }

        /// <summary>
        /// Handle game restart
        /// </summary>
        public void Handle(GameRestartEvent eventData)
        {
            // Clear all asteroids
            var asteroidQuery = new QueryDescription().WithAll<Asteroid>();
            _world.Query(in asteroidQuery, (Entity entity) =>
            {
                _world.Destroy(entity);
            });

            _activeAsteroids.Clear();
            _currentWave = 1;

            // Spawn initial wave
            SpawnWave(_currentWave);
        }

        private void SplitAsteroid(Entity originalAsteroid, Vector2 position)
        {
            var asteroid = _world.Get<Asteroid>(originalAsteroid);
            var originalVelocity = _world.Has<Velocity>(originalAsteroid) ?
                _world.Get<Velocity>(originalAsteroid).Value : Vector2.Zero;

            AsteroidSize newSize = asteroid.Size switch
            {
                AsteroidSize.Large => AsteroidSize.Medium,
                AsteroidSize.Medium => AsteroidSize.Small,
                _ => AsteroidSize.Small // Shouldn't happen
            };

            var newAsteroidData = _config.GetAsteroidData(newSize);

            // Create 2-3 smaller asteroids
            int splitCount = _config.SplitCount;
            for (int i = 0; i < splitCount; i++)
            {
                // Generate velocities in different directions
                float angle = (MathF.PI * 2 / splitCount) * i + MathUtils.RandomRange(-0.5f, 0.5f);
                Vector2 direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                Vector2 splitVelocity = originalVelocity + direction * newAsteroidData.Speed * _config.SplitSpeedMultiplier;

                // Spawn new asteroid slightly offset from original position
                Vector2 spawnOffset = direction * _config.SplitSpawnDistance;
                SpawnAsteroid(newSize, position + spawnOffset, splitVelocity);
            }
        }

        private Vector2 GetSafeSpawnPosition()
        {
            Vector2 position;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                // Spawn at screen edges
                if (Random.Shared.Next(2) == 0)
                {
                    // Spawn on left or right edge
                    float x = Random.Shared.Next(2) == 0 ? -50f : _screenWidth + 50f;
                    float y = Random.Shared.NextSingle() * _screenHeight;
                    position = new Vector2(x, y);
                }
                else
                {
                    // Spawn on top or bottom edge
                    float x = Random.Shared.NextSingle() * _screenWidth;
                    float y = Random.Shared.Next(2) == 0 ? -50f : _screenHeight + 50f;
                    position = new Vector2(x, y);
                }
                attempts++;
            }
            while (IsTooCloseToPlayer(position) && attempts < maxAttempts);

            return position;
        }

        private bool IsTooCloseToPlayer(Vector2 position)
        {
            const float safeDistance = 150f; // Minimum distance from player

            var playerQuery = new QueryDescription().WithAll<Player, Transform>();
            bool tooClose = false;

            _world.Query(in playerQuery, (ref Transform playerTransform) =>
            {
                float distance = Vector2.Distance(position, playerTransform.Position);
                if (distance < safeDistance)
                {
                    tooClose = true;
                }
            });

            return tooClose;
        }

        private Vector2 GenerateRandomVelocity(float speed)
        {
            float angle = Random.Shared.NextSingle() * MathF.PI * 2;
            float actualSpeed = speed * MathUtils.RandomRange(0.7f, 1.3f); // Add some variation

            return new Vector2(
                MathF.Cos(angle) * actualSpeed,
                MathF.Sin(angle) * actualSpeed
            );
        }

        /// <summary>
        /// Get count of asteroids by size
        /// </summary>
        public Dictionary<AsteroidSize, int> GetAsteroidCounts()
        {
            var counts = new Dictionary<AsteroidSize, int>
            {
                { AsteroidSize.Large, 0 },
                { AsteroidSize.Medium, 0 },
                { AsteroidSize.Small, 0 }
            };

            var asteroidQuery = new QueryDescription().WithAll<Asteroid>();
            _world.Query(in asteroidQuery, (ref Asteroid asteroid) =>
            {
                counts[asteroid.Size]++;
            });

            return counts;
        }

        /// <summary>
        /// Get total asteroid count
        /// </summary>
        public int GetTotalAsteroidCount()
        {
            int count = 0;
            var asteroidQuery = new QueryDescription().WithAll<Asteroid>();
            _world.Query(in asteroidQuery, (Entity entity) => count++);
            return count;
        }
    }

    /// <summary>
    /// Configuration for asteroid behavior and appearance
    /// </summary>
    public class AsteroidConfig
    {
        // Wave configuration
        public int BaseAsteroidCount { get; set; } = 4; // Asteroids in wave 1
        public int AsteroidsPerWave { get; set; } = 1; // Additional asteroids each wave

        // Splitting behavior
        public int SplitCount { get; set; } = 2; // How many pieces when destroyed
        public float SplitSpeedMultiplier { get; set; } = 1.2f; // Speed boost for split pieces
        public float SplitSpawnDistance { get; set; } = 20f; // How far apart to spawn pieces

        // Asteroid data by size
        private readonly Dictionary<AsteroidSize, AsteroidData> _asteroidData = new()
        {
            [AsteroidSize.Large] = new AsteroidData
            {
                Size = 60f,
                Speed = 50f,
                MaxRotationSpeed = 2f,
                ScoreValue = 20,
                Color = Color.Gray
            },
            [AsteroidSize.Medium] = new AsteroidData
            {
                Size = 35f,
                Speed = 75f,
                MaxRotationSpeed = 3f,
                ScoreValue = 50,
                Color = Color.LightGray
            },
            [AsteroidSize.Small] = new AsteroidData
            {
                Size = 20f,
                Speed = 100f,
                MaxRotationSpeed = 4f,
                ScoreValue = 100,
                Color = Color.White
            }
        };

        public AsteroidData GetAsteroidData(AsteroidSize size) => _asteroidData[size];
    }

    /// <summary>
    /// Data for different asteroid sizes
    /// </summary>
    public struct AsteroidData
    {
        public float Size;
        public float Speed;
        public float MaxRotationSpeed;
        public int ScoreValue;
        public Color Color;
    }
}