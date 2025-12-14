using Arch.Core;
using Kobold.Core.Abstractions.Input;
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
    /// <summary>
    /// Handles Pong-specific game state transitions and game logic
    /// Pure state management - no UI concerns
    /// </summary>
    public class PongGameStateSystem : GameStateSystem<CoreGameState>, IEventHandler<GameOverEvent>
    {
        private const float SCREEN_WIDTH = 800f;
        private const float SCREEN_HEIGHT = 600f;

        public PongGameStateSystem(World world, EventBus eventBus, IInputManager inputManager)
            : base(world, eventBus, inputManager)
        {
            ConfigureGameStates();
            EventBus.Subscribe<GameOverEvent>(this);
        }

        private void ConfigureGameStates()
        {
            // Configure Game Over state
            ConfigureState(new CoreGameState(StandardGameState.GameOver), new StateConfig
            {
                InputTransitions = new List<InputTransition>
                {
                    new InputTransition
                    {
                        Key = KeyCode.Space,
                        NextState = new CoreGameState(StandardGameState.Playing),
                        OnTransition = RestartGame
                    },
                },
                OnStateEnter = OnGameOverEnter,
                OnStateExit = OnGameOverExit
            });

            // Configure Playing state
            ConfigureState(new CoreGameState(StandardGameState.Playing), new StateConfig
            {
                InputTransitions = new List<InputTransition>
                {
                    new InputTransition
                    {
                        Key = KeyCode.P,
                        NextState = new CoreGameState(StandardGameState.Paused),
                        OnTransition = () => Console.WriteLine("Game paused")
                    }
                },
                OnStateEnter = OnPlayingEnter,
                OnStateExit = OnPlayingExit
            });

            // Configure Paused state
            ConfigureState(new CoreGameState(StandardGameState.Paused), new StateConfig
            {
                InputTransitions = new List<InputTransition>
                {
                    new InputTransition
                    {
                        Key = KeyCode.Space,
                        NextState = new CoreGameState(StandardGameState.Playing),
                        OnTransition = () => Console.WriteLine("Game resumed with space")
                    }
                },
                OnStateEnter = OnPausedEnter,
                OnStateExit = OnPausedExit
            });
        }

        // Event handler for game over events from other systems
        public void Handle(GameOverEvent eventData)
        { 
            // Create game over state with winner information
            var gameOverState = new CoreGameState(
                StandardGameState.GameOver,
                $"Player {eventData.WinningPlayerId} Wins! {eventData.WinnerScore}-{eventData.LoserScore}"
            );

            ChangeState(gameOverState);
        }

        // State enter/exit callbacks
        private void OnGameOverEnter()
        {

            // Remove the ball's renderer so it's invisible during game over (in a more complex game this should be handled in a different system maybe)
            var ballQuery = new QueryDescription().WithAll<Ball, RectangleRenderer>();
            World.Query(in ballQuery, (Entity entity, ref Ball ball, ref RectangleRenderer renderer) =>
            {
                // Store the renderer for later restoration (optional, see below)
                World.Remove<RectangleRenderer>(entity);
            });
        }

        private void OnGameOverExit()
        {

            // Restore the ball's renderer
            var ballQuery = new QueryDescription().WithAll<Ball>().WithNone<RectangleRenderer>();
            World.Query(in ballQuery, (Entity entity) =>
            {
                // Add the renderer back
                var ballRenderer = RectangleRenderer.GameObject(new Vector2(20f, 20f), Color.White);
                World.Add(entity, ballRenderer);
                Console.WriteLine($"Added renderer back to ball entity: {entity}");
            });
        }

        private void OnPlayingEnter()
        {
            Console.WriteLine("Entering Playing state");
            // Game is now active - could start background music, etc.
        }

        private void OnPlayingExit()
        {
            Console.WriteLine("Exiting Playing state");
            // Could pause music, save state, etc.
        }

        private void OnPausedEnter()
        {
            Console.WriteLine("Entering Paused state");
            // Could pause music, dim screen, etc.
        }

        private void OnPausedExit()
        {
            Console.WriteLine("Exiting Paused state");
            // Resume game logic
        }

        // Game restart logic - resets all game entities to initial state
        private void RestartGame()
        {
            Console.WriteLine("PongGameStateSystem: Restarting game...");

            ResetBall();
            ResetPaddles();

            // Notify other systems that the game is restarting
            EventBus.Publish(new GameRestartEvent());
        }

        private void ResetBall()
        {
            var ballQuery = new QueryDescription().WithAll<Ball, Transform, Velocity>();
            World.Query(in ballQuery, (ref Ball ball, ref Transform transform, ref Velocity velocity) =>
            {
                // Reset to center
                transform.Position = new Vector2(SCREEN_WIDTH / 2, SCREEN_HEIGHT / 2);

                // Random direction but ensure it's not too vertical
                var angle = (Random.Shared.NextSingle() - 0.5f) * 0.5f; // -0.25 to 0.25 radians
                var direction = Random.Shared.Next(2) == 0 ? -1 : 1; // Left or right

                velocity.Value = new Vector2(
                    direction * ball.Speed * MathF.Cos(angle),
                    ball.Speed * MathF.Sin(angle)
                );

                Console.WriteLine($"Ball reset: position={transform.Position}, velocity={velocity.Value}");
            });
        }

        private void ResetPaddles()
        {
            var paddleQuery = new QueryDescription().WithAll<Paddle, Transform, Velocity>();
            World.Query(in paddleQuery, (ref Paddle paddle, ref Transform transform, ref Velocity velocity) =>
            {
                // Stop paddle movement
                velocity.Value = Vector2.Zero;

                // Reset positions
                if (paddle.IsPlayer)
                {
                    transform.Position = new Vector2(50f, SCREEN_HEIGHT / 2 - 50f);
                    Console.WriteLine("Player paddle reset");
                }
                else
                {
                    transform.Position = new Vector2(SCREEN_WIDTH - 70f, SCREEN_HEIGHT / 2 - 50f);
                    Console.WriteLine("AI paddle reset");
                }
            });
        }

        // Additional helper methods for game state queries
        public bool IsGamePlaying()
        {
            var currentState = GetCurrentState();
            return currentState.IsPlaying;
        }

        public bool IsGameOver()
        {
            var currentState = GetCurrentState();
            return currentState.IsGameOver;
        }

        public bool IsGamePaused()
        {
            var currentState = GetCurrentState();
            return currentState.IsPaused;
        }

        // Method to force a state change (could be useful for debugging or special events)
        public void ForceGameOver(int winningPlayerId, string message = "")
        {
            var gameOverState = new CoreGameState(StandardGameState.GameOver, message);
            ChangeState(gameOverState);
        }

        public void ForcePause()
        {
            if (IsGamePlaying())
            {
                ChangeState(new CoreGameState(StandardGameState.Paused));
            }
        }

        public void ForceResume()
        {
            if (IsGamePaused())
            {
                ChangeState(new CoreGameState(StandardGameState.Playing));
            }
        }
    }
}
