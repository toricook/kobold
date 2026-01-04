using System.Collections.Generic;
using System.Drawing;

namespace Kobold.Extensions.Animation.Components
{
    /// <summary>
    /// Defines a sequence of frames for an animation
    /// </summary>
    public class AnimationClip
    {
        public string Name { get; set; }
        public Rectangle[] Frames { get; set; }
        public float FrameDuration { get; set; } // Duration of each frame in seconds
        public bool Loop { get; set; }

        public AnimationClip(string name, Rectangle[] frames, float frameDuration, bool loop = true)
        {
            Name = name;
            Frames = frames;
            FrameDuration = frameDuration;
            Loop = loop;
        }

        /// <summary>
        /// Helper to create an animation from a sprite sheet with uniform frame sizes
        /// </summary>
        public static AnimationClip FromSpriteSheet(string name, int startX, int startY, int frameWidth,
            int frameHeight, int frameCount, float frameDuration, bool loop = true)
        {
            var frames = new Rectangle[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                frames[i] = new Rectangle(
                    startX + (i * frameWidth),
                    startY,
                    frameWidth,
                    frameHeight
                );
            }
            return new AnimationClip(name, frames, frameDuration, loop);
        }

        /// <summary>
        /// Helper to create an animation from a sprite sheet grid
        /// </summary>
        public static AnimationClip FromGrid(string name, int startRow, int startCol, int rows, int cols,
            int frameWidth, int frameHeight, float frameDuration, bool loop = true)
        {
            var frames = new List<Rectangle>();
            for (int row = startRow; row < startRow + rows; row++)
            {
                for (int col = startCol; col < startCol + cols; col++)
                {
                    frames.Add(new Rectangle(
                        col * frameWidth,
                        row * frameHeight,
                        frameWidth,
                        frameHeight
                    ));
                }
            }
            return new AnimationClip(name, frames.ToArray(), frameDuration, loop);
        }
    }

    /// <summary>
    /// Component that manages sprite animation state
    /// </summary>
    public struct Animation
    {
        public Dictionary<string, AnimationClip> Clips;
        public string CurrentClip;
        public int CurrentFrame;
        public float TimeInCurrentFrame;
        public bool IsPlaying;

        public Animation(Dictionary<string, AnimationClip> clips, string initialClip = "")
        {
            Clips = clips;
            CurrentClip = initialClip;
            CurrentFrame = 0;
            TimeInCurrentFrame = 0f;
            IsPlaying = true;
        }

        /// <summary>
        /// Play a specific animation clip
        /// </summary>
        public void Play(string clipName, bool restart = false)
        {
            if (CurrentClip != clipName || restart)
            {
                CurrentClip = clipName;
                CurrentFrame = 0;
                TimeInCurrentFrame = 0f;
                IsPlaying = true;
            }
        }

        /// <summary>
        /// Get the current frame rectangle for rendering
        /// </summary>
        public readonly Rectangle GetCurrentFrameRect()
        {
            if (Clips != null && Clips.TryGetValue(CurrentClip, out var clip))
            {
                if (clip.Frames != null && clip.Frames.Length > 0)
                {
                    return clip.Frames[CurrentFrame];
                }
            }
            return new Rectangle(0, 0, 0, 0);
        }

        /// <summary>
        /// Get the current animation clip
        /// </summary>
        public readonly AnimationClip? GetCurrentClip()
        {
            if (Clips != null && Clips.TryGetValue(CurrentClip, out var clip))
            {
                return clip;
            }
            return null;
        }
    }
}
