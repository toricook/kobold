using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Kobold.Core;
using Pong.Components;
using Pong.Systems;
using System.Drawing;
using System.Numerics;

namespace Pong
{
    public class PongGame : GameEngineBase
    {
        private const float SCREEN_WIDTH = 800f;
        private const float SCREEN_HEIGHT = 600f;
        private const float PADDLE_WIDTH = 20f;
        private const float PADDLE_HEIGHT = 100f;
        private const float BALL_SIZE = 20f;
        private const float PADDLE_SPEED = 300f;
        private const float BALL_SPEED = 250f;

        private Entity _playerPaddle;
        private Entity _aiPaddle;
        private Entity _ball;
        private Entity _scoreDisplay;
        private Entity _gameStateEntity;

        public PongGame() : base()
        {
        }

        public PongGame(IRenderer renderer, IInputManager inputManager)
            : base(renderer, inputManager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            // Create game state entity
            CreateGameState();

            // Create game entities
            CreatePaddles();
            CreateBall();
            CreateUI();

            // Initialize and register systems
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            var physicsConfig = new PhysicsConfig
            {
                EnableGravity = false,
                EnableDamping = false
            };

            // Create systems
            var inputSystem = new InputSystem(InputManager, World);
            var physicsSystem = new PhysicsSystem(World, physicsConfig);
            var coreCollisionSystem = new CollisionSystem(World, EventBus);
            var pongCollisionHandler = new PongCollisionHandler(World, EventBus, SCREEN_WIDTH, SCREEN_HEIGHT);
            var aiSystem = new AISystem(World);
            var renderSystem = new RenderSystem(Renderer, World);
            var scoreSystem = new ScoreSystem(World, EventBus, 3);
            var gameStateSystem = new PongGameStateSystem(World, EventBus, InputManager);
            var pongUISystem = new PongUISystem(World, EventBus);

            // Register systems with proper gameplay state requirements

            // These systems should ONLY run during "Playing" state
            SystemManager.AddSystem(inputSystem, SystemUpdateOrder.INPUT, requiresGameplayState: true);
            SystemManager.AddSystem(physicsSystem, SystemUpdateOrder.PHYSICS, requiresGameplayState: true);
            SystemManager.AddSystem(coreCollisionSystem, SystemUpdateOrder.COLLISION, requiresGameplayState: true);
            SystemManager.AddSystem(pongCollisionHandler, SystemUpdateOrder.COLLISION + 1, requiresGameplayState: true);
            SystemManager.AddSystem(aiSystem, SystemUpdateOrder.AI, requiresGameplayState: true);

            // These systems should ALWAYS run (during any state)
            SystemManager.AddSystem(scoreSystem, SystemUpdateOrder.GAME_LOGIC, requiresGameplayState: false);
            SystemManager.AddSystem(gameStateSystem, SystemUpdateOrder.UI, requiresGameplayState: false);
            SystemManager.AddSystem(pongUISystem, SystemUpdateOrder.UI + 1, requiresGameplayState: false);

            // Render system always runs
            SystemManager.AddRenderSystem(renderSystem);
        }

        private void CreateGameState()
        {
            _gameStateEntity = World.Create(
                new GameState(GameStateType.Playing)
            );
        }

        private void CreatePaddles()
        {
            // Player paddle (left side) - game object layer
            _playerPaddle = World.Create(
                new Transform(new Vector2(50f, SCREEN_HEIGHT / 2 - PADDLE_HEIGHT / 2)),
                new Velocity(Vector2.Zero),
                new Paddle(PADDLE_SPEED, isPlayer: true),
                PlayerControlled.CreateVerticalOnly(PADDLE_SPEED),
                new BoxCollider(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT)),
                RectangleRenderer.GameObject(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT), Color.White)
            );

            // AI paddle (right side) - game object layer
            _aiPaddle = World.Create(
                new Transform(new Vector2(SCREEN_WIDTH - 50f - PADDLE_WIDTH, SCREEN_HEIGHT / 2 - PADDLE_HEIGHT / 2)),
                new Velocity(Vector2.Zero),
                new Paddle(PADDLE_SPEED, isPlayer: false),
                new BoxCollider(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT)),
                RectangleRenderer.GameObject(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT), Color.White)
            );
        }

        private void CreateBall()
        {
            var initialVelocity = new Vector2(-BALL_SPEED, (Random.Shared.NextSingle() - 0.5f) * BALL_SPEED);

            _ball = World.Create(
                new Transform(new Vector2(SCREEN_WIDTH / 2 - BALL_SIZE / 2, SCREEN_HEIGHT / 2 - BALL_SIZE / 2)),
                new Velocity(initialVelocity),
                new Ball(BALL_SPEED),
                new BoxCollider(new Vector2(BALL_SIZE, BALL_SIZE)),
                RectangleRenderer.GameObject(new Vector2(BALL_SIZE, BALL_SIZE), Color.White)
            );
        }

        private void CreateUI()
        {
            // Score display - UI layer
            _scoreDisplay = World.Create(
                new Transform(new Vector2(SCREEN_WIDTH / 2 - 50f, 50f)),
                TextRenderer.UIText("0 - 0", Color.White, 24f)
            );

            // Center line - background layer
            for (int i = 0; i < SCREEN_HEIGHT; i += 20)
            {
                World.Create(
                    new Transform(new Vector2(SCREEN_WIDTH / 2 - 2f, i)),
                    RectangleRenderer.Background(new Vector2(4f, 10f), Color.Red)
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