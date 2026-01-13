using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Kobold.Core.Assets;

namespace Kobold.Extensions.Animation.Utilities
{
    /// <summary>
    /// Fluent API builder for creating SpriteSheetConfig objects programmatically.
    /// Useful for creating sprite sheet configurations at runtime without JSON files.
    /// </summary>
    public class SpriteSheetConfigBuilder
    {
        private readonly SpriteSheetConfig _config = new();

        /// <summary>
        /// Creates a new SpriteSheetConfigBuilder instance
        /// </summary>
        public static SpriteSheetConfigBuilder Create() => new();

        /// <summary>
        /// Sets the grid dimensions for the sprite sheet
        /// </summary>
        /// <param name="spriteWidth">Width of each sprite in pixels</param>
        /// <param name="spriteHeight">Height of each sprite in pixels</param>
        /// <param name="columns">Number of columns in the sprite sheet (optional, will be calculated if not provided)</param>
        /// <param name="rows">Number of rows in the sprite sheet (optional, will be calculated if not provided)</param>
        public SpriteSheetConfigBuilder WithGrid(int spriteWidth, int spriteHeight, int? columns = null, int? rows = null)
        {
            _config.SpriteWidth = spriteWidth;
            _config.SpriteHeight = spriteHeight;
            if (columns.HasValue)
                _config.Columns = columns.Value;
            if (rows.HasValue)
                _config.Rows = rows.Value;
            return this;
        }

        /// <summary>
        /// Sets the spacing between sprites in the sheet
        /// </summary>
        /// <param name="spacing">Spacing in pixels</param>
        public SpriteSheetConfigBuilder WithSpacing(int spacing)
        {
            _config.Spacing = spacing;
            return this;
        }

        /// <summary>
        /// Sets the margin around the sprite sheet
        /// </summary>
        /// <param name="margin">Margin in pixels</param>
        public SpriteSheetConfigBuilder WithMargin(int margin)
        {
            _config.Margin = margin;
            return this;
        }

        /// <summary>
        /// Sets the default pivot point for all sprites
        /// </summary>
        /// <param name="x">X component (0 = left, 0.5 = center, 1 = right)</param>
        /// <param name="y">Y component (0 = top, 0.5 = center, 1 = bottom)</param>
        public SpriteSheetConfigBuilder WithPivot(float x, float y)
        {
            _config.Pivot = new Vector2Data { X = x, Y = y };
            return this;
        }

        /// <summary>
        /// Sets the default pivot point to center (0.5, 0.5)
        /// </summary>
        public SpriteSheetConfigBuilder WithCenterPivot()
        {
            return WithPivot(0.5f, 0.5f);
        }

        /// <summary>
        /// Sets the default pivot point to bottom-center (0.5, 1.0)
        /// Common for character sprites in top-down games
        /// </summary>
        public SpriteSheetConfigBuilder WithBottomCenterPivot()
        {
            return WithPivot(0.5f, 1.0f);
        }

        /// <summary>
        /// Adds an animation using a linear frame sequence
        /// </summary>
        /// <param name="name">Name of the animation</param>
        /// <param name="startFrame">Starting frame index</param>
        /// <param name="frameCount">Number of frames</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="loop">Whether the animation loops</param>
        public SpriteSheetConfigBuilder AddAnimation(string name, int startFrame, int frameCount, float fps, bool loop = true)
        {
            _config.Animations[name] = new AnimationData
            {
                StartFrame = startFrame,
                FrameCount = frameCount,
                Fps = fps,
                Loop = loop
            };
            return this;
        }

        /// <summary>
        /// Adds an animation using row and column indices
        /// </summary>
        /// <param name="name">Name of the animation</param>
        /// <param name="row">Row index</param>
        /// <param name="startColumn">Starting column index</param>
        /// <param name="frameCount">Number of frames</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="loop">Whether the animation loops</param>
        public SpriteSheetConfigBuilder AddAnimationFromRow(string name, int row, int startColumn, int frameCount, float fps, bool loop = true)
        {
            _config.Animations[name] = new AnimationData
            {
                Row = row,
                StartColumn = startColumn,
                FrameCount = frameCount,
                Fps = fps,
                Loop = loop
            };
            return this;
        }

        /// <summary>
        /// Adds an animation using arbitrary frame indices
        /// </summary>
        /// <param name="name">Name of the animation</param>
        /// <param name="frameIndices">Array of frame indices</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="loop">Whether the animation loops</param>
        public SpriteSheetConfigBuilder AddAnimation(string name, int[] frameIndices, float fps, bool loop = true)
        {
            _config.Animations[name] = new AnimationData
            {
                FrameIndices = frameIndices,
                Fps = fps,
                Loop = loop
            };
            return this;
        }

        /// <summary>
        /// Adds a named region (for single sprites or special regions)
        /// </summary>
        /// <param name="name">Name of the region</param>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        public SpriteSheetConfigBuilder AddNamedRegion(string name, int x, int y, int width, int height)
        {
            _config.NamedRegions[name] = new RectangleData
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
            return this;
        }

        /// <summary>
        /// Adds a named region using frame index (converts to pixel coordinates)
        /// </summary>
        /// <param name="name">Name of the region</param>
        /// <param name="frameIndex">Frame index in grid order</param>
        public SpriteSheetConfigBuilder AddNamedRegion(string name, int frameIndex)
        {
            int col = frameIndex % _config.Columns;
            int row = frameIndex / _config.Columns;

            int x = _config.Margin + col * (_config.SpriteWidth + _config.Spacing);
            int y = _config.Margin + row * (_config.SpriteHeight + _config.Spacing);

            return AddNamedRegion(name, x, y, _config.SpriteWidth, _config.SpriteHeight);
        }

        /// <summary>
        /// Adds metadata key-value pair
        /// </summary>
        /// <param name="key">Metadata key</param>
        /// <param name="value">Metadata value</param>
        public SpriteSheetConfigBuilder AddMetadata(string key, string value)
        {
            _config.Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// Helper method to add standard four-way walking animations
        /// </summary>
        /// <param name="upRow">Row index for up animation</param>
        /// <param name="downRow">Row index for down animation</param>
        /// <param name="leftRow">Row index for left animation</param>
        /// <param name="rightRow">Row index for right animation</param>
        /// <param name="framesPerDirection">Number of frames per direction</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="startCol">Starting column (default 0)</param>
        /// <param name="namePrefix">Prefix for animation names (default "walk_")</param>
        public SpriteSheetConfigBuilder AddFourWayWalking(
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

        /// <summary>
        /// Calculates the columns and rows based on texture dimensions and sprite size.
        /// Call this after loading the texture if you didn't specify columns/rows.
        /// </summary>
        /// <param name="textureWidth">Total texture width in pixels</param>
        /// <param name="textureHeight">Total texture height in pixels</param>
        public SpriteSheetConfigBuilder CalculateGridFromTexture(int textureWidth, int textureHeight)
        {
            int usableWidth = textureWidth - 2 * _config.Margin;
            int usableHeight = textureHeight - 2 * _config.Margin;

            _config.Columns = (usableWidth + _config.Spacing) / (_config.SpriteWidth + _config.Spacing);
            _config.Rows = (usableHeight + _config.Spacing) / (_config.SpriteHeight + _config.Spacing);

            return this;
        }

        /// <summary>
        /// Builds and returns the SpriteSheetConfig
        /// </summary>
        public SpriteSheetConfig Build()
        {
            return _config;
        }
    }
}
