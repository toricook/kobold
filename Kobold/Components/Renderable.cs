using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Standard rendering layer values
    /// Lower values render first (behind), higher values render last (on top)
    /// </summary>
    public static class RenderLayers
    {
        public const int Background = -100;
        public const int GameObjects = 0;
        public const int Effects = 50;
        public const int UI = 100;
        public const int Debug = 1000;
    }

    // Base interface for all renderable components
    public interface IRenderable
    {
        int Layer { get; }
    }

    public struct RectangleRenderer : IRenderable
    {
        public Vector2 Size;
        public Color Color;
        public int Layer { get; }

        public RectangleRenderer(Vector2 size, Color color, int layer = RenderLayers.GameObjects)
        {
            Size = size;
            Color = color;
            Layer = layer;
        }

        // Convenience constructors for common layers
        public static RectangleRenderer Background(Vector2 size, Color color)
            => new RectangleRenderer(size, color, RenderLayers.Background);

        public static RectangleRenderer GameObject(Vector2 size, Color color)
            => new RectangleRenderer(size, color, RenderLayers.GameObjects);

        public static RectangleRenderer UI(Vector2 size, Color color)
            => new RectangleRenderer(size, color, RenderLayers.UI);
    }

    public struct TextRenderer : IRenderable
    {
        public string Text;
        public Color Color;
        public float FontSize;
        public int Layer { get; }

        public TextRenderer(string text, Color color, float fontSize = 16f, int layer = RenderLayers.UI)
        {
            Text = text;
            Color = color;
            FontSize = fontSize;
            Layer = layer;
        }

        // Convenience constructors for common layers
        public static TextRenderer GameText(string text, Color color, float fontSize = 16f)
            => new TextRenderer(text, color, fontSize, RenderLayers.GameObjects);

        public static TextRenderer UIText(string text, Color color, float fontSize = 16f)
            => new TextRenderer(text, color, fontSize, RenderLayers.UI);

        public static TextRenderer DebugText(string text, Color color, float fontSize = 12f)
            => new TextRenderer(text, color, fontSize, RenderLayers.Debug);
    }

    /// <summary>
    /// Triangle renderer component
    /// </summary>
    public struct TriangleRenderer : IRenderable
    {
        public Vector2[] Points; // 3 points relative to transform position
        public Color Color;
        public int Layer { get; }

        public TriangleRenderer(Vector2[] points, Color color, int layer = RenderLayers.GameObjects)
        {
            if (points.Length != 3)
                throw new ArgumentException("Triangle must have exactly 3 points");

            Points = points;
            Color = color;
            Layer = layer;
        }

        public TriangleRenderer(Vector2 point1, Vector2 point2, Vector2 point3, Color color,
            int layer = RenderLayers.GameObjects) : this(new Vector2[] { point1, point2, point3 }, color, layer)
        {
        }

        /// <summary>
        /// Create an isosceles triangle pointing up
        /// </summary>
        public static TriangleRenderer PointingUp(float width, float height, Color color, int layer = RenderLayers.GameObjects)
        {
            var points = new Vector2[]
            {
                new Vector2(0, -height/2),           // Top point
                new Vector2(-width/2, height/2),     // Bottom left
                new Vector2(width/2, height/2)       // Bottom right
            };
            return new TriangleRenderer(points, color, layer);
        }

        /// <summary>
        /// Create an isosceles triangle pointing right
        /// </summary>
        public static TriangleRenderer PointingRight(float width, float height, Color color, int layer = RenderLayers.GameObjects)
        {
            var points = new Vector2[]
            {
                new Vector2(width/2, 0),             // Tip (pointing right)
                new Vector2(-width/2, -height/2),    // Bottom left
                new Vector2(-width/2, height/2)      // Top left
            };
            return new TriangleRenderer(points, color, layer);
        }

        // Convenience constructors for common layers
        public static TriangleRenderer GameObject(Vector2[] points, Color color)
            => new TriangleRenderer(points, color, RenderLayers.GameObjects);

        public static TriangleRenderer UI(Vector2[] points, Color color)
            => new TriangleRenderer(points, color, RenderLayers.UI);
    }
}
