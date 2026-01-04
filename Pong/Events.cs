using Kobold.Extensions.Physics.Systems;
using Kobold.Extensions.Physics.Components;
using Kobold.Extensions.Collision.Systems;
using Kobold.Extensions.Collision.Components;
using Kobold.Extensions.Input.Systems;
using Kobold.Extensions.Input.Components;
using Kobold.Extensions.Boundaries.Systems;
using Kobold.Extensions.Boundaries.Components;
using Kobold.Extensions.Triggers.Systems;
using Kobold.Extensions.Destruction.Systems;
using Kobold.Extensions.Destruction.Components;
using Kobold.Extensions.Gameplay.Components;
using Kobold.Extensions.GameState.Systems;
﻿using Kobold.Core.Events;
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
