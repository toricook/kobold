using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Pong.Components;
using System.Numerics;

namespace Pong.Systems
{
    /// <summary>
    /// Handles Pong-specific collision responses (ball-paddle bouncing and scoring)
    /// </summary>
    public class PongCollisionHandler : ISystem, IEventHandler<CollisionEvent>
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly float _screenWidth;
        private readonly float _screenHeight;

        public PongCollisionHandler(World world, EventBus eventBus, float screenWidth, float screenHeight)
        {
            _world = world;
            _eventBus = eventBus;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;

            // Subscribe to collision events from the core collision system
            eventBus.Subscribe<CollisionEvent>(this);
        }

        public void Update(float deltaTime)
        {
            HandleBallWallCollisions();
            HandlePaddleBounds();
        }

        /// <summary>
        /// Handle collision events from the core collision system
        /// </summary>
        public void Handle(CollisionEvent eventData)
        {
            var entity1 = eventData.Entity1;
            var entity2 = eventData.Entity2;

            // Check if this is a ball-paddle collision
            if (IsBallPaddleCollision(entity1, entity2, out var ballEntity, out var paddleEntity))
            {
                HandleBallPaddleCollision(ballEntity, paddleEntity);
            }
        }

        private bool IsBallPaddleCollision(Entity entity1, Entity entity2, out Entity ballEntity, out Entity paddleEntity)
        {
            ballEntity = Entity.Null;
            paddleEntity = Entity.Null;

            if (_world.Has<Ball>(entity1) && _world.Has<Paddle>(entity2))
            {
                ballEntity = entity1;
                paddleEntity = entity2;
                return true;
            }
            else if (_world.Has<Ball>(entity2) && _world.Has<Paddle>(entity1))
            {
                ballEntity = entity2;
                paddleEntity = entity1;
                return true;
            }

            return false;
        }

        private void HandleBallPaddleCollision(Entity ballEntity, Entity paddleEntity)
        {
            ref var ballTransform = ref _world.Get<Transform>(ballEntity);
            ref var ballVelocity = ref _world.Get<Velocity>(ballEntity);
            ref var ballCollider = ref _world.Get<BoxCollider>(ballEntity);

            ref var paddleTransform = ref _world.Get<Transform>(paddleEntity);
            ref var paddleCollider = ref _world.Get<BoxCollider>(paddleEntity);

            // Simple collision response - reverse X velocity
            ballVelocity.Value = new Vector2(-ballVelocity.Value.X, ballVelocity.Value.Y);

            // Move ball out of paddle
            if (ballTransform.Position.X < paddleTransform.Position.X)
            {
                ballTransform.Position = new Vector2(paddleTransform.Position.X - ballCollider.Size.X, ballTransform.Position.Y);
            }
            else
            {
                ballTransform.Position = new Vector2(paddleTransform.Position.X + paddleCollider.Size.X, ballTransform.Position.Y);
            }
        }

        private void HandleBallWallCollisions()
        {
            var ballQuery = new QueryDescription().WithAll<Ball, Transform, Velocity, BoxCollider>();

            _world.Query(in ballQuery, (Entity entity, ref Ball ball, ref Transform transform, ref Velocity velocity, ref BoxCollider collider) =>
            {
                // Top and bottom walls - bounce
                if (transform.Position.Y <= 0 || transform.Position.Y + collider.Size.Y >= _screenHeight)
                {
                    velocity.Value = new Vector2(velocity.Value.X, -velocity.Value.Y);
                    transform.Position = new Vector2(transform.Position.X,
                        Math.Clamp(transform.Position.Y, 0, _screenHeight - collider.Size.Y));
                }

                // Left and right walls (scoring) - stop physics and reset ball
                if (transform.Position.X <= 0)
                {
                    // AI scored (ball went off left side)
                    _eventBus.Publish(new PlayerScoredEvent(playerId: 2, newScore: 0));
                    ResetBall(ref transform, ref velocity, ball.Speed, 1); // Ball goes right
                }
                else if (transform.Position.X >= _screenWidth)
                {
                    // Player scored (ball went off right side)
                    _eventBus.Publish(new PlayerScoredEvent(playerId: 1, newScore: 0));
                    ResetBall(ref transform, ref velocity, ball.Speed, -1); // Ball goes left
                }
            });
        }

        private void ResetBall(ref Transform transform, ref Velocity velocity, float speed, int direction)
        {
            transform.Position = new Vector2(_screenWidth / 2, _screenHeight / 2);
            var newVelocity = new Vector2(direction * speed, (Random.Shared.NextSingle() - 0.5f) * speed);
            velocity.Value = newVelocity;

            _eventBus.Publish(new BallResetEvent(newVelocity.X, newVelocity.Y));
        }

        private void HandlePaddleBounds()
        {
            var paddleQuery = new QueryDescription().WithAll<Paddle, Transform, BoxCollider>();

            _world.Query(in paddleQuery, (ref Paddle paddle, ref Transform transform, ref BoxCollider collider) =>
            {
                // Clamp paddle position to screen bounds
                transform.Position = new Vector2(transform.Position.X,
                    Math.Clamp(transform.Position.Y, 0, _screenHeight - collider.Size.Y));
            });
        }
    }
}