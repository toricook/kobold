using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Extensions.Animation.Components;
using AnimationComponent = Kobold.Extensions.Animation.Components.Animation;

namespace Kobold.Extensions.Animation.Systems
{
    /// <summary>
    /// System that updates sprite animations
    /// </summary>
    public class AnimationSystem : ISystem
    {
        private readonly World _world;

        public AnimationSystem(World world)
        {
            _world = world;
        }

        public void Update(float deltaTime)
        {
            // Query for entities with both Animation and SpriteRenderer components
            var animationQuery = new QueryDescription().WithAll<AnimationComponent, SpriteRenderer>();

            _world.Query(in animationQuery, (Entity entity, ref AnimationComponent animation, ref SpriteRenderer spriteRenderer) =>
            {
                if (!animation.IsPlaying)
                    return;

                var currentClip = animation.GetCurrentClip();
                if (currentClip == null || currentClip.Frames == null || currentClip.Frames.Length == 0)
                    return;

                // Update time in current frame
                animation.TimeInCurrentFrame += deltaTime;

                // Check if we need to advance to the next frame
                if (animation.TimeInCurrentFrame >= currentClip.FrameDuration)
                {
                    animation.TimeInCurrentFrame -= currentClip.FrameDuration;
                    animation.CurrentFrame++;

                    // Handle end of animation
                    if (animation.CurrentFrame >= currentClip.Frames.Length)
                    {
                        if (currentClip.Loop)
                        {
                            animation.CurrentFrame = 0;
                        }
                        else
                        {
                            animation.CurrentFrame = currentClip.Frames.Length - 1;
                            animation.IsPlaying = false;
                        }
                    }
                }

                // Update the sprite renderer's source rect to the current frame
                spriteRenderer.SourceRect = animation.GetCurrentFrameRect();
            });
        }
    }
}
