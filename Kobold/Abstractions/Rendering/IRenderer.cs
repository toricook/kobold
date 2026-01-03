using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions.Rendering
{
    /// <summary>
    /// The renderer defines how stuff actually gets drawn to the screen
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Begin a rendering batch. Must be called before any draw operations.
        /// </summary>
        void Begin();

        /// <summary>
        /// End the current rendering batch and submit draw calls to the GPU.
        /// </summary>
        void End();

        /// <summary>
        /// Draw a filled rectangle
        /// </summary>
        /// <param name="position">Top-left corner position</param>
        /// <param name="size">Width and height of the rectangle</param>
        /// <param name="color">Fill color</param>
        void DrawRectangle(Vector2 position, Vector2 size, Color color);

        /// <summary>
        /// Draw a texture at the specified position
        /// </summary>
        /// <param name="texture">The texture to draw</param>
        /// <param name="position">Position to draw at</param>
        /// <param name="scale">Optional scale factor (default is 1,1)</param>
        void DrawTexture(ITexture texture, Vector2 position, Vector2 scale = default);

        /// <summary>
        /// Draw a sprite from a texture with advanced options
        /// </summary>
        /// <param name="texture">The source texture</param>
        /// <param name="position">Position to draw at</param>
        /// <param name="sourceRect">Rectangle defining the sprite region in the texture</param>
        /// <param name="scale">Scale factor</param>
        /// <param name="rotation">Rotation in radians</param>
        /// <param name="tint">Color tint to apply</param>
        void DrawSprite(ITexture texture, Vector2 position, Rectangle sourceRect, Vector2 scale, float rotation, Color tint);

        /// <summary>
        /// Draw text at the specified position
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="position">Position to draw at</param>
        /// <param name="color">Text color</param>
        /// <param name="fontSize">Font size in pixels (default is 16)</param>
        void DrawText(string text, Vector2 position, Color color, float fontSize = 16f);

        /// <summary>
        /// Draw a triangle outline
        /// </summary>
        /// <param name="points">Array of 3 points defining the triangle vertices</param>
        /// <param name="position">Position offset</param>
        /// <param name="rotation">Rotation in radians</param>
        /// <param name="color">Line color</param>
        void DrawTriangle(Vector2[] points, Vector2 position, float rotation, Color color);

        /// <summary>
        /// Draw a filled triangle
        /// </summary>
        /// <param name="points">Array of 3 points defining the triangle vertices</param>
        /// <param name="position">Position offset</param>
        /// <param name="rotation">Rotation in radians</param>
        /// <param name="color">Fill color</param>
        void DrawTriangleFilled(Vector2[] points, Vector2 position, float rotation, Color color);

        /// <summary>
        /// Draw a line between two points
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="color">Line color</param>
        /// <param name="thickness">Line thickness in pixels (default is 1)</param>
        void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f);
    }

}
