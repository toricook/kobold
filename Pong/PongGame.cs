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
            // Physics configuration - no gravity or damping for Pong
            var physicsConfig = new PhysicsConfig
            {
                EnableGravity = false,
                EnableDamping = false
            };

            // Create systems - using core systems where possible
            var inputSystem = new InputSystem(InputManager, World);
            var physicsSystem = new PhysicsSystem(World, physicsConfig);
            var coreCollisionSystem = new CollisionSystem(World, EventBus);
            var pongCollisionHandler = new PongCollisionHandler(World, EventBus, SCREEN_WIDTH, SCREEN_HEIGHT);
            var aiSystem = new AISystem(World);
            var renderSystem = new RenderSystem(Renderer, World); // Core RenderSystem
            var scoreSystem = new ScoreSystem(World, EventBus, 2);
            var gameStateSystem = new PongGameStateSystem(World, EventBus, InputManager);

            // Register update systems with ordering
            SystemManager.AddSystem(inputSystem, SystemUpdateOrder.INPUT, requiresGameplayState: true);
            SystemManager.AddSystem(physicsSystem, SystemUpdateOrder.PHYSICS, requiresGameplayState: true);
            SystemManager.AddSystem(coreCollisionSystem, SystemUpdateOrder.COLLISION, requiresGameplayState: true);
            SystemManager.AddSystem(pongCollisionHandler, SystemUpdateOrder.COLLISION + 1, requiresGameplayState: true);
            SystemManager.AddSystem(aiSystem, SystemUpdateOrder.AI, requiresGameplayState: true);
            SystemManager.AddSystem(scoreSystem, SystemUpdateOrder.GAME_LOGIC, requiresGameplayState: false);
            SystemManager.AddSystem(gameStateSystem, SystemUpdateOrder.UI, requiresGameplayState: false);

            // Register render systems separately
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
            // Player paddle (left side)
            _playerPaddle = World.Create(
                        new Transform(new Vector2(50f, SCREEN_HEIGHT / 2 - PADDLE_HEIGHT / 2)),
                        new Velocity(Vector2.Zero),
                        new Paddle(PADDLE_SPEED, isPlayer: true),
                        PlayerControlled.CreateVerticalOnly(PADDLE_SPEED),
                        new BoxCollider(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT)),
                        new RectangleRenderer(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT), Color.White)
                    );

            // AI paddle (right side)
            _aiPaddle = World.Create(
                new Transform(new Vector2(SCREEN_WIDTH - 50f - PADDLE_WIDTH, SCREEN_HEIGHT / 2 - PADDLE_HEIGHT / 2)),
                new Velocity(Vector2.Zero),
                new Paddle(PADDLE_SPEED, isPlayer: false),
                new BoxCollider(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT)),
                new RectangleRenderer(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT), Color.White)
            );
        }

        private void CreateBall()
        {
            // Start ball moving towards player
            var initialVelocity = new Vector2(-BALL_SPEED, (Random.Shared.NextSingle() - 0.5f) * BALL_SPEED);

            _ball = World.Create(
                new Transform(new Vector2(SCREEN_WIDTH / 2 - BALL_SIZE / 2, SCREEN_HEIGHT / 2 - BALL_SIZE / 2)),
                new Velocity(initialVelocity),
                new Ball(BALL_SPEED),
                new BoxCollider(new Vector2(BALL_SIZE, BALL_SIZE)),
                new RectangleRenderer(new Vector2(BALL_SIZE, BALL_SIZE), Color.White)
            );
        }

        private void CreateUI()
        {
            // Score display
            _scoreDisplay = World.Create(
                new Transform(new Vector2(SCREEN_WIDTH / 2 - 50f, 50f)),
                new TextRenderer("0 - 0", Color.White, 24f)
            );

            // Center line
            for (int i = 0; i < SCREEN_HEIGHT; i += 20)
            {
                World.Create(
                    new Transform(new Vector2(SCREEN_WIDTH / 2 - 2f, i)),
                    new RectangleRenderer(new Vector2(4f, 10f), Color.White)
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