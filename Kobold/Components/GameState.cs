using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    public enum GameStateType
    {
        Playing,
        GameOver,
        Paused
    }

    public struct GameState : IEquatable<GameState>
    {
        public GameStateType State;
        public string Message;
        public int WinningPlayerId;

        public GameState(GameStateType state, string message = "", int winningPlayerId = 0)
        {
            State = state;
            Message = message;
            WinningPlayerId = winningPlayerId;
        }

        public bool IsPlaying => State == GameStateType.Playing;
        public bool IsGameOver => State == GameStateType.GameOver;
        public bool IsPaused => State == GameStateType.Paused;

        // Override ToString to provide consistent string representation for state management
        public override string ToString()
        {
            return State.ToString(); // Only use the main state type for string representation
        }

        // Implement IEquatable for better comparison
        public bool Equals(GameState other)
        {
            return State == other.State;
        }

        public override bool Equals(object obj)
        {
            return obj is GameState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }

        public static bool operator ==(GameState left, GameState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameState left, GameState right)
        {
            return !left.Equals(right);
        }
    }
}
