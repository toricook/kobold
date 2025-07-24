using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Kobold.Core;
using Asteroids.Components;
using Asteroids.Systems;
using System.Drawing;
using System.Numerics;

namespace Asteroids
{
    public class AsteroidsGame : GameEngineBase
    {
        // Entity references
        private Entity _gameStateEntity;
        private Entity _scoreDisplay;
        private Entity _highScoreDisplay;
        private Entity _livesDisplay;

        public AsteroidsGame() : base()
        {
        }

        public AsteroidsGame(IRenderer renderer, IInputManager inputManager)
            : base(renderer, inputManager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            // Create game state entity
            CreateGameState();

            // Create UI (no ship creation needed)
            CreateUI();

            // Initialize and register systems
            InitializeSystems();

            // Spawn initial wave of asteroids
            var asteroidSystem = SystemManager.GetSystem<AsteroidSystem>();
            asteroidSystem?.SpawnWave(1);

            // Create initial player ship through lives system
            CreateInitialShip();
        }

        private void InitializeSystems()
        {
            // Configure physics for space-like movement
            var physicsConfig = new PhysicsConfig
            {
                EnableGravity = false,
                EnableDamping = true,
                EnableRotationalPhysics = true,
                EnableThrust = true,
                EnableSpeedLimits = true,
                DefaultLinearDamping = 0.01f, // Very light space friction
                DefaultAngularDamping = 0.02f,
                UseMultiplicativeDamping = true
            };

            // Configure boundary system for wrapping
            var boundaryConfig = new BoundaryConfig(Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT)
            {
                DefaultBehavior = BoundaryBehavior.Wrap,
                PlayerBehavior = BoundaryBehavior.Wrap,
                ProjectileBehavior = BoundaryBehavior.Destroy,
                EnemyBehavior = BoundaryBehavior.Wrap
            };

            // Configure collision system
            var collisionConfig = new CollisionConfig
            {
                EnableCollisionResponse = false // We'll handle collisions in game-specific systems
            };

            // Create systems
            var shipControlSystem = new ShipControlSystem(World, InputManager);
            var weaponSystem = new WeaponSystem(World, InputManager, EventBus);
            var scoringSystem = new ScoringSystem(World, EventBus);
            var livesSystem = new LivesSystem(World, EventBus, Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT);
            var asteroidSystem = new AsteroidSystem(World, EventBus, Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT);
            var asteroidCollisionHandler = new AsteroidCollisionHandler(World, EventBus);
            var physicsSystem = new PhysicsSystem(World, physicsConfig);
            var boundarySystem = new BoundarySystem(World, EventBus, boundaryConfig);
            var collisionSystem = new CollisionSystem(World, EventBus, collisionConfig);
            var destructionSystem = new DestructionSystem(World, EventBus);
            var renderSystem = new RenderSystem(Renderer, World);

            // Register systems with proper gameplay state requirements
            // These systems should ONLY run during "Playing" state
            SystemManager.AddSystem(shipControlSystem, SystemUpdateOrder.INPUT, requiresGameplayState: true);
            SystemManager.AddSystem(weaponSystem, SystemUpdateOrder.INPUT + 1, requiresGameplayState: true);
            SystemManager.AddSystem(physicsSystem, SystemUpdateOrder.PHYSICS, requiresGameplayState: true);
            SystemManager.AddSystem(boundarySystem, SystemUpdateOrder.PHYSICS + 10, requiresGameplayState: true);
            SystemManager.AddSystem(collisionSystem, SystemUpdateOrder.COLLISION, requiresGameplayState: true);
            SystemManager.AddSystem(asteroidCollisionHandler, SystemUpdateOrder.COLLISION + 1, requiresGameplayState: true);
            SystemManager.AddSystem(asteroidSystem, SystemUpdateOrder.GAME_LOGIC, requiresGameplayState: true);
            SystemManager.AddSystem(destructionSystem, SystemUpdateOrder.CLEANUP, requiresGameplayState: true);

            // These systems should ALWAYS run (during any state)
            SystemManager.AddSystem(scoringSystem, SystemUpdateOrder.GAME_LOGIC - 1, requiresGameplayState: false);
            SystemManager.AddSystem(livesSystem, SystemUpdateOrder.GAME_LOGIC, requiresGameplayState: false);

            // Render system always runs
            SystemManager.AddRenderSystem(renderSystem);
        }

        private void CreateGameState()
        {
            _gameStateEntity = World.Create(
                new GameState(GameStateType.Playing)
            );
        }

        private void CreateUI()
        {
            // Score display - UI layer
            _scoreDisplay = World.Create(
                new Transform(new Vector2(50f, 50f)),
                TextRenderer.UIText("SCORE: 0", Color.White, 24f)
            );

            // High score display - UI layer
            _highScoreDisplay = World.Create(
                new Transform(new Vector2(50f, 80f)),
                TextRenderer.UIText("HIGH: 10000", Color.Yellow, 20f)
            );

            // Lives display - UI layer  
            _livesDisplay = World.Create(
                new Transform(new Vector2(Constants.SCREEN_WIDTH - 150f, 50f)),
                TextRenderer.UIText("LIVES: 3", Color.White, 24f)
            );

            // Instructions - UI layer
            var instructions = World.Create(
                new Transform(new Vector2(50f, Constants.SCREEN_HEIGHT - 80f)),
                TextRenderer.UIText("ARROWS: Rotate/Thrust  SPACE: Fire", Color.Gray, 16f)
            );

            // Set up scoring system with display entities
            var scoringSystem = SystemManager.GetSystem<ScoringSystem>();
            scoringSystem?.SetScoreDisplayEntities(_scoreDisplay, _highScoreDisplay);

            // Set up lives system with display entity
            var livesSystem = SystemManager.GetSystem<LivesSystem>();
            livesSystem?.SetLivesDisplayEntity(_livesDisplay);
        }

        private void CreateInitialShip()
        {
            // Create the initial ship manually (not through respawn system)
            var shipPosition = new Vector2(Constants.SCREEN_WIDTH / 2, Constants.SCREEN_HEIGHT / 2);

            var ship = World.Create(
                new Transform(shipPosition),
                new Velocity(Vector2.Zero),
                new AngularVelocity(0f),
                Ship.Create(400f, 180f, 300f),
                new Thruster(400f, false),
                new MaxSpeed(300f),
                new Drag(0.005f, 0.01f),
                new BoxCollider(new Vector2(20f, 20f)),
                new Weapon(fireRate: 6f, bulletSpeed: 400f, bulletLifetime: 2.5f),
                TriangleRenderer.PointingRight(20f, 16f, Color.White),
                new CustomBoundaryBehavior(BoundaryBehavior.Wrap),
                new CollisionLayerComponent(CollisionLayer.Player),
                new Player()
            );
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Render()
        {
            var renderSystem = SystemManager.GetSystem<RenderSystem>();
            renderSystem?.Render();
        }
    }
}