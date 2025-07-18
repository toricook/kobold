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
}
