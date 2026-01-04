using Kobold.Core.Events;

namespace Kobold.Extensions.GameState.Events
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
