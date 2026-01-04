using Arch.Core;
using Kobold.Core;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;
using Kobold.Core.Components;
using Kobold.Extensions.Gameplay.Components;
using Kobold.Core.Services;
using Kobold.Core.Systems;
using Kobold.Extensions.Physics.Systems;
using Kobold.Extensions.Collision.Systems;
using Kobold.Extensions.Tilemaps;
using System.Drawing;
using System.Numerics;

using Kobold.Extensions.Input.Systems;
using Kobold.Extensions.Triggers.Systems;
using Kobold.Extensions.Destruction.Systems;
using Kobold.Extensions.Input.Components;
using Kobold.Extensions.Collision.Components;
using Kobold.Extensions.Physics.Components;
using PhysicsComponent = Kobold.Extensions.Physics.Components.Physics;

namespace Kobold.Extensions.GameBases
{
    /// <summary>
    /// Base class for platformer games that provides common system setup and helper methods.
    /// Automatically configures physics with gravity, collision, input, rendering, and tilemap systems.
    /// </summary>
    public abstract class PlatformerGameBase : GameEngineBase
    {
        /// <summary>
        /// Default screen width for platformers (can be customized in derived classes)
        /// </summary>
        protected virtual int ScreenWidth => 1280;

        /// <summary>
        /// Default screen height for platformers (can be customized in derived classes)
        /// </summary>
        protected virtual int ScreenHeight => 720;

        /// <summary>
        /// Reference to the main camera entity
        /// </summary>
        protected Entity CameraEntity;

        /// <summary>
        /// Reference to the player entity
        /// </summary>
        protected Entity PlayerEntity;

        public PlatformerGameBase() : base()
        {
        }

        public PlatformerGameBase(IRenderer renderer, IInputManager inputManager)
            : base(renderer, inputManager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            // Initialize platformer-specific systems
            InitializePlatformerSystems();

            // Subscribe to player death events
            EventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);

            // Allow derived classes to create their game entities
            CreateGameEntities();
        }

        /// <summary>
        /// Called when the player dies. Default behavior is to restart the game.
        /// Override to customize death handling (e.g., show game over screen, decrease lives, etc.)
        /// </summary>
        protected virtual void OnPlayerDeath(PlayerDeathEvent deathEvent)
        {
            System.Console.WriteLine($"=== PLAYER DIED ===");
            System.Console.WriteLine($"Reason: {deathEvent.Reason}");
            RestartGame();
        }

        /// <summary>
        /// Restarts the game by clearing all entities and recreating them.
        /// Override to customize restart behavior.
        /// </summary>
        protected virtual void RestartGame()
        {
            System.Console.WriteLine($"=== RESTARTING GAME ===");

            // Clear all entities
            World.Clear();

            // Recreate game entities
            CreateGameEntities();

            // Log player respawn position
            var query = new QueryDescription().WithAll<Player, Transform>();
            World.Query(in query, (ref Transform transform) =>
            {
                System.Console.WriteLine($"Player respawned at position: ({transform.Position.X}, {transform.Position.Y})");
            });

            System.Console.WriteLine($"=== GAME RESTARTED ===");
        }

        /// <summary>
        /// Initializes common platformer systems with sensible defaults.
        /// Override RegisterAdditionalSystems() to add more systems.
        /// </summary>
        protected virtual void InitializePlatformerSystems()
        {
            // Configure physics for platformer (with gravity)
            var physicsConfig = ConfigurePlatformerPhysics();

            // Configure collision
            var collisionConfig = ConfigurePlatformerCollision();

            // Configure jumping
            var jumpingConfig = ConfigurePlatformerJumping();

            // Create core systems
            var inputSystem = new InputSystem(InputManager, World);
            var jumpingSystem = new JumpingSystem(World, InputManager, jumpingConfig);
            var physicsSystem = new PhysicsSystem(World, physicsConfig);
            var collisionSystem = new CollisionSystem(World, EventBus, collisionConfig);
            var tilemapCollisionSystem = new TilemapCollisionSystem(World);
            var deathZoneSystem = new DeathZoneSystem(World, ScreenHeight + 100f, EventBus); // Death zone slightly below screen
            var triggerSystem = new TriggerSystem(World, EventBus, InputManager, KeyCode.E);
            var destructionSystem = new DestructionSystem(World, EventBus);
            var renderSystem = new RenderSystem(Renderer, World);

            // Register systems in execution order
            SystemManager.AddSystem(inputSystem, SystemUpdateOrder.INPUT);
            SystemManager.AddSystem(jumpingSystem, SystemUpdateOrder.INPUT + 1);
            SystemManager.AddSystem(tilemapCollisionSystem, SystemUpdateOrder.PHYSICS - 1);
            SystemManager.AddSystem(physicsSystem, SystemUpdateOrder.PHYSICS);
            SystemManager.AddSystem(collisionSystem, SystemUpdateOrder.COLLISION);
            SystemManager.AddSystem(deathZoneSystem, SystemUpdateOrder.GAME_LOGIC);
            SystemManager.AddSystem(triggerSystem, SystemUpdateOrder.COLLISION + 1);
            SystemManager.AddSystem(destructionSystem, SystemUpdateOrder.CLEANUP);

            // Register render systems
            SystemManager.AddRenderSystem(renderSystem);

            // Allow derived classes to add additional systems
            RegisterAdditionalSystems();
        }

        /// <summary>
        /// Configures physics for a platformer game with gravity enabled.
        /// Override to customize physics settings.
        /// </summary>
        protected virtual PhysicsConfig ConfigurePlatformerPhysics()
        {
            return new PhysicsConfig
            {
                EnableGravity = true,
                EnableDamping = true,
                EnableRotationalPhysics = false,
                EnableThrust = false,
                EnableSpeedLimits = true,
                GlobalGravity = new Vector2(0, 1200f), // Platformer gravity (positive Y = down in screen coordinates)
                DefaultDamping = 0.0f, // No damping by default for platformers
                MinVelocityThreshold = 0.01f,
                UseMultiplicativeDamping = false
            };
        }

        /// <summary>
        /// Configures collision for a platformer game.
        /// Override to customize collision settings.
        /// </summary>
        protected virtual CollisionConfig ConfigurePlatformerCollision()
        {
            return new CollisionConfig
            {
                // NOTE: CollisionSystem only applies velocity impulses, not position correction.
                // For platformers, use tilemaps for solid platforms (they include position correction).
                // Entity-entity collisions will still be detected and published as events.
                EnableCollisionResponse = false // Disabled - no position correction in CollisionSystem
            };
        }

        /// <summary>
        /// Configures jumping for a platformer game.
        /// Override to customize jump behavior.
        /// </summary>
        protected virtual JumpingConfig ConfigurePlatformerJumping()
        {
            return new JumpingConfig
            {
                JumpForce = 500f, // Reasonable jump height
                GroundedThreshold = 50f, // Increased tolerance for ground detection (was 10f)
                JumpKey = KeyCode.Space,
                AlternateJumpKey = KeyCode.W
            };
        }

        /// <summary>
        /// Override this to register additional game-specific systems (UI, AI, combat, etc.).
        /// Called after core platformer systems are registered.
        /// </summary>
        protected virtual void RegisterAdditionalSystems()
        {
            // Override in derived classes to add custom systems
        }

        /// <summary>
        /// Creates the initial game entities (player, camera, level, etc.).
        /// Must be implemented by derived classes.
        /// </summary>
        protected abstract void CreateGameEntities();

        /// <summary>
        /// Helper method to create a basic platformer player entity with common components.
        /// Override to customize player creation.
        /// Note: For jumping, add a Jumping component separately or handle in your own input system.
        /// </summary>
        /// <param name="position">Starting position</param>
        /// <param name="spriteTexture">Sprite texture</param>
        /// <param name="spriteRegion">Sprite region</param>
        /// <param name="moveSpeed">Horizontal movement speed</param>
        /// <returns>The created player entity</returns>
        protected virtual Entity CreatePlayer(
            Vector2 position,
            ITexture spriteTexture,
            Rectangle spriteRegion,
            float moveSpeed = 200f)
        {
            PlayerEntity = World.Create(
                new Transform(position),
                new Velocity(Vector2.Zero),
                new SpriteRenderer(spriteTexture, spriteRegion, new Vector2(1f, 1f)),
                new BoxCollider(28f, 28f, new Vector2(-14f, -14f)),
                PlayerControlled.CreateHorizontalOnly(moveSpeed), // Horizontal movement only for platformers
                new PhysicsComponent(damping: 0.0f), // Enable gravity
                new Player()
            );

            return PlayerEntity;
        }

        /// <summary>
        /// Helper method to create a platformer camera that follows the player.
        /// Override to customize camera creation.
        /// </summary>
        /// <param name="initialPosition">Initial camera position (usually player's starting position)</param>
        /// <param name="smoothSpeed">Camera follow smoothness (0 = instant, higher = smoother)</param>
        /// <returns>The created camera entity</returns>
        protected virtual Entity CreateCamera(Vector2 initialPosition, float smoothSpeed = 5f)
        {
            var camera = new Camera(ScreenWidth, ScreenHeight, smoothSpeed)
            {
                Position = initialPosition,
                FollowTarget = true
            };

            CameraEntity = World.Create(camera);
            return CameraEntity;
        }

        /// <summary>
        /// Helper method to set camera bounds (typically based on level size).
        /// Call this after loading a level to prevent camera from showing out-of-bounds areas.
        /// </summary>
        /// <param name="mapWidth">Width of the level in pixels</param>
        /// <param name="mapHeight">Height of the level in pixels</param>
        protected void SetCameraBounds(float mapWidth, float mapHeight)
        {
            if (World.IsAlive(CameraEntity))
            {
                ref var camera = ref World.Get<Camera>(CameraEntity);
                camera.SetBounds(mapWidth, mapHeight);
            }
        }

        /// <summary>
        /// Helper method to create a simple tilemap for platformer levels.
        /// Tilemaps provide proper collision with position correction.
        /// </summary>
        /// <param name="width">Width in tiles</param>
        /// <param name="height">Height in tiles</param>
        /// <param name="tileWidth">Width of each tile in pixels</param>
        /// <param name="tileHeight">Height of each tile in pixels</param>
        /// <returns>A new tilemap</returns>
        protected virtual TileMap CreateSimpleTileMap(int width, int height, int tileWidth = 32, int tileHeight = 32)
        {
            return new TileMap(width, height, tileWidth, tileHeight);
        }

        /// <summary>
        /// Helper to create a simple tileset with collision properties.
        /// </summary>
        /// <param name="tileWidth">Width of each tile in pixels</param>
        /// <param name="tileHeight">Height of each tile in pixels</param>
        /// <returns>A basic tileset</returns>
        protected virtual TileSet CreateSimpleTileSet(int tileWidth = 32, int tileHeight = 32)
        {
            var tileSet = new TileSet(tileWidth, tileHeight);

            // Tile 0: Empty/Air (no collision) - default properties
            tileSet.SetTileProperties(0, new TileProperties
            {
                IsSolid = false,
                CollisionLayer = TileCollisionLayer.None
            });

            // Tile 1: Solid block (with collision)
            tileSet.SetTileProperties(1, TileProperties.Solid());

            return tileSet;
        }

        /// <summary>
        /// Helper method to load a tilemap and create a tilemap entity.
        /// Common pattern for platformer levels.
        /// </summary>
        /// <param name="tileMap">The tilemap</param>
        /// <param name="tileSet">The tileset</param>
        /// <param name="texture">Tileset texture</param>
        /// <param name="spriteSheet">Optional sprite sheet</param>
        /// <returns>The created tilemap entity</returns>
        protected virtual Entity CreateTileMapEntity(
            TileMap tileMap,
            TileSet tileSet,
            ITexture texture,
            SpriteSheet? spriteSheet = null)
        {
            return World.Create(
                new TileMapComponent(tileMap, tileSet, texture, spriteSheet)
            );
        }
    }
}
