using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Factories;
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
    public class PongGameStateSystem : GameStateSystem<GameState>, IEventHandler<GameOverEvent>
    {
        private const float SCREEN_WIDTH = 800f;
        private const float SCREEN_HEIGHT = 600f;

        public PongGameStateSystem(World world, EventBus eventBus, IInputManager inputManager)
            : base(world, eventBus, inputManager)
        {
            // Configure game states
            ConfigureGameStates();

            // Subscribe to game events
            eventBus.Subscribe<GameOverEvent>(this);
        }

        private void ConfigureGameStates()
        {
            // Configure Game Over state
            ConfigureState(new GameState(GameStateType.GameOver), new GameStateConfig
            {
                UIElements = new List<UIElementConfig>
                {
                    UIElementFactory.CreateText(new Vector2(300f, 250f), "Game Over!", Color.White, 32f),
                    UIElementFactory.CreateText(new Vector2(280f, 320f), "Press SPACE to restart", Color.Gray, 16f)
                },
                InputTransitions = new List<InputTransition>
                {
                    new InputTransition
                    {
                        Key = KeyCode.Space,
                        NextState = new GameState(GameStateType.Playing),
                        OnTransition = RestartGame
                    }
                },
                OnStateEnter = () => StopAllMovement()
            });
        }

        public void Handle(GameOverEvent eventData)
        {
            var gameOverState = new GameState(GameStateType.GameOver,
                $"Player {eventData.WinningPlayerId} Wins!",
                eventData.WinningPlayerId);

            ChangeState(gameOverState);
        }

        private void RestartGame()
        {
            // Reset ball position and give it initial velocity
            var ballQuery = new QueryDescription().WithAll<Ball, Transform, Velocity>();
            World.Query(in ballQuery, (ref Ball ball, ref Transform transform, ref Velocity velocity) =>
            {
                transform.Position = new Vector2(SCREEN_WIDTH / 2, SCREEN_HEIGHT / 2);
                velocity.Value = new Vector2(-ball.Speed, (Random.Shared.NextSingle() - 0.5f) * ball.Speed);
            });

            // Reset paddle positions
            var paddleQuery = new QueryDescription().WithAll<Paddle, Transform>();
            World.Query(in paddleQuery, (ref Paddle paddle, ref Transform transform) =>
            {
                if (paddle.IsPlayer)
                {
                    transform.Position = new Vector2(50f, SCREEN_HEIGHT / 2 - 50f);
                }
                else
                {
                    transform.Position = new Vector2(SCREEN_WIDTH - 70f, SCREEN_HEIGHT / 2 - 50f);
                }
            });

            // Publish restart event for score system
            EventBus.Publish(new GameRestartEvent());
        }
    }
}
