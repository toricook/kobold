using System.Numerics;

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