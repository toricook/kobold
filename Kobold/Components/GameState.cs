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

    public struct GameState
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
    }
}
