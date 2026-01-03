using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Core game state component that represents the current high-level state of the game.
    /// This is used by the SystemManager to determine which systems should be active,
    /// and by game-specific systems to control flow and behavior.
    ///
    /// The core engine provides basic states (Playing, Paused, GameOver), but games
    /// can extend this with their own state enums by implementing IGameState.
    ///
    /// Only one GameState component should exist per game - it's typically attached
    /// to a singleton entity that represents the overall game session.
    ///
    /// DEPRECATED: This component-based approach to game state is deprecated.
    /// Use GameStateManager&lt;TState&gt; from Kobold.Core.Services instead, which provides
    /// better performance and clearer semantics for singleton game state.
    /// See: Kobold.Core.Services.GameStateManager
    /// </summary>
    /// <typeparam name="TState">The state enum type (e.g., PongGameState, SpaceInvadersState)</typeparam>
    [Obsolete("Use GameStateManager<TState> from Kobold.Core.Services instead. This component-based approach forces singleton state into the ECS model unnecessarily.")]
    public struct GameState<TState> : IEquatable<GameState<TState>> where TState : struct, Enum
    {
        /// <summary>
        /// The current state of the game. This determines which systems are active
        /// and how the game behaves. For example:
        /// - Playing: All gameplay systems active
        /// - Paused: Only UI and input systems active
        /// - GameOver: Only UI systems active, waiting for restart
        /// </summary>
        public TState State { get; set; }

        /// <summary>
        /// Optional message associated with the current state.
        /// Examples:
        /// - "Player 1 Wins!" for GameOver state
        /// - "Wave 3 Starting..." for Playing state
        /// - "Game Paused" for Paused state
        /// 
        /// Used primarily by UI systems for displaying state-specific text.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Additional contextual data for the current state.
        /// This is a flexible dictionary for state-specific information that doesn't
        /// warrant its own field. Examples:
        /// - "WinningPlayerId" -> "1" for GameOver state
        /// - "WaveNumber" -> "5" for Playing state
        /// - "PauseReason" -> "PlayerInput" vs "LostFocus" for Paused state
        /// </summary>
        public Dictionary<string, object> StateData { get; set; }

        /// <summary>
        /// Timestamp when this state was entered.
        /// Useful for tracking how long the game has been in a particular state,
        /// implementing time-based state transitions, or analytics.
        /// </summary>
        public DateTime StateEnteredAt { get; set; }

        /// <summary>
        /// Creates a new GameState with the specified state and optional message.
        /// </summary>
        /// <param name="state">The game state enum value</param>
        /// <param name="message">Optional descriptive message</param>
        /// <param name="stateData">Optional additional state data</param>
        public GameState(TState state, string message = "", Dictionary<string, object> stateData = null)
        {
            State = state;
            Message = message ?? "";
            StateData = stateData ?? new Dictionary<string, object>();
            StateEnteredAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets a typed value from the StateData dictionary.
        /// Returns the default value for the type if the key doesn't exist or can't be cast.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to</typeparam>
        /// <param name="key">The key to look up</param>
        /// <returns>The typed value or default(T)</returns>
        public readonly T GetStateData<T>(string key)
        {
            if (StateData != null && StateData.TryGetValue(key, out var value))
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
            StateData ??= new Dictionary<string, object>();
            StateData[key] = value;
        }

        /// <summary>
        /// Gets how long the game has been in this current state.
        /// </summary>
        /// <returns>Time elapsed since entering this state</returns>
        public readonly TimeSpan GetTimeInState()
        {
            return DateTime.UtcNow - StateEnteredAt;
        }

        /// <summary>
        /// Creates a new GameState with a different state but preserving other data.
        /// Useful for state transitions where you want to keep context.
        /// </summary>
        /// <param name="newState">The new state to transition to</param>
        /// <param name="newMessage">Optional new message (null keeps current message)</param>
        /// <returns>New GameState with updated state</returns>
        public readonly GameState<TState> WithState(TState newState, string newMessage = null)
        {
            return new GameState<TState>(newState, newMessage ?? Message, new Dictionary<string, object>(StateData ?? new Dictionary<string, object>()));
        }

        /// <summary>
        /// Creates a new GameState with updated message but same state.
        /// </summary>
        /// <param name="newMessage">The new message</param>
        /// <returns>New GameState with updated message</returns>
        public readonly GameState<TState> WithMessage(string newMessage)
        {
            return new GameState<TState>(State, newMessage, StateData);
        }

        // Implement IEquatable for proper comparison (used by SystemManager and state transitions)
        public readonly bool Equals(GameState<TState> other)
        {
            return State.Equals(other.State);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is GameState<TState> other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return State.GetHashCode();
        }

        public static bool operator ==(GameState<TState> left, GameState<TState> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameState<TState> left, GameState<TState> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the state as a string for debugging and logging.
        /// Only uses the State enum for string representation to ensure consistency.
        /// </summary>
        public override readonly string ToString()
        {
            return State.ToString();
        }

        /// <summary>
        /// Returns a detailed string representation including message and state data.
        /// Useful for debugging and detailed logging.
        /// </summary>
        public readonly string ToDetailedString()
        {
            var details = $"GameState: {State}";
            if (!string.IsNullOrEmpty(Message))
                details += $", Message: '{Message}'";
            if (StateData != null && StateData.Count > 0)
                details += $", Data: [{string.Join(", ", StateData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}]";
            details += $", Duration: {GetTimeInState().TotalSeconds:F1}s";
            return details;
        }
    }

    /// <summary>
    /// Standard game states that most games will need.
    /// Games can use this directly or create their own enum with these values included.
    ///
    /// DEPRECATED: This enum is deprecated. Use StandardGameState from Kobold.Core.Services instead.
    /// </summary>
    [Obsolete("Use StandardGameState from Kobold.Core.Services instead.")]
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

    /// <summary>
    /// Convenience wrapper for games that use the standard core game states.
    /// This provides a simpler API than the full generic GameState&lt;TState&gt; for common cases.
    /// Most simple games can just use this instead of defining their own state enum.
    ///
    /// DEPRECATED: This component wrapper is deprecated. Use CoreGameStateManager from Kobold.Core.Services instead.
    /// </summary>
    [Obsolete("Use CoreGameStateManager from Kobold.Core.Services instead.")]
    public struct CoreGameState : IEquatable<CoreGameState>
    {
        private readonly GameState<StandardGameState> _inner;

        public StandardGameState State => _inner.State;
        public string Message => _inner.Message;
        public Dictionary<string, object> StateData => _inner.StateData;
        public DateTime StateEnteredAt => _inner.StateEnteredAt;

        public CoreGameState(StandardGameState state, string message = "", Dictionary<string, object> stateData = null)
        {
            _inner = new GameState<StandardGameState>(state, message, stateData);
        }

        // Convenience properties for common state checks
        public readonly bool IsPlaying => State == StandardGameState.Playing;
        public readonly bool IsPaused => State == StandardGameState.Paused;
        public readonly bool IsGameOver => State == StandardGameState.GameOver;
        public readonly bool IsLoading => State == StandardGameState.Loading;
        public readonly bool IsInMenu => State == StandardGameState.Menu;

        // Forward all methods to the inner generic type
        public readonly T GetStateData<T>(string key) => _inner.GetStateData<T>(key);
        public void SetStateData(string key, object value) => _inner.SetStateData(key, value);
        public readonly TimeSpan GetTimeInState() => _inner.GetTimeInState();
        public readonly CoreGameState WithState(StandardGameState newState, string newMessage = null)
            => new CoreGameState(newState, newMessage ?? Message, StateData);
        public readonly CoreGameState WithMessage(string newMessage)
            => new CoreGameState(State, newMessage, StateData);

        public readonly bool Equals(CoreGameState other) => _inner.Equals(other._inner);
        public override readonly bool Equals(object obj) => obj is CoreGameState other && Equals(other);
        public override readonly int GetHashCode() => _inner.GetHashCode();
        public override readonly string ToString() => _inner.ToString();
        public readonly string ToDetailedString() => _inner.ToDetailedString();

        public static bool operator ==(CoreGameState left, CoreGameState right) => left.Equals(right);
        public static bool operator !=(CoreGameState left, CoreGameState right) => !left.Equals(right);

        // Implicit conversions for convenience
        public static implicit operator CoreGameState(StandardGameState state) => new CoreGameState(state);
        public static implicit operator StandardGameState(CoreGameState gameState) => gameState.State;
    }
}