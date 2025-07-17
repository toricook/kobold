using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
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

        private InputSystem _inputSystem;
        private MovementSystem _movementSystem;
        private AISystem _aiSystem;
        private CollisionSystem _collisionSystem;
        private RenderSystem _renderSystem;

        private Entity _playerPaddle;
        private Entity _aiPaddle;
        private Entity _ball;
        private Entity _scoreDisplay;
        private Score _gameScore;

        public override void Initialize()
        {
            base.Initialize();

            // Initialize systems
            _inputSystem = new InputSystem(InputManager, World);
            _movementSystem = new MovementSystem(World);
            _aiSystem = new AISystem(World);
            _collisionSystem = new CollisionSystem(World, SCREEN_WIDTH, SCREEN_HEIGHT);
            _renderSystem = new RenderSystem(Renderer, World);

            // Create game entities
            CreatePaddles();
            CreateBall();
            CreateUI();

            _gameScore = new Score();
        }

        private void CreatePaddles()
        {
            // Player paddle (left side)
            _playerPaddle = World.Create(
                new Transform(new Vector2(50f, SCREEN_HEIGHT / 2 - PADDLE_HEIGHT / 2)),
                new Velocity(Vector2.Zero),
                new Paddle(PADDLE_SPEED, isPlayer: true),
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

            // Update all systems
            _inputSystem.Update(deltaTime);
            _aiSystem.Update(deltaTime);
            _movementSystem.Update(deltaTime);
            _collisionSystem.Update(deltaTime);

            // Update score display
            UpdateScoreDisplay();

            // Check for game restart
            if (InputManager.IsKeyPressed(KeyCode.Space))
            {
                ResetGame();
            }
        }

        private void UpdateScoreDisplay()
        {
            ref var textRenderer = ref World.Get<TextRenderer>(_scoreDisplay);
            textRenderer.Text = $"{_gameScore.PlayerScore} - {_gameScore.AIScore}";
        }

        private void ResetGame()
        {
            // Reset ball position and velocity
            ref var ballTransform = ref World.Get<Transform>(_ball);
            ref var ballVelocity = ref World.Get<Velocity>(_ball);

            ballTransform.Position = new Vector2(SCREEN_WIDTH / 2 - BALL_SIZE / 2, SCREEN_HEIGHT / 2 - BALL_SIZE / 2);
            ballVelocity.Value = new Vector2(-BALL_SPEED, (Random.Shared.NextSingle() - 0.5f) * BALL_SPEED);

            // Reset paddle positions
            ref var playerTransform = ref World.Get<Transform>(_playerPaddle);
            ref var aiTransform = ref World.Get<Transform>(_aiPaddle);

            playerTransform.Position = new Vector2(50f, SCREEN_HEIGHT / 2 - PADDLE_HEIGHT / 2);
            aiTransform.Position = new Vector2(SCREEN_WIDTH - 50f - PADDLE_WIDTH, SCREEN_HEIGHT / 2 - PADDLE_HEIGHT / 2);

            // Reset scores
            _gameScore = new Score();
        }

        public override void Render()
        {
            _renderSystem.Render();
        }
    }
}