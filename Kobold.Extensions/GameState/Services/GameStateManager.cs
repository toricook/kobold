using Kobold.Core.Events;
using Kobold.Extensions.GameState.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kobold.Extensions.GameState.Services
{
    /// <summary>
    /// Manages game state as a service rather than an ECS component.
    /// Provides efficient access to the current game state and handles state transitions
    /// with callbacks and event publishing.
    /// </summary>
    /// <typeparam name="TState">The state enum type (e.g., StandardGameState, PongGameState)</typeparam>
    public class GameStateManager<TState> where TState : struct, Enum
    {
        private readonly EventBus _eventBus;
        private readonly Dictionary<string, StateConfiguration> _stateConfigs = new();

        private TState _currentState;
        private string _currentMessage;
        private Dictionary<string, object> _stateData;
        private DateTime _stateEnteredAt;

        /// <summary>
        /// The current state of the game.
        /// </summary>
        public TState CurrentState => _currentState;

        /// <summary>
        /// Optional message associated with the current state.
        /// </summary>
        public string Message => _currentMessage;

        /// <summary>
        /// Additional contextual data for the current state.
        /// </summary>
        public IReadOnlyDictionary<string, object> StateData => _stateData;

        /// <summary>
        /// Timestamp when the current state was entered.
        /// </summary>
        public DateTime StateEnteredAt => _stateEnteredAt;

        /// <summary>
        /// Creates a new GameStateManager with the specified initial state.
        /// </summary>
        /// <param name="eventBus">Event bus for publishing state change events</param>
        /// <param name="initialState">The initial state to start in</param>
        /// <param name="initialMessage">Optional initial message</param>
        public GameStateManager(EventBus eventBus, TState initialState, string initialMessage = "")
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _currentState = initialState;
            _currentMessage = initialMessage ?? "";
            _stateData = new Dictionary<string, object>();
            _stateEnteredAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Configures a state with callbacks that will be invoked on state enter/exit.
        /// </summary>
        /// <param name="state">The state to configure</param>
        /// <param name="config">Configuration with callbacks</param>
        public void ConfigureState(TState state, StateConfiguration config)
        {
            string stateKey = state.ToString();
            _stateConfigs[stateKey] = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Changes to a new state, invoking callbacks and publishing events.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <param name="message">Optional message for the new state</param>
        /// <param name="preserveStateData">Whether to preserve state data across the transition</param>
        public void ChangeState(TState newState, string message = null, bool preserveStateData = false)
        {
            var previousState = _currentState;

            // Don't do anything if we're already in this state (unless message changed)
            if (previousState.Equals(newState) && message == null)
                return;

            // Call previous state exit callback
            string previousStateKey = previousState.ToString();
            if (_stateConfigs.TryGetValue(previousStateKey, out var previousConfig))
            {
                previousConfig.OnStateExit?.Invoke();
            }

            // Update state
            _currentState = newState;
            _currentMessage = message ?? "";
            _stateEnteredAt = DateTime.UtcNow;

            if (!preserveStateData)
            {
                _stateData = new Dictionary<string, object>();
            }

            // Call new state enter callback
            string newStateKey = newState.ToString();
            if (_stateConfigs.TryGetValue(newStateKey, out var newConfig))
            {
                newConfig.OnStateEnter?.Invoke();
            }

            // Publish state change event
            _eventBus.Publish(new GameStateChangedEvent<TState>(previousState, newState));
        }

        /// <summary>
        /// Gets a typed value from the StateData dictionary.
        /// Returns the default value for the type if the key doesn't exist or can't be cast.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to</typeparam>
        /// <param name="key">The key to look up</param>
        /// <returns>The typed value or default(T)</returns>
        public T GetStateData<T>(string key)
        {
            if (_stateData != null && _stateData.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;

                // Try to convert if possible
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    // Conversion failed, return default
                }
            }
            return default(T);
        }

        /// <summary>
        /// Sets a value in the StateData dictionary.
        /// </summary>
        /// <param name="key">The key to set</param>
        /// <param name="value">The value to store</param>
        public void SetStateData(string key, object value)
        {
            _stateData ??= new Dictionary<string, object>();
            _stateData[key] = value;
        }

        /// <summary>
        /// Gets how long the game has been in the current state.
        /// </summary>
        /// <returns>Time elapsed since entering the current state</returns>
        public TimeSpan GetTimeInState()
        {
            return DateTime.UtcNow - _stateEnteredAt;
        }

        /// <summary>
        /// Updates the message for the current state without triggering a state change.
        /// </summary>
        /// <param name="newMessage">The new message</param>
        public void UpdateMessage(string newMessage)
        {
            _currentMessage = newMessage ?? "";
        }

        /// <summary>
        /// Checks if the current state matches the specified state.
        /// </summary>
        /// <param name="state">The state to check against</param>
        /// <returns>True if current state matches</returns>
        public bool IsInState(TState state)
        {
            return _currentState.Equals(state);
        }

        /// <summary>
        /// Returns the current state as a string for debugging and logging.
        /// </summary>
        public override string ToString()
        {
            return _currentState.ToString();
        }

        /// <summary>
        /// Returns a detailed string representation including message and state data.
        /// Useful for debugging and detailed logging.
        /// </summary>
        public string ToDetailedString()
        {
            var details = $"GameState: {_currentState}";
            if (!string.IsNullOrEmpty(_currentMessage))
                details += $", Message: '{_currentMessage}'";
            if (_stateData != null && _stateData.Count > 0)
                details += $", Data: [{string.Join(", ", _stateData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}]";
            details += $", Duration: {GetTimeInState().TotalSeconds:F1}s";
            return details;
        }
    }

    /// <summary>
    /// Configuration for a game state, including callbacks for state enter/exit.
    /// </summary>
    public class StateConfiguration
    {
        /// <summary>
        /// Called when entering this state.
        /// </summary>
        public Action OnStateEnter { get; set; }

        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        public Action OnStateExit { get; set; }
    }

    /// <summary>
    /// Convenience wrapper for games that use the standard core game states.
    /// Provides strongly-typed state checks and transitions.
    /// </summary>
    public class CoreGameStateManager
    {
        private readonly GameStateManager<StandardGameState> _inner;

        public StandardGameState CurrentState => _inner.CurrentState;
        public string Message => _inner.Message;
        public IReadOnlyDictionary<string, object> StateData => _inner.StateData;
        public DateTime StateEnteredAt => _inner.StateEnteredAt;

        public CoreGameStateManager(EventBus eventBus, StandardGameState initialState = StandardGameState.Menu, string initialMessage = "")
        {
            _inner = new GameStateManager<StandardGameState>(eventBus, initialState, initialMessage);
        }

        public void ConfigureState(StandardGameState state, StateConfiguration config)
            => _inner.ConfigureState(state, config);

        public void ChangeState(StandardGameState newState, string message = null, bool preserveStateData = false)
            => _inner.ChangeState(newState, message, preserveStateData);

        public T GetStateData<T>(string key) => _inner.GetStateData<T>(key);
        public void SetStateData(string key, object value) => _inner.SetStateData(key, value);
        public TimeSpan GetTimeInState() => _inner.GetTimeInState();
        public void UpdateMessage(string newMessage) => _inner.UpdateMessage(newMessage);
        public bool IsInState(StandardGameState state) => _inner.IsInState(state);

        // Convenience properties for common state checks
        public bool IsPlaying => CurrentState == StandardGameState.Playing;
        public bool IsPaused => CurrentState == StandardGameState.Paused;
        public bool IsGameOver => CurrentState == StandardGameState.GameOver;
        public bool IsLoading => CurrentState == StandardGameState.Loading;
        public bool IsInMenu => CurrentState == StandardGameState.Menu;

        public override string ToString() => _inner.ToString();
        public string ToDetailedString() => _inner.ToDetailedString();
    }

    /// <summary>
    /// Standard game states that most games will need.
    /// Games can use this directly or create their own enum with these values included.
    /// </summary>
    public enum StandardGameState
    {
        /// <summary>
        /// Game is actively running - all gameplay systems should be active.
        /// This is the main game loop state where players can interact and things happen.
        /// </summary>
        Playing,

        /// <summary>
        /// Game is temporarily stopped but can be resumed.
        /// Only UI and input systems should be active to handle unpause input.
        /// Game world state is preserved.
        /// </summary>
        Paused,

        /// <summary>
        /// Game session has ended (win/lose condition met).
        /// Only UI systems should be active to show final state and restart options.
        /// Game world state may be reset or preserved for restart.
        /// </summary>
        GameOver,

        /// <summary>
        /// Game is loading resources or initializing.
        /// Typically only loading/progress systems are active.
        /// Transitions to Playing when ready.
        /// </summary>
        Loading,

        /// <summary>
        /// Showing main menu or game selection screens.
        /// Only UI and menu systems are active.
        /// No gameplay systems should run.
        /// </summary>
        Menu
    }
}
