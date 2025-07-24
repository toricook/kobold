using Kobold.Core.Abstractions;
using Microsoft.Xna.Framework.Graphics;
using SystemColor = System.Drawing.Color;
using SystemVector2 = System.Numerics.Vector2;
using Microsoft.Xna.Framework;

namespace Kobold.Monogame
{
    public class MonoGameRenderer : IRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture2D _pixelTexture;
        private readonly SpriteFont _defaultFont;

        public MonoGameRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont defaultFont)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _defaultFont = defaultFont;

            // Create a 1x1 white pixel for rectangle drawing
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Begin()
        {
            _spriteBatch.Begin();
        }

        public void End()
        {
            _spriteBatch.End();
        }

        public void DrawRectangle(SystemVector2 position, SystemVector2 size, SystemColor color)
        {
            var rect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            _spriteBatch.Draw(_pixelTexture, rect, null, ToXnaColor(color));
        }

        public void DrawTexture(ITexture texture, SystemVector2 position, SystemVector2 scale = default)
        {
            if (texture is MonoGameTexture mgTexture)
            {
                var scaleVector = scale == default ? Vector2.One : new Vector2(scale.X, scale.Y);
                _spriteBatch.Draw(mgTexture.Texture, new Vector2(position.X, position.Y), null, Color.White, 0f, Vector2.Zero, scaleVector, SpriteEffects.None, 0f);
            }
        }

        public void DrawText(string text, SystemVector2 position, SystemColor color, float fontSize = 16f)
        {
            if (_defaultFont != null)
            {
                var scale = fontSize / 16f;
                _spriteBatch.DrawString(_defaultFont, text,
                    new Vector2(position.X, position.Y),
                    ToXnaColor(color), 0f,Vector2.Zero, scale,
                    SpriteEffects.None, 0f);
            }
            else
            {
                // Use our pixel font
                PixelFont.DrawText(this, text, position, color, fontSize);
            }
        }

        public void DrawTriangle(SystemVector2[] points, SystemVector2 position, float rotation, SystemColor color)
        {
            if (points.Length != 3)
                throw new ArgumentException("Triangle must have exactly 3 points");

            // Draw triangle as 3 lines (wireframe)
            var transformedPoints = TransformPoints(points, position, rotation);

            DrawLine(transformedPoints[0], transformedPoints[1], color);
            DrawLine(transformedPoints[1], transformedPoints[2], color);
            DrawLine(transformedPoints[2], transformedPoints[0], color);
        }

        public void DrawTriangleFilled(SystemVector2[] points, SystemVector2 position, float rotation, SystemColor color)
        {
            if (points.Length != 3)
                throw new ArgumentException("Triangle must have exactly 3 points");

            // For filled triangles, we'll use a simple approach with lines
            // This isn't perfect but works for small triangles like ships
            var transformedPoints = TransformPoints(points, position, rotation);

            // Draw triangle by filling with horizontal lines
            FillTriangle(transformedPoints, color);
        }

        public void DrawLine(SystemVector2 start, SystemVector2 end, SystemColor color, float thickness = 1f)
        {
            var distance = SystemVector2.Distance(start, end);
            var angle = MathF.Atan2(end.Y - start.Y, end.X - start.X);

            var rectangle = new Rectangle(
                (int)start.X,
                (int)(start.Y - thickness / 2),
                (int)distance,
                (int)thickness
            );

            _spriteBatch.Draw(_pixelTexture, rectangle, null, ToXnaColor(color), angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        private SystemVector2[] TransformPoints(SystemVector2[] points, SystemVector2 position, float rotation)
        {
            var transformed = new SystemVector2[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                // Apply rotation
                if (rotation != 0f)
                {
                    float cos = MathF.Cos(rotation);
                    float sin = MathF.Sin(rotation);

                    float rotatedX = points[i].X * cos - points[i].Y * sin;
                    float rotatedY = points[i].X * sin + points[i].Y * cos;

                    transformed[i] = new SystemVector2(rotatedX + position.X, rotatedY + position.Y);
                }
                else
                {
                    transformed[i] = points[i] + position;
                }
            }

            return transformed;
        }

        private void FillTriangle(SystemVector2[] points, SystemColor color)
        {
            // Simple triangle filling using scanline approach
            // Sort points by Y coordinate
            Array.Sort(points, (a, b) => a.Y.CompareTo(b.Y));

            var p1 = points[0]; // Top point
            var p2 = points[1]; // Middle point  
            var p3 = points[2]; // Bottom point

            // Draw horizontal lines to fill the triangle
            for (float y = p1.Y; y <= p3.Y; y += 1f)
            {
                float leftX, rightX;

                // Find intersection points with triangle edges
                if (y <= p2.Y)
                {
                    // Upper part of triangle
                    leftX = GetXAtY(p1, p2, y);
                    rightX = GetXAtY(p1, p3, y);
                }
                else
                {
                    // Lower part of triangle
                    leftX = GetXAtY(p2, p3, y);
                    rightX = GetXAtY(p1, p3, y);
                }

                // Ensure leftX is actually on the left
                if (leftX > rightX)
                {
                    (leftX, rightX) = (rightX, leftX);
                }

                // Draw horizontal line
                if (rightX - leftX > 0)
                {
                    DrawLine(new SystemVector2(leftX, y), new SystemVector2(rightX, y), color);
                }
            }
        }

        private float GetXAtY(SystemVector2 p1, SystemVector2 p2, float y)
        {
            if (Math.Abs(p2.Y - p1.Y) < 0.001f) // Avoid division by zero
                return p1.X;

            float t = (y - p1.Y) / (p2.Y - p1.Y);
            return p1.X + t * (p2.X - p1.X);
        }

        public static Color ToXnaColor(SystemColor color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }
    }

    public class MonoGameTexture : ITexture
    {
        public Texture2D Texture { get; }
        public int Width => Texture.Width;
        public int Height => Texture.Height;

        public MonoGameTexture(Texture2D texture)
        {
            Texture = texture;
        }
    }
}
