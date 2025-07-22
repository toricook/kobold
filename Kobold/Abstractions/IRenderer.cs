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
        void DrawTriangle(Vector2[] points, Vector2 position, float rotation, Color color);
        void DrawTriangleFilled(Vector2[] points, Vector2 position, float rotation, Color color);
        void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f);
    }

    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
    }
}
