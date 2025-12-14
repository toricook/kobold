using Arch.Core;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Asteroids.Components;
using Asteroids.Events;
using Asteroids.Systems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Kobold.Core.Abstractions;
using Kobold.Core.Components.Gameplay;

namespace Asteroids.Systems
{
    /// <summary>
    /// Manages player lives, ship destruction, respawning, and game over conditions
    /// </summary>
    public class LivesSystem : ISystem, IEventHandler<ShipHitEvent>, IEventHandler<GameRestartEvent>
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly LivesConfig _config;
        private readonly float _screenWidth;
        private readonly float _screenHeight;

        private int _currentLives;
        private Entity _livesDisplayEntity = Entity.Null;
        private Entity _playerShipEntity = Entity.Null;
        private float _respawnTimer = 0f;
        private bool _isRespawning = false;

        public LivesSystem(World world, EventBus eventBus, float screenWidth, float screenHeight, LivesConfig config = null)
        {
            _world = world;
            _eventBus = eventBus;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _config = config ?? new LivesConfig();

            _currentLives = _config.StartingLives;

            // Subscribe to events
            _eventBus.Subscribe<ShipHitEvent>(this);
            _eventBus.Subscribe<GameRestartEvent>(this);
        }

        public void Update(float deltaTime)
        {
            // Handle respawn timer
            if (_isRespawning)
            {
                _respawnTimer -= deltaTime;
                if (_respawnTimer <= 0f)
                {
                    RespawnPlayer();
                }
            }

            // Update invulnerability
            UpdateInvulnerability(deltaTime);

            // Find player ship if we don't have a reference
            if (_playerShipEntity == Entity.Null || !_world.IsAlive(_playerShipEntity))
            {
                FindPlayerShip();
            }
        }

        public void Handle(ShipHitEvent eventData)
        {
            Console.WriteLine($"LivesSystem received ShipHitEvent: Ship {eventData.Ship}");

            // Ignore hits during invulnerability
            if (_world.Has<Invulnerable>(eventData.Ship))
            {
                Console.WriteLine("Ship has invulnerability, ignoring hit");
                return;
            }

            Console.WriteLine($"Processing ship hit, current lives: {_currentLives}");

            // Destroy the ship
            DestroyPlayerShip(eventData.Ship, eventData.HitPosition);

            // Lose a life
            LoseLife();

            // Check for game over or start respawn
            if (_currentLives <= 0)
            {
                Console.WriteLine("Game Over - no lives remaining");
                TriggerGameOver();
            }
            else
            {
                Console.WriteLine($"Starting respawn, lives remaining: {_currentLives}");
                StartRespawn();
            }
        }

        public void Handle(GameRestartEvent eventData)
        {
            ResetLives();
            _isRespawning = false;
            _respawnTimer = 0f;
        }

        private void DestroyPlayerShip(Entity ship, Vector2 position)
        {
            // Publish ship destroyed event for effects/sound
            _eventBus.Publish(new ShipDestroyedEvent(ship, position, _currentLives - 1));

            // TODO: Create explosion effect at position
            // _eventBus.Publish(new ExplosionRequestedEvent(position, ExplosionType.Large));

            // Mark ship for destruction
            DestructionSystem.MarkForDestruction(_world, ship, DestructionReason.Collision);
            _playerShipEntity = Entity.Null;
        }

        private void LoseLife()
        {
            _currentLives--;
            UpdateLivesDisplay();

            _eventBus.Publish(new LivesChangedEvent(_currentLives, _currentLives + 1));
        }

        private void StartRespawn()
        {
            _isRespawning = true;
            _respawnTimer = _config.RespawnDelay;

            _eventBus.Publish(new RespawnStartedEvent(_respawnTimer));
        }

        private void RespawnPlayer()
        {
            _isRespawning = false;
            _respawnTimer = 0f;

            // Check if spawn area is safe
            Vector2 spawnPosition = GetSafeSpawnPosition();

            // Create new ship
            _playerShipEntity = CreatePlayerShip(spawnPosition);

            // Add invulnerability
            _world.Add(_playerShipEntity, new Invulnerable(_config.InvulnerabilityDuration));

            Console.WriteLine($"Ship respawned: Entity {_playerShipEntity}, Position {spawnPosition}, Lives: {_currentLives}");
            Console.WriteLine($"Ship has Ship component: {_world.Has<Ship>(_playerShipEntity)}");
            Console.WriteLine($"Ship has Player component: {_world.Has<Player>(_playerShipEntity)}");
            Console.WriteLine($"Ship has CollisionLayer: {_world.Has<CollisionLayerComponent>(_playerShipEntity)}");

            _eventBus.Publish(new ShipRespawnedEvent(_playerShipEntity, spawnPosition, _currentLives));
        }

        private void TriggerGameOver()
        {
            _eventBus.Publish(new GameOverEvent(_currentLives, 0)); // 0 lives remaining
        }

        private void ResetLives()
        {
            _currentLives = _config.StartingLives;
            UpdateLivesDisplay();
        }

        private void UpdateLivesDisplay()
        {
            // Find lives display entity if we don't have it
            if (_livesDisplayEntity == Entity.Null || !_world.IsAlive(_livesDisplayEntity))
            {
                FindLivesDisplayEntity();
            }

            // Update lives display
            if (_livesDisplayEntity != Entity.Null && _world.IsAlive(_livesDisplayEntity))
            {
                if (_world.Has<TextRenderer>(_livesDisplayEntity))
                {
                    ref var textRenderer = ref _world.Get<TextRenderer>(_livesDisplayEntity);
                    textRenderer.Text = $"LIVES: {_currentLives}";
                }
            }
        }

        private void UpdateInvulnerability(float deltaTime)
        {
            // Handle invulnerability for entities with any renderer
            var invulnQuery = new QueryDescription().WithAll<Invulnerable>();

            _world.Query(in invulnQuery, (Entity entity, ref Invulnerable invuln) =>
            {
                invuln.RemainingTime -= deltaTime;

                Console.WriteLine($"Entity {entity} invulnerability: {invuln.RemainingTime:F2} seconds remaining");

                // Flashing effect - toggle visibility every 0.1 seconds
                float flashSpeed = 10f; // 10 flashes per second
                bool isVisible = (int)(invuln.RemainingTime * flashSpeed) % 2 == 0;

                // Apply flashing to triangle renderer if present
                if (_world.Has<TriangleRenderer>(entity))
                {
                    ref var renderer = ref _world.Get<TriangleRenderer>(entity);
                    var color = renderer.Color;
                    renderer.Color = Color.FromArgb(isVisible ? 255 : 100, color.R, color.G, color.B);
                }
                // Apply flashing to rectangle renderer if present
                else if (_world.Has<RectangleRenderer>(entity))
                {
                    ref var renderer = ref _world.Get<RectangleRenderer>(entity);
                    var color = renderer.Color;
                    renderer.Color = Color.FromArgb(isVisible ? 255 : 100, color.R, color.G, color.B);
                }

                // Remove invulnerability when expired
                if (invuln.RemainingTime <= 0f)
                {
                    Console.WriteLine($"Removing invulnerability from entity {entity}");
                    _world.Remove<Invulnerable>(entity);

                    // Restore full alpha for triangle renderer
                    if (_world.Has<TriangleRenderer>(entity))
                    {
                        ref var renderer = ref _world.Get<TriangleRenderer>(entity);
                        var color = renderer.Color;
                        renderer.Color = Color.FromArgb(255, color.R, color.G, color.B);
                    }
                    // Restore full alpha for rectangle renderer
                    else if (_world.Has<RectangleRenderer>(entity))
                    {
                        ref var renderer = ref _world.Get<RectangleRenderer>(entity);
                        var color = renderer.Color;
                        renderer.Color = Color.FromArgb(255, color.R, color.G, color.B);
                    }
                }
            });
        }

        private Vector2 GetSafeSpawnPosition()
        {
            Vector2 centerPosition = new Vector2(_screenWidth / 2, _screenHeight / 2);

            // Check if center is safe (no asteroids nearby)
            if (IsPositionSafe(centerPosition, _config.SafeRespawnRadius))
            {
                return centerPosition;
            }

            // Try a few random positions
            for (int i = 0; i < 10; i++)
            {
                Vector2 randomPos = new Vector2(
                    Random.Shared.NextSingle() * _screenWidth,
                    Random.Shared.NextSingle() * _screenHeight
                );

                if (IsPositionSafe(randomPos, _config.SafeRespawnRadius))
                {
                    return randomPos;
                }
            }

            // If all else fails, spawn at center anyway
            return centerPosition;
        }

        private bool IsPositionSafe(Vector2 position, float safeRadius)
        {
            var asteroidQuery = new QueryDescription().WithAll<Asteroid, Transform>();
            bool isSafe = true;

            _world.Query(in asteroidQuery, (ref Transform asteroidTransform) =>
            {
                float distance = Vector2.Distance(position, asteroidTransform.Position);
                if (distance < safeRadius)
                {
                    isSafe = false;
                }
            });

            return isSafe;
        }

        private Entity CreatePlayerShip(Vector2 position)
        {
            // Use the same parameters as the initial ship creation
            var ship = _world.Create(
                new Transform(position),
                new Velocity(Vector2.Zero),
                new AngularVelocity(0f),
                Ship.Create(400f, 180f, 300f), // Match AsteroidsGame constants
                new Thruster(400f, false),
                new MaxSpeed(300f),
                new Drag(0.005f, 0.01f),
                new BoxCollider(new Vector2(20f, 20f)), // Match AsteroidsGame constants
                new Weapon(fireRate: 6f, bulletSpeed: 400f, bulletLifetime: 2.5f),
                TriangleRenderer.PointingRight(20f, 16f, Color.White), // Match AsteroidsGame constants
                new CustomBoundaryBehavior(BoundaryBehavior.Wrap),
                new CollisionLayerComponent(CollisionLayer.Player),
                new Player()
            );

            return ship;
        }

        private void FindPlayerShip()
        {
            var shipQuery = new QueryDescription().WithAll<Ship, Player>();
            _world.Query(in shipQuery, (Entity entity) =>
            {
                _playerShipEntity = entity;
            });
        }

        private void FindLivesDisplayEntity()
        {
            var textQuery = new QueryDescription().WithAll<TextRenderer>();
            _world.Query(in textQuery, (Entity entity, ref TextRenderer textRenderer) =>
            {
                if (textRenderer.Text.StartsWith("LIVES:"))
                {
                    _livesDisplayEntity = entity;
                }
            });
        }

        /// <summary>
        /// Get current lives information
        /// </summary>
        public LivesInfo GetLivesInfo()
        {
            return new LivesInfo
            {
                CurrentLives = _currentLives,
                MaxLives = _config.StartingLives,
                IsRespawning = _isRespawning,
                RespawnTimeRemaining = _respawnTimer
            };
        }

        /// <summary>
        /// Set the entity that displays lives
        /// </summary>
        public void SetLivesDisplayEntity(Entity livesEntity)
        {
            _livesDisplayEntity = livesEntity;
            UpdateLivesDisplay();
        }
    }

    /// <summary>
    /// Configuration for lives system
    /// </summary>
    public class LivesConfig
    {
        public int StartingLives { get; set; } = 3;
        public float RespawnDelay { get; set; } = 2.0f; // seconds
        public float InvulnerabilityDuration { get; set; } = 3.0f; // seconds
        public float SafeRespawnRadius { get; set; } = 100f; // pixels
        public bool AllowRespawnInDanger { get; set; } = true; // If no safe spot found
    }

    /// <summary>
    /// Information about current lives state
    /// </summary>
    public struct LivesInfo
    {
        public int CurrentLives;
        public int MaxLives;
        public bool IsRespawning;
        public float RespawnTimeRemaining;

        public override string ToString()
        {
            return $"Lives: {CurrentLives}/{MaxLives}, Respawning: {IsRespawning}, Time: {RespawnTimeRemaining:F1}";
        }
    }

    /// <summary>
    /// Event fired when the player ship is destroyed
    /// </summary>
    public class ShipDestroyedEvent : BaseEvent
    {
        public Entity Ship { get; }
        public Vector2 Position { get; }
        public int LivesRemaining { get; }

        public ShipDestroyedEvent(Entity ship, Vector2 position, int livesRemaining)
        {
            Ship = ship;
            Position = position;
            LivesRemaining = livesRemaining;
        }
    }

    /// <summary>
    /// Event fired when lives count changes
    /// </summary>
    public class LivesChangedEvent : BaseEvent
    {
        public int NewLives { get; }
        public int PreviousLives { get; }

        public LivesChangedEvent(int newLives, int previousLives)
        {
            NewLives = newLives;
            PreviousLives = previousLives;
        }
    }

    /// <summary>
    /// Event fired when respawn process starts
    /// </summary>
    public class RespawnStartedEvent : BaseEvent
    {
        public float RespawnDelay { get; }

        public RespawnStartedEvent(float respawnDelay)
        {
            RespawnDelay = respawnDelay;
        }
    }

    /// <summary>
    /// Event fired when ship respawns
    /// </summary>
    public class ShipRespawnedEvent : BaseEvent
    {
        public Entity Ship { get; }
        public Vector2 Position { get; }
        public int LivesRemaining { get; }

        public ShipRespawnedEvent(Entity ship, Vector2 position, int livesRemaining)
        {
            Ship = ship;
            Position = position;
            LivesRemaining = livesRemaining;
        }
    }

    /// <summary>
    /// Event fired when game is over (no lives left)
    /// </summary>
    public class GameOverEvent : BaseEvent
    {
        public int FinalLives { get; }
        public int LivesRemaining { get; }

        public GameOverEvent(int finalLives, int livesRemaining)
        {
            FinalLives = finalLives;
            LivesRemaining = livesRemaining;
        }
    }
}