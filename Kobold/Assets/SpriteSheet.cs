using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Kobold.Core.Assets
{
    /// <summary>
    /// Represents a sprite sheet with its texture and configuration
    /// </summary>
    public class SpriteSheet
    {
        public ITexture Texture { get; }
        public SpriteSheetConfig Config { get; }

        public SpriteSheet(ITexture texture, SpriteSheetConfig config)
        {
            Texture = texture;
            Config = config;
        }

        /// <summary>
        /// Get a sprite frame by grid index (0-based, left-to-right, top-to-bottom)
        /// </summary>
        public Rectangle GetFrame(int index)
        {
            if (index < 0 || index >= Config.Rows * Config.Columns)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Frame index {index} is out of range. Sheet has {Config.Rows * Config.Columns} frames.");
            }

            int row = index / Config.Columns;
            int col = index % Config.Columns;

            return GetFrameByRowCol(row, col);
        }

        /// <summary>
        /// Get a sprite frame by row and column
        /// </summary>
        public Rectangle GetFrameByRowCol(int row, int col)
        {
            if (row < 0 || row >= Config.Rows)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (col < 0 || col >= Config.Columns)
                throw new ArgumentOutOfRangeException(nameof(col));

            int x = Config.Margin + col * (Config.SpriteWidth + Config.Spacing);
            int y = Config.Margin + row * (Config.SpriteHeight + Config.Spacing);

            return new Rectangle(x, y, Config.SpriteWidth, Config.SpriteHeight);
        }

        /// <summary>
        /// Get a named region from the sprite sheet
        /// </summary>
        public Rectangle GetNamedRegion(string name)
        {
            if (!Config.NamedRegions.TryGetValue(name, out var regionData))
            {
                throw new KeyNotFoundException($"Named region '{name}' not found in sprite sheet config.");
            }

            return regionData.ToRectangle();
        }

        /// <summary>
        /// Check if a named region exists
        /// </summary>
        public bool HasNamedRegion(string name)
        {
            return Config.NamedRegions.ContainsKey(name);
        }

        /// <summary>
        /// Get all frames for an animation as rectangles
        /// </summary>
        public Rectangle[] GetAnimationFrames(string animationName)
        {
            if (!Config.Animations.TryGetValue(animationName, out var animData))
            {
                throw new KeyNotFoundException($"Animation '{animationName}' not found in sprite sheet config.");
            }

            return GetAnimationFrames(animData);
        }

        /// <summary>
        /// Get all frames for an animation from AnimationData
        /// </summary>
        public Rectangle[] GetAnimationFrames(AnimationData animData)
        {
            // If custom frame indices are specified, use those
            if (animData.FrameIndices != null && animData.FrameIndices.Length > 0)
            {
                var frames = new Rectangle[animData.FrameIndices.Length];
                for (int i = 0; i < animData.FrameIndices.Length; i++)
                {
                    frames[i] = GetFrame(animData.FrameIndices[i]);
                }
                return frames;
            }

            // If Row is specified, use row-based indexing
            if (animData.Row.HasValue)
            {
                int row = animData.Row.Value;
                int startCol = animData.StartColumn ?? 0;
                var frames = new Rectangle[animData.FrameCount];

                for (int i = 0; i < animData.FrameCount; i++)
                {
                    frames[i] = GetFrameByRowCol(row, startCol + i);
                }
                return frames;
            }

            // Otherwise use sequential frame indices
            {
                var frames = new Rectangle[animData.FrameCount];
                for (int i = 0; i < animData.FrameCount; i++)
                {
                    frames[i] = GetFrame(animData.StartFrame + i);
                }
                return frames;
            }
        }

        /// <summary>
        /// Create an AnimationClip from a configured animation
        /// </summary>
        public AnimationClip CreateAnimationClip(string animationName)
        {
            if (!Config.Animations.TryGetValue(animationName, out var animData))
            {
                throw new KeyNotFoundException($"Animation '{animationName}' not found in sprite sheet config.");
            }

            var frames = GetAnimationFrames(animData);
            float frameDuration = 1f / animData.Fps;

            return new AnimationClip(animationName, frames, frameDuration, animData.Loop);
        }

        /// <summary>
        /// Create all configured animations as AnimationClips
        /// </summary>
        public Dictionary<string, AnimationClip> CreateAllAnimations()
        {
            var animations = new Dictionary<string, AnimationClip>();

            foreach (var kvp in Config.Animations)
            {
                animations[kvp.Key] = CreateAnimationClip(kvp.Key);
            }

            return animations;
        }

        /// <summary>
        /// Get total number of frames in the sprite sheet
        /// </summary>
        public int TotalFrames => Config.Rows * Config.Columns;
    }
}
