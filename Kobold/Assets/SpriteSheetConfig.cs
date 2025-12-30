using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Kobold.Core.Assets
{
    /// <summary>
    /// Configuration for how a sprite sheet should be divided
    /// </summary>
    public class SpriteSheetConfig
    {
        /// <summary>
        /// Width of each sprite in pixels (for uniform grids)
        /// </summary>
        public int SpriteWidth { get; set; }

        /// <summary>
        /// Height of each sprite in pixels (for uniform grids)
        /// </summary>
        public int SpriteHeight { get; set; }

        /// <summary>
        /// Number of columns in the sprite sheet
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// Number of rows in the sprite sheet
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Spacing between sprites in pixels (default 0)
        /// </summary>
        public int Spacing { get; set; } = 0;

        /// <summary>
        /// Margin around the sprite sheet in pixels (default 0)
        /// </summary>
        public int Margin { get; set; } = 0;

        /// <summary>
        /// Default pivot point for all sprites (0,0 = top-left, 0.5,0.5 = center, 1,1 = bottom-right)
        /// </summary>
        public Vector2Data Pivot { get; set; } = new Vector2Data { X = 0.5f, Y = 0.5f };

        /// <summary>
        /// Named regions for specific sprites or groups
        /// </summary>
        public Dictionary<string, RectangleData> NamedRegions { get; set; } = new Dictionary<string, RectangleData>();

        /// <summary>
        /// Animation definitions
        /// </summary>
        public Dictionary<string, AnimationData> Animations { get; set; } = new Dictionary<string, AnimationData>();

        /// <summary>
        /// Metadata (optional user data)
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Vector2 data for JSON serialization
    /// </summary>
    public class Vector2Data
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2 ToVector2() => new Vector2(X, Y);

        public static Vector2Data FromVector2(Vector2 v) => new Vector2Data { X = v.X, Y = v.Y };
    }

    /// <summary>
    /// Rectangle data for JSON serialization
    /// </summary>
    public class RectangleData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle ToRectangle() => new Rectangle(X, Y, Width, Height);

        public static RectangleData FromRectangle(Rectangle r) => new RectangleData { X = r.X, Y = r.Y, Width = r.Width, Height = r.Height };
    }

    /// <summary>
    /// Animation configuration data
    /// </summary>
    public class AnimationData
    {
        /// <summary>
        /// Starting frame index (in grid order: left-to-right, top-to-bottom)
        /// </summary>
        public int StartFrame { get; set; }

        /// <summary>
        /// Number of frames in the animation
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// Frames per second
        /// </summary>
        public float Fps { get; set; } = 10f;

        /// <summary>
        /// Whether the animation should loop
        /// </summary>
        public bool Loop { get; set; } = true;

        /// <summary>
        /// Row number (alternative to StartFrame for grid-based)
        /// </summary>
        public int? Row { get; set; }

        /// <summary>
        /// Starting column (used with Row)
        /// </summary>
        public int? StartColumn { get; set; }

        /// <summary>
        /// Custom frame indices (overrides StartFrame/FrameCount)
        /// </summary>
        public int[]? FrameIndices { get; set; }
    }
}
