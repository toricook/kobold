using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using Pong.Components;
using System.Numerics;

namespace Pong.Systems
{
    public class AISystem : ISystem
    {
        private readonly World _world;

        public AISystem(World world)
        {
            _world = world;
        }

        public void Update(float deltaTime)
        {
            // Find the ball
            Vector2 ballPosition = Vector2.Zero;
            var ballQuery = new QueryDescription().WithAll<Ball, Transform>();
            _world.Query(in ballQuery, (ref Ball ball, ref Transform transform) =>
            {
                ballPosition = transform.Position;
            });

            // Move AI paddle towards ball
            var paddleQuery = new QueryDescription().WithAll<Paddle, Transform, Velocity>();
            _world.Query(in paddleQuery, (ref Paddle paddle, ref Transform transform, ref Velocity velocity) =>
            {
                if (paddle.IsPlayer) return;

                float difference = ballPosition.Y - transform.Position.Y;

                if (Math.Abs(difference) > 10f) // Dead zone to prevent jittering
                {
                    velocity.Value = new Vector2(0, Math.Sign(difference) * paddle.Speed * 0.8f); // AI is slightly slower
                }
                else
                {
                    velocity.Value = Vector2.Zero;
                }
            });
        }
    }
}