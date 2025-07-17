using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions
{
    public interface IRenderer
    {
        void Begin();
        void End();
        void DrawRectangle(Vector2 position, Vector2 size, Color color);
        void DrawTexture(ITexture texture, Vector2 position, Vector2 scale = default);
        void DrawText(string text, Vector2 position, Color color, float fontSize = 16f);
    }

    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
    }
}
