using Kobold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
    public class PlayerScoredEvent : BaseEvent
    {
        public int PlayerId { get; }
        public int NewScore { get; }

        public PlayerScoredEvent(int playerId, int newScore)
        {
            PlayerId = playerId;
            NewScore = newScore;
        }
    }

    public class BallResetEvent : BaseEvent
    {
        public float InitialSpeedX { get; }
        public float InitialSpeedY { get; }

        public BallResetEvent(float initialSpeedX, float initialSpeedY)
        {
            InitialSpeedX = initialSpeedX;
            InitialSpeedY = initialSpeedY;
        }
    }

    public class GameOverEvent : BaseEvent
    {
        public int WinningPlayerId { get; }
        public int WinnerScore { get; }
        public int LoserScore { get; }

        public GameOverEvent(int winningPlayerId, int winnerScore, int loserScore)
        {
            WinningPlayerId = winningPlayerId;
            WinnerScore = winnerScore;
            LoserScore = loserScore;
        }
    }
}
