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
﻿using System.Numerics;

namespace Pong.Components
{
    public struct Paddle
    {
        public float Speed;
        public bool IsPlayer; // true for player, false for AI

        public Paddle(float speed, bool isPlayer)
        {
            Speed = speed;
            IsPlayer = isPlayer;
        }
    }

    public struct Ball
    {
        public float Speed;

        public Ball(float speed)
        {
            Speed = speed;
        }
    }

    public struct Score
    {
        public int PlayerScore;
        public int AIScore;

        public Score(int playerScore = 0, int aiScore = 0)
        {
            PlayerScore = playerScore;
            AIScore = aiScore;
        }
    }
}