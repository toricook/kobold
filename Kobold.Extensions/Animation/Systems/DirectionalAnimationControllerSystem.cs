using System;
using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Extensions.Animation.Components;
using Kobold.Extensions.Physics.Components;
using AnimationComponent = Kobold.Extensions.Animation.Components.Animation;

namespace Kobold.Extensions.Animation.Systems
{
    /// <summary>
    /// Animation controller system that automatically switches animation clips based on movement direction.
    /// Queries entities with Velocity, Animation, and DirectionalAnimation components,
    /// and plays the appropriate directional animation based on the velocity vector.
    /// This is a controller system - it decides which animation to play, while AnimationSystem handles playback.
    /// </summary>
    public class DirectionalAnimationControllerSystem : ISystem
    {
        private readonly World _world;
        private QueryDescription _query;

        public DirectionalAnimationControllerSystem(World world)
        {
            _world = world;
            _query = new QueryDescription()
                .WithAll<Velocity, AnimationComponent, DirectionalAnimation>();
        }

        public void Update(float deltaTime)
        {
            _world.Query(in _query, (ref Velocity velocity, ref AnimationComponent animation, ref DirectionalAnimation directional) =>
            {
                // Check if speed is below minimum threshold
                float speed = velocity.Speed;
                if (speed < directional.MinimumSpeed)
                {
                    // Play idle animation if specified
                    if (!string.IsNullOrEmpty(directional.IdleAnimation))
                    {
                        animation.Play(directional.IdleAnimation, restart: false);
                    }
                    return;
                }

                // Determine direction from velocity
                string? animationToPlay = null;

                if (directional.UseEightDirections)
                {
                    animationToPlay = GetEightWayAnimation(velocity.Value, directional);
                }
                else
                {
                    animationToPlay = GetFourWayAnimation(velocity.Value, directional);
                }

                // Play the determined animation
                if (!string.IsNullOrEmpty(animationToPlay))
                {
                    animation.Play(animationToPlay, restart: false);
                }
            });
        }

        /// <summary>
        /// Determines which 4-way animation to play based on velocity
        /// </summary>
        private string? GetFourWayAnimation(System.Numerics.Vector2 velocity, DirectionalAnimation directional)
        {
            float absX = MathF.Abs(velocity.X);
            float absY = MathF.Abs(velocity.Y);

            // Determine primary direction based on priority
            bool isHorizontalDominant = directional.DirectionPriority == DirectionPriority.Horizontal
                ? absX >= absY
                : absX > absY;

            if (isHorizontalDominant)
            {
                // Horizontal movement
                if (velocity.X > 0)
                    return directional.RightAnimation;
                else if (velocity.X < 0)
                    return directional.LeftAnimation;
            }
            else
            {
                // Vertical movement (remember: positive Y = down in screen coords)
                if (velocity.Y > 0)
                    return directional.DownAnimation;
                else if (velocity.Y < 0)
                    return directional.UpAnimation;
            }

            return null;
        }

        /// <summary>
        /// Determines which 8-way animation to play based on velocity
        /// </summary>
        private string? GetEightWayAnimation(System.Numerics.Vector2 velocity, DirectionalAnimation directional)
        {
            float absX = MathF.Abs(velocity.X);
            float absY = MathF.Abs(velocity.Y);

            // Define a threshold for diagonal vs cardinal directions
            // If the ratio is close to 1:1, it's diagonal
            // Otherwise, it's more cardinal
            const float diagonalThreshold = 0.4f; // Ratio where we consider it diagonal
            float ratio = MathF.Min(absX, absY) / MathF.Max(absX, absY);
            bool isDiagonal = ratio > diagonalThreshold;

            if (isDiagonal)
            {
                // Determine diagonal direction
                if (velocity.X > 0 && velocity.Y < 0)
                {
                    // Up-Right
                    return directional.UpRightAnimation ?? directional.RightAnimation ?? directional.UpAnimation;
                }
                else if (velocity.X < 0 && velocity.Y < 0)
                {
                    // Up-Left
                    return directional.UpLeftAnimation ?? directional.LeftAnimation ?? directional.UpAnimation;
                }
                else if (velocity.X > 0 && velocity.Y > 0)
                {
                    // Down-Right
                    return directional.DownRightAnimation ?? directional.RightAnimation ?? directional.DownAnimation;
                }
                else if (velocity.X < 0 && velocity.Y > 0)
                {
                    // Down-Left
                    return directional.DownLeftAnimation ?? directional.LeftAnimation ?? directional.DownAnimation;
                }
            }
            else
            {
                // Cardinal direction (more horizontal or vertical)
                if (absX > absY)
                {
                    // More horizontal
                    if (velocity.X > 0)
                        return directional.RightAnimation;
                    else
                        return directional.LeftAnimation;
                }
                else
                {
                    // More vertical
                    if (velocity.Y > 0)
                        return directional.DownAnimation;
                    else
                        return directional.UpAnimation;
                }
            }

            return null;
        }
    }
}
