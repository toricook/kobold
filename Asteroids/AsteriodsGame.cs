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
        // Ship constants
        private const float SHIP_SIZE = 20f;
        private const float SHIP_THRUST_POWER = 400f;
        private const float SHIP_ROTATION_SPEED = 180f; // degrees per second
        private const float SHIP_MAX_SPEED = 300f;

        // Entity references
        private Entity _ship;
        private Entity _gameStateEntity;

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

            // Create game entities
            CreateShip();
            CreateUI();

            // Initialize and register systems
            InitializeSystems();
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
            var physicsSystem = new PhysicsSystem(World, physicsConfig);
            var boundarySystem = new BoundarySystem(World, EventBus, boundaryConfig);
            var collisionSystem = new CollisionSystem(World, EventBus, collisionConfig);
            var renderSystem = new RenderSystem(Renderer, World);

            // Register systems with proper gameplay state requirements
            // These systems should ONLY run during "Playing" state
            SystemManager.AddSystem(shipControlSystem, SystemUpdateOrder.INPUT, requiresGameplayState: true);
            SystemManager.AddSystem(physicsSystem, SystemUpdateOrder.PHYSICS, requiresGameplayState: true);
            SystemManager.AddSystem(boundarySystem, SystemUpdateOrder.PHYSICS + 1, requiresGameplayState: true);
            SystemManager.AddSystem(collisionSystem, SystemUpdateOrder.COLLISION, requiresGameplayState: true);

            // Render system always runs
            SystemManager.AddRenderSystem(renderSystem);

            // Subscribe to boundary events for debugging
            EventBus.Subscribe<BoundaryCollisionEvent>(evt => {
                Console.WriteLine($"Boundary collision: Entity {evt.Entity} hit {evt.Boundary} with behavior {evt.Behavior}");
            });
        }

        private void CreateGameState()
        {
            _gameStateEntity = World.Create(
                new GameState(GameStateType.Playing)
            );
        }

        private void CreateShip()
        {
            var shipPosition = new Vector2(Constants.SCREEN_WIDTH / 2, Constants.SCREEN_HEIGHT / 2);

            _ship = World.Create(
                new Transform(shipPosition),
                new Velocity(Vector2.Zero),
                new AngularVelocity(0f),
                Ship.Create(SHIP_THRUST_POWER, SHIP_ROTATION_SPEED, SHIP_MAX_SPEED),
                new Thruster(SHIP_THRUST_POWER, false),
                new MaxSpeed(SHIP_MAX_SPEED),
                new Drag(0.005f, 0.01f), // Custom drag for ship
                new BoxCollider(new Vector2(SHIP_SIZE, SHIP_SIZE)),
                TriangleRenderer.PointingRight(SHIP_SIZE, SHIP_SIZE * 0.8f, Color.White),
                new CustomBoundaryBehavior(BoundaryBehavior.Wrap),
                new CollisionLayerComponent(CollisionLayer.Player),
                new Player() // Tag for identification
            );
        }

        private void CreateUI()
        {
            // Score display - UI layer
            var scoreDisplay = World.Create(
                new Transform(new Vector2(50f, 50f)),
                TextRenderer.UIText("SCORE: 0", Color.White, 24f)
            );

            // Lives display - UI layer  
            var livesDisplay = World.Create(
                new Transform(new Vector2(Constants.SCREEN_WIDTH - 300f, 50f)),
                TextRenderer.UIText("LIVES: 3", Color.White, 24f)
            );

            // Instructions - UI layer
            var instructions = World.Create(
                new Transform(new Vector2(50f, Constants.SCREEN_HEIGHT - 80f)),
                TextRenderer.UIText("ARROWS: Rotate/Thrust  SPACE: Fire", Color.Gray, 16f)
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

        // Helper methods for creating entities (to be expanded later)
        public Entity CreateBullet(Vector2 position, Vector2 velocity)
        {
            // TODO: Implement when we add the weapon system
            return Entity.Null;
        }

        public Entity CreateAsteroid(Vector2 position, AsteroidSize size)
        {
            // TODO: Implement when we add the asteroid system
            return Entity.Null;
        }

        public Entity CreateExplosion(Vector2 position)
        {
            // TODO: Implement when we add particle effects
            return Entity.Null;
        }
    }
}