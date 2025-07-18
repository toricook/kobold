using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    /// <summary>
    /// Generic game state management system
    /// </summary>
    public class GameStateSystem<TGameState> : ISystem where TGameState : struct
    {
        protected readonly World World;
        protected readonly EventBus EventBus;
        protected readonly IInputManager InputManager;
        private readonly Dictionary<TGameState, GameStateConfig> _stateConfigs = new();
        private readonly List<Entity> _currentUIEntities = new();

        public GameStateSystem(World world, EventBus eventBus, IInputManager inputManager)
        {
            World = world;
            EventBus = eventBus;
            InputManager = inputManager;
        }

        public virtual void Update(float deltaTime)
        {
            HandleStateTransitionInputs();
        }

        /// <summary>
        /// Configure a game state with UI and input handling
        /// </summary>
        public void ConfigureState(TGameState state, GameStateConfig config)
        {
            _stateConfigs[state] = config;
        }

        /// <summary>
        /// Change to a new game state
        /// </summary>
        public void ChangeState(TGameState newState)
        {
            // Update the game state entity
            var gameStateQuery = new QueryDescription().WithAll<TGameState>();
            World.Query(in gameStateQuery, (ref TGameState gameState) =>
            {
                gameState = newState;
            });

            // Handle UI changes
            HandleStateChange(newState);
        }

        /// <summary>
        /// Get the current game state
        /// </summary>
        public TGameState GetCurrentState()
        {
            TGameState currentState = default;
            var gameStateQuery = new QueryDescription().WithAll<TGameState>();
            World.Query(in gameStateQuery, (ref TGameState gameState) =>
            {
                currentState = gameState;
            });
            return currentState;
        }

        protected virtual void HandleStateTransitionInputs()
        {
            var currentState = GetCurrentState();

            if (_stateConfigs.TryGetValue(currentState, out var config))
            {
                // Check for input-based state transitions
                foreach (var transition in config.InputTransitions)
                {
                    if (InputManager.IsKeyPressed(transition.Key))
                    {
                        ChangeState((TGameState)transition.NextState);
                        transition.OnTransition?.Invoke();
                        break;
                    }
                }
            }
        }

        protected virtual void HandleStateChange(TGameState newState)
        {
            // Clear current UI
            ClearCurrentUI();

            // Show new UI if configured
            if (_stateConfigs.TryGetValue(newState, out var config))
            {
                foreach (var uiElement in config.UIElements)
                {
                    var entity = World.Create(uiElement.Components.ToArray());
                    _currentUIEntities.Add(entity);
                }

                // Call state enter callback
                config.OnStateEnter?.Invoke();
            }
        }

        protected void ClearCurrentUI()
        {
            foreach (var entity in _currentUIEntities)
            {
                World.Destroy(entity);
            }
            _currentUIEntities.Clear();
        }

        /// <summary>
        /// Stop all movement (common for pause/game over states)
        /// </summary>
        protected void StopAllMovement()
        {
            var velocityQuery = new QueryDescription().WithAll<Velocity>();
            World.Query(in velocityQuery, (ref Velocity velocity) =>
            {
                velocity.Value = System.Numerics.Vector2.Zero;
            });
        }
    }

    /// <summary>
    /// Configuration for a game state
    /// </summary>
    public class GameStateConfig
    {
        public List<UIElementConfig> UIElements { get; set; } = new();
        public List<InputTransition> InputTransitions { get; set; } = new();
        public Action OnStateEnter { get; set; }
        public Action OnStateExit { get; set; }
    }

    /// <summary>
    /// Configuration for a UI element to show in a state
    /// </summary>
    public class UIElementConfig
    {
        public List<object> Components { get; set; } = new();
    }

    /// <summary>
    /// Input-based state transition
    /// </summary>
    public class InputTransition
    {
        public KeyCode Key { get; set; }
        public object NextState { get; set; } // Generic to work with any state type
        public Action OnTransition { get; set; }
    }

}
