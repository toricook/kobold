using Arch.Core;
using Kobold.Core;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;
using Kobold.Core.Components;
using Kobold.Core.Components.Gameplay;
using Kobold.Core.Systems;
using System.Drawing;
using System.Numerics;

namespace CaveExplorer
{
    public class CaveExplorerGame : GameEngineBase
    {
        // Entity references
        private Entity _gameStateEntity;
        private SpriteSheet _spriteSheet;

        public CaveExplorerGame() : base()
        {
        }

        public CaveExplorerGame(IRenderer renderer, IInputManager inputManager)
            : base(renderer, inputManager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            // Create game state entity
            CreateGameState();

            // Initialize and register systems
            InitializeSystems();

            // Create initial game entities
            CreateInitialEntities();
        }

        private void InitializeSystems()
        {
            // Configure physics
            var physicsConfig = new PhysicsConfig
            {
                EnableGravity = true,
                EnableDamping = true,
                EnableRotationalPhysics = false,
                EnableThrust = false,
                EnableSpeedLimits = true,
                DefaultLinearDamping = 0.1f,
                DefaultAngularDamping = 0.1f,
                UseMultiplicativeDamping = true
            };

            // Configure boundary system
            var boundaryConfig = new BoundaryConfig(Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT)
            {
                DefaultBehavior = BoundaryBehavior.Clamp,
                PlayerBehavior = BoundaryBehavior.Clamp,
                ProjectileBehavior = BoundaryBehavior.Destroy,
                EnemyBehavior = BoundaryBehavior.Clamp
            };

            // Configure collision system
            var collisionConfig = new CollisionConfig
            {
                EnableCollisionResponse = true
            };

            // Create systems
            var physicsSystem = new PhysicsSystem(World, physicsConfig);
            var boundarySystem = new BoundarySystem(World, EventBus, boundaryConfig);
            var collisionSystem = new CollisionSystem(World, EventBus, collisionConfig);
            var destructionSystem = new DestructionSystem(World, EventBus);
            var renderSystem = new RenderSystem(Renderer, World);

            // Register systems
            SystemManager.AddSystem(physicsSystem, SystemUpdateOrder.PHYSICS, requiresGameplayState: true);
            SystemManager.AddSystem(boundarySystem, SystemUpdateOrder.PHYSICS + 10, requiresGameplayState: true);
            SystemManager.AddSystem(collisionSystem, SystemUpdateOrder.COLLISION, requiresGameplayState: true);
            SystemManager.AddSystem(destructionSystem, SystemUpdateOrder.CLEANUP, requiresGameplayState: true);

            // Render system always runs
            SystemManager.AddRenderSystem(renderSystem);
        }

        private void CreateGameState()
        {
            _gameStateEntity = World.Create(
                new CoreGameState(StandardGameState.Playing)
            );
        }

        private void CreateInitialEntities()
        {
            // Load sprite sheet
            _spriteSheet = Assets.LoadSpriteSheet("sprites");

            // Create test sprites to display different sprites from the sheet
            // Player sprite (frame 0)
            var player = World.Create(
                new Transform(new Vector2(Constants.SCREEN_WIDTH / 2, Constants.SCREEN_HEIGHT / 2)),
                new SpriteRenderer(_spriteSheet.Texture,
                    _spriteSheet.GetFrame(0),
                    new Vector2(2f, 2f))
            );

            // Enemy sprite (frame 1)
            var enemy1 = World.Create(
                new Transform(new Vector2(200, 200)),
                new SpriteRenderer(_spriteSheet.Texture,
                    _spriteSheet.GetFrame(1),
                    new Vector2(2f, 2f))
            );

            // Another sprite (frame 2)
            var enemy2 = World.Create(
                new Transform(new Vector2(800, 200)),
                new SpriteRenderer(_spriteSheet.Texture,
                    _spriteSheet.GetFrame(2),
                    new Vector2(2f, 2f))
            );

            // Display a sprite from the second row (frame 21 = first sprite of row 2)
            var secondRowSprite = World.Create(
                new Transform(new Vector2(200, 500)),
                new SpriteRenderer(_spriteSheet.Texture,
                    _spriteSheet.GetFrame(21),
                    new Vector2(2f, 2f))
            );

            // Using a named region (if you want to use specific sprites)
            if (_spriteSheet.HasNamedRegion("player"))
            {
                var namedPlayer = World.Create(
                    new Transform(new Vector2(800, 500)),
                    new SpriteRenderer(_spriteSheet.Texture,
                        _spriteSheet.GetNamedRegion("player"),
                        new Vector2(2f, 2f))
                );
            }
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
