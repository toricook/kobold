using System;
using System.Collections.Generic;
using System.Drawing;
using Kobold.Core.Assets;
using Kobold.Extensions.Animation.Components;
using AnimationComponent = Kobold.Extensions.Animation.Components.Animation;

namespace Kobold.Extensions.Animation.Utilities
{
    /// <summary>
    /// Fluent API builder for creating Animation components with clips from sprite sheets.
    /// Supports various sprite sheet layouts: single row, multiple animations per row,
    /// row-based layouts, and arbitrary frame sequences.
    /// </summary>
    public class AnimationBuilder
    {
        private SpriteSheet? _spriteSheet;
        private readonly Dictionary<string, AnimationClip> _clips = new();

        /// <summary>
        /// Creates a new AnimationBuilder instance
        /// </summary>
        public static AnimationBuilder Create() => new();

        /// <summary>
        /// Sets the sprite sheet to use for all animations
        /// </summary>
        public AnimationBuilder WithSpriteSheet(SpriteSheet sheet)
        {
            _spriteSheet = sheet;
            return this;
        }

        /// <summary>
        /// Adds an animation using a linear frame range (e.g., frames 0-7).
        /// Frames are indexed in grid order: left-to-right, top-to-bottom.
        /// </summary>
        /// <param name="name">Name of the animation clip</param>
        /// <param name="frames">Range of frame indices (e.g., 0..7 for frames 0-7 inclusive)</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="loop">Whether the animation should loop</param>
        public AnimationBuilder AddAnimation(string name, Range frames, float fps, bool loop = true)
        {
            EnsureSpriteSheet();

            int totalFrames = _spriteSheet!.Config.Columns * _spriteSheet.Config.Rows;
            var (start, length) = frames.GetOffsetAndLength(totalFrames);

            var rectangles = new Rectangle[length];
            for (int i = 0; i < length; i++)
            {
                rectangles[i] = _spriteSheet.GetFrame(start + i);
            }

            _clips[name] = new AnimationClip(name, rectangles, 1f / fps, loop);
            return this;
        }

        /// <summary>
        /// Adds an animation using a linear start frame and count.
        /// Frames are indexed in grid order: left-to-right, top-to-bottom.
        /// </summary>
        /// <param name="name">Name of the animation clip</param>
        /// <param name="startFrame">Starting frame index</param>
        /// <param name="frameCount">Number of frames in the animation</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="loop">Whether the animation should loop</param>
        public AnimationBuilder AddAnimation(string name, int startFrame, int frameCount, float fps, bool loop = true)
        {
            EnsureSpriteSheet();

            var rectangles = new Rectangle[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                rectangles[i] = _spriteSheet!.GetFrame(startFrame + i);
            }

            _clips[name] = new AnimationClip(name, rectangles, 1f / fps, loop);
            return this;
        }

        /// <summary>
        /// Adds an animation using arbitrary, non-contiguous frame indices.
        /// Useful for cherry-picking specific frames or creating complex animation sequences.
        /// </summary>
        /// <param name="name">Name of the animation clip</param>
        /// <param name="frameIndices">Array of frame indices to use</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="loop">Whether the animation should loop</param>
        public AnimationBuilder AddAnimation(string name, int[] frameIndices, float fps, bool loop = true)
        {
            EnsureSpriteSheet();

            var rectangles = new Rectangle[frameIndices.Length];
            for (int i = 0; i < frameIndices.Length; i++)
            {
                rectangles[i] = _spriteSheet!.GetFrame(frameIndices[i]);
            }

            _clips[name] = new AnimationClip(name, rectangles, 1f / fps, loop);
            return this;
        }

        /// <summary>
        /// Adds an animation from a specific row in the sprite sheet.
        /// Convenient for sprite sheets organized with one animation per row.
        /// </summary>
        /// <param name="name">Name of the animation clip</param>
        /// <param name="row">Row index (0-based)</param>
        /// <param name="startCol">Starting column index (0-based)</param>
        /// <param name="frameCount">Number of frames to include from the row</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="loop">Whether the animation should loop</param>
        public AnimationBuilder AddAnimationFromRow(string name, int row, int startCol, int frameCount, float fps, bool loop = true)
        {
            EnsureSpriteSheet();

            var rectangles = new Rectangle[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                rectangles[i] = _spriteSheet!.GetFrameByRowCol(row, startCol + i);
            }

            _clips[name] = new AnimationClip(name, rectangles, 1f / fps, loop);
            return this;
        }

        /// <summary>
        /// Adds an animation from a rectangular region spanning multiple rows and columns.
        /// Frames are collected in row-major order (left-to-right, top-to-bottom).
        /// </summary>
        /// <param name="name">Name of the animation clip</param>
        /// <param name="startRow">Starting row index (0-based)</param>
        /// <param name="startCol">Starting column index (0-based)</param>
        /// <param name="rows">Number of rows to include</param>
        /// <param name="cols">Number of columns to include</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="loop">Whether the animation should loop</param>
        public AnimationBuilder AddAnimationFromRegion(string name, int startRow, int startCol, int rows, int cols, float fps, bool loop = true)
        {
            EnsureSpriteSheet();

            var rectangles = new List<Rectangle>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    rectangles.Add(_spriteSheet!.GetFrameByRowCol(startRow + r, startCol + c));
                }
            }

            _clips[name] = new AnimationClip(name, rectangles.ToArray(), 1f / fps, loop);
            return this;
        }

        /// <summary>
        /// Adds an animation from a named animation in the sprite sheet config.
        /// Uses the SpriteSheet's built-in animation definitions.
        /// </summary>
        /// <param name="name">Name of the animation in the sprite sheet config</param>
        /// <param name="fps">Optional override for frames per second (uses config value if null)</param>
        /// <param name="loop">Optional override for looping (uses config value if null)</param>
        public AnimationBuilder AddAnimationFromConfig(string name, float? fps = null, bool? loop = null)
        {
            EnsureSpriteSheet();

            var frames = _spriteSheet!.GetAnimationFrames(name);
            var configAnim = _spriteSheet.Config.Animations[name];

            _clips[name] = new AnimationClip(
                name,
                frames,
                1f / (fps ?? configAnim.Fps),
                loop ?? configAnim.Loop
            );

            return this;
        }

        /// <summary>
        /// Adds all animations defined in the sprite sheet config
        /// </summary>
        public AnimationBuilder AddAllAnimationsFromConfig()
        {
            EnsureSpriteSheet();

            foreach (var animName in _spriteSheet!.Config.Animations.Keys)
            {
                AddAnimationFromConfig(animName);
            }

            return this;
        }

        /// <summary>
        /// Helper method to add four-way directional walking animations.
        /// Assumes each direction is on a separate row.
        /// </summary>
        /// <param name="upRow">Row index for up animation</param>
        /// <param name="downRow">Row index for down animation</param>
        /// <param name="leftRow">Row index for left animation</param>
        /// <param name="rightRow">Row index for right animation</param>
        /// <param name="framesPerDirection">Number of frames per direction</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="startCol">Starting column for all animations (default 0)</param>
        /// <param name="namePrefix">Prefix for animation names (default "walk_")</param>
        public AnimationBuilder AddFourWayWalking(
            int upRow,
            int downRow,
            int leftRow,
            int rightRow,
            int framesPerDirection,
            float fps,
            int startCol = 0,
            string namePrefix = "walk_")
        {
            AddAnimationFromRow($"{namePrefix}up", upRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}down", downRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}left", leftRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}right", rightRow, startCol, framesPerDirection, fps);
            return this;
        }
        is 
        /// <summary>
        /// Helper method to add eight-way directional walking animations.
        /// Assumes each direction is on a separate row.
        /// </summary>
        /// <param name="upRow">Row index for up animation</param>
        /// <param name="downRow">Row index for down animation</param>
        /// <param name="leftRow">Row index for left animation</param>
        /// <param name="rightRow">Row index for right animation</param>
        /// <param name="upLeftRow">Row index for up-left animation</param>
        /// <param name="upRightRow">Row index for up-right animation</param>
        /// <param name="downLeftRow">Row index for down-left animation</param>
        /// <param name="downRightRow">Row index for down-right animation</param>
        /// <param name="framesPerDirection">Number of frames per direction</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="startCol">Starting column for all animations (default 0)</param>
        /// <param name="namePrefix">Prefix for animation names (default "walk_")</param>
        public AnimationBuilder AddEightWayWalking(
            int upRow,
            int downRow,
            int leftRow,
            int rightRow,
            int upLeftRow,
            int upRightRow,
            int downLeftRow,
            int downRightRow,
            int framesPerDirection,
            float fps,
            int startCol = 0,
            string namePrefix = "walk_")
        {
            AddAnimationFromRow($"{namePrefix}up", upRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}down", downRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}left", leftRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}right", rightRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}up_left", upLeftRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}up_right", upRightRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}down_left", downLeftRow, startCol, framesPerDirection, fps);
            AddAnimationFromRow($"{namePrefix}down_right", downRightRow, startCol, framesPerDirection, fps);
            return this;
        }

        /// <summary>
        /// Builds and returns the Animation component with all configured clips
        /// </summary>
        public AnimationComponent Build()
        {
            return new AnimationComponent(_clips);
        }

        private void EnsureSpriteSheet()
        {
            if (_spriteSheet == null)
            {
                throw new InvalidOperationException("SpriteSheet must be set before adding animations. Call WithSpriteSheet() first.");
            }
        }
    }
}
