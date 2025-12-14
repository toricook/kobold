using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Events
{
    /// <summary>
    /// Event published when the game state changes
    /// </summary>
    public class GameStateChangedEvent<TGameState> : BaseEvent where TGameState : struct
    {
        public TGameState PreviousState { get; }
        public TGameState NewState { get; }

        public GameStateChangedEvent(TGameState previousState, TGameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }
}
