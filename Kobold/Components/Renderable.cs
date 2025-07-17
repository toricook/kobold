using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    public struct RectangleRenderer
    {
        public Vector2 Size;
        public Color Color;

        public RectangleRenderer(Vector2 size, Color color)
        {
            Size = size;
            Color = color;
        }
    }

    public struct TextRenderer
    {
        public string Text;
        public Color Color;
        public float FontSize;

        public TextRenderer(string text, Color color, float fontSize = 16f)
        {
            Text = text;
            Color = color;
            FontSize = fontSize;
        }
    }
}
