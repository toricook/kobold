using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Pong.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Systems
{
    public class GameStateSystem : ISystem, IEventHandler<GameOverEvent>, IEventHandler<GameRestartEvent>
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly IInputManager _inputManager;
        private Entity _gameOverText;
        private Entity _instructionText;
        private bool _gameOverEntitiesCreated = false;

        public GameStateSystem(World world, EventBus eventBus, IInputManager inputManager)
        {
            _world = world;
            _eventBus = eventBus;
            _inputManager = inputManager;

            // Subscribe to events
            eventBus.Subscribe<GameOverEvent>(this);
            eventBus.Subscribe<GameRestartEvent>(this);
        }

        public void Update(float deltaTime)
        {
            // Query current game state
            var gameStateQuery = new QueryDescription().WithAll<GameState>();
            bool isGameOver = false;

            _world.Query(in gameStateQuery, (ref GameState gameState) =>
            {
                isGameOver = gameState.IsGameOver;
            });

            // Handle input during game over
            if (isGameOver && _inputManager.IsKeyPressed(KeyCode.Space))
            {
                _eventBus.Publish(new GameRestartEvent());
            }
        }

        public void Handle(GameOverEvent eventData)
        {
            // Update game state in world
            var gameStateQuery = new QueryDescription().WithAll<GameState>();
            _world.Query(in gameStateQuery, (ref GameState gameState) =>
            {
                gameState = new GameState(
                    GameStateType.GameOver,
                    $"Player {eventData.WinningPlayerId} Wins!",
                    eventData.WinningPlayerId
                );
            });

            ShowGameOverScreen(eventData);
            StopGameplay();
        }

        public void Handle(GameRestartEvent eventData)
        {
            // Update game state in world
            var gameStateQuery = new QueryDescription().WithAll<GameState>();
            _world.Query(in gameStateQuery, (ref GameState gameState) =>
            {
                gameState = new GameState(GameStateType.Playing);
            });

            HideGameOverScreen();
            ResumeGameplay();
        }

        private void ShowGameOverScreen(GameOverEvent eventData)
        {
            if (!_gameOverEntitiesCreated)
            {
                // Game Over title
                _gameOverText = _world.Create(
                    new Transform(new Vector2(300f, 250f)),
                    new TextRenderer($"Player {eventData.WinningPlayerId} Wins!", Color.White, 32f)
                );

                // Instruction text
                _instructionText = _world.Create(
                    new Transform(new Vector2(280f, 320f)),
                    new TextRenderer("Press SPACE to restart", Color.Gray, 16f)
                );

                _gameOverEntitiesCreated = true;
            }
            else
            {
                // Update existing text
                ref var gameOverTextRenderer = ref _world.Get<TextRenderer>(_gameOverText);
                gameOverTextRenderer.Text = $"Player {eventData.WinningPlayerId} Wins!";
            }
        }

        private void HideGameOverScreen()
        {
            if (_gameOverEntitiesCreated)
            {
                _world.Destroy(_gameOverText);
                _world.Destroy(_instructionText);
                _gameOverEntitiesCreated = false;
            }
        }

        private void StopGameplay()
        {
            // Stop ball movement
            var ballQuery = new QueryDescription().WithAll<Ball, Velocity>();
            _world.Query(in ballQuery, (ref Ball ball, ref Velocity velocity) =>
            {
                velocity.Value = Vector2.Zero;
            });

            // Stop paddle movement
            var paddleQuery = new QueryDescription().WithAll<Paddle, Velocity>();
            _world.Query(in paddleQuery, (ref Paddle paddle, ref Velocity velocity) =>
            {
                velocity.Value = Vector2.Zero;
            });
        }

        private void ResumeGameplay()
        {
            // Reset ball position and give it initial velocity
            var ballQuery = new QueryDescription().WithAll<Ball, Transform, Velocity>();
            _world.Query(in ballQuery, (ref Ball ball, ref Transform transform, ref Velocity velocity) =>
            {
                transform.Position = new Vector2(400f, 300f); // Center of screen
                velocity.Value = new Vector2(-ball.Speed, (Random.Shared.NextSingle() - 0.5f) * ball.Speed);
            });

            // Reset paddle positions
            var paddleQuery = new QueryDescription().WithAll<Paddle, Transform>();
            _world.Query(in paddleQuery, (ref Paddle paddle, ref Transform transform) =>
            {
                if (paddle.IsPlayer)
                {
                    transform.Position = new Vector2(50f, 250f); // Left side
                }
                else
                {
                    transform.Position = new Vector2(730f, 250f); // Right side
                }
            });
        }
    }
}
