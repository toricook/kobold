using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Components;
using Pong.Components;
using System.Numerics;

namespace Pong.Systems
{
    public class CollisionSystem
    {
        private readonly World _world;
        private readonly float _screenWidth;
        private readonly float _screenHeight;

        public CollisionSystem(World world, float screenWidth, float screenHeight)
        {
            _world = world;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public void Update(float deltaTime)
        {
            HandleBallWallCollisions();
            HandleBallPaddleCollisions();
            HandlePaddleBounds();
        }

        private void HandleBallWallCollisions()
        {
            var ballQuery = new QueryDescription().WithAll<Ball, Transform, Velocity, BoxCollider>();

            _world.Query(in ballQuery, (Entity entity, ref Ball ball, ref Transform transform, ref Velocity velocity, ref BoxCollider collider) =>
            {
                // Top and bottom walls
                if (transform.Position.Y <= 0 || transform.Position.Y + collider.Size.Y >= _screenHeight)
                {
                    velocity.Value = new Vector2(velocity.Value.X, -velocity.Value.Y);
                    transform.Position = new Vector2(transform.Position.X,
                        Math.Clamp(transform.Position.Y, 0, _screenHeight - collider.Size.Y));
                }

                // Left and right walls (scoring)
                if (transform.Position.X <= 0 || transform.Position.X >= _screenWidth)
                {
                    // Reset ball to center
                    transform.Position = new Vector2(_screenWidth / 2, _screenHeight / 2);
                    velocity.Value = new Vector2(-Math.Sign(velocity.Value.X) * ball.Speed,
                        (Random.Shared.NextSingle() - 0.5f) * ball.Speed);

                    // Update score (you'd implement this)
                }
            });
        }

        private void HandleBallPaddleCollisions()
        {
            // First, collect all paddle data
            var paddles = new List<(Vector2 position, Vector2 size)>();
            var paddleQuery = new QueryDescription().WithAll<Paddle, Transform, BoxCollider>();

            _world.Query(in paddleQuery, (ref Paddle paddle, ref Transform paddleTransform, ref BoxCollider paddleCollider) =>
            {
                paddles.Add((paddleTransform.Position, paddleCollider.Size));
            });

            // Then handle ball collisions with each paddle
            var ballQuery = new QueryDescription().WithAll<Ball, Transform, Velocity, BoxCollider>();

            _world.Query(in ballQuery, (ref Ball ball, ref Transform ballTransform, ref Velocity ballVelocity, ref BoxCollider ballCollider) =>
            {
                foreach (var (paddlePos, paddleSize) in paddles)
                {
                    if (IsColliding(ballTransform.Position, ballCollider.Size, paddlePos, paddleSize))
                    {
                        // Simple collision response - reverse X velocity
                        ballVelocity.Value = new Vector2(-ballVelocity.Value.X, ballVelocity.Value.Y);

                        // Move ball out of paddle
                        if (ballTransform.Position.X < paddlePos.X)
                        {
                            ballTransform.Position = new Vector2(paddlePos.X - ballCollider.Size.X, ballTransform.Position.Y);
                        }
                        else
                        {
                            ballTransform.Position = new Vector2(paddlePos.X + paddleSize.X, ballTransform.Position.Y);
                        }

                        break; // Only handle one collision per frame
                    }
                }
            });
        }

        private void HandlePaddleBounds()
        {
            var paddleQuery = new QueryDescription().WithAll<Paddle, Transform, BoxCollider>();

            _world.Query(in paddleQuery, (ref Paddle paddle, ref Transform transform, ref BoxCollider collider) =>
            {
                transform.Position = new Vector2(transform.Position.X,
                    Math.Clamp(transform.Position.Y, 0, _screenHeight - collider.Size.Y));
            });
        }

        private bool IsColliding(Vector2 pos1, Vector2 size1, Vector2 pos2, Vector2 size2)
        {
            return pos1.X < pos2.X + size2.X &&
                   pos1.X + size1.X > pos2.X &&
                   pos1.Y < pos2.Y + size2.Y &&
                   pos1.Y + size1.Y > pos2.Y;
        }
    }
}