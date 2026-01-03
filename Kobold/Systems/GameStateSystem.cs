using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Input;
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
    /// Generic game state management system. Configure states with callbacks and an input that should trigger a transition out of this state
    /// if applicable. Then calling ChangeState will handle performing the callbacks and publishing a GameStateChangedEvent.
    ///
    /// DEPRECATED: This system is deprecated in favor of GameStateManager&lt;TState&gt; from Kobold.Core.Services.
    /// The new service-based approach provides better performance (no ECS queries for singleton state)
    /// and clearer semantics. Input handling should be done in a dedicated input system that calls
    /// the GameStateManager to change states.
    /// </summary>
    [Obsolete("Use GameStateManager<TState> from Kobold.Core.Services instead. Handle input transitions in a dedicated input system.")]
    public class GameStateSystem<TGameState> : ISystem where TGameState : struct
    {
        protected readonly World World;
        protected readonly EventBus EventBus;
        protected readonly IInputManager InputManager;
        private readonly Dictionary<string, StateConfig> _stateConfigs = new();

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

        public void ConfigureState(TGameState state, StateConfig config)
        {
            string stateKey = state.ToString();
            _stateConfigs[stateKey] = config;
        }

        public void ChangeState(TGameState newState)
        {
            var previousState = GetCurrentState();

            // Update the game state entity
            var gameStateQuery = new QueryDescription().WithAll<TGameState>();
            World.Query(in gameStateQuery, (ref TGameState gameState) =>
            {
                gameState = newState;
            });

            // Handle state change logic
            HandleStateChange(newState, previousState);
        }

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
            string stateKey = currentState.ToString();

            if (_stateConfigs.TryGetValue(stateKey, out var config))
            {
                foreach (var transition in config.InputTransitions)
                {
                    if (InputManager.IsKeyPressed(transition.Key))
                    {
                        if (transition.NextState is TGameState nextState)
                        {
                            ChangeState(nextState);
                            transition.OnTransition?.Invoke();
                            break;
                        }
                    }
                }
            }
        }

        protected virtual void HandleStateChange(TGameState newState, TGameState previousState)
        {
            string previousStateKey = previousState.ToString();
            string newStateKey = newState.ToString();

            // Call previous state exit callback
            if (_stateConfigs.TryGetValue(previousStateKey, out var previousConfig))
            {
                previousConfig.OnStateExit?.Invoke();
            }

            // Call new state enter callback
            if (_stateConfigs.TryGetValue(newStateKey, out var config))
            {
                config.OnStateEnter?.Invoke();
            }

            // Publish state change event
            EventBus.Publish(new GameStateChangedEvent<TGameState>(previousState, newState));
        }
    }

    public class StateConfig
    {
        public List<InputTransition> InputTransitions { get; set; } = new();
        public Action OnStateEnter { get; set; }
        public Action OnStateExit { get; set; }
    }

    public class InputTransition
    {
        public KeyCode Key { get; set; }
        public object NextState { get; set; }
        public Action OnTransition { get; set; }
    }

}
