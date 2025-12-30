using Microsoft.Xna.Framework.Graphics;
using SystemColor = System.Drawing.Color;
using SystemVector2 = System.Numerics.Vector2;
using Microsoft.Xna.Framework;
using Kobold.Core.Abstractions.Rendering;

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

        public void DrawSprite(ITexture texture, SystemVector2 position, System.Drawing.Rectangle sourceRect, SystemVector2 scale, float rotation, SystemColor tint)
        {
            if (texture is MonoGameTexture mgTexture)
            {
                var scaleVector = new Vector2(scale.X, scale.Y);
                var xnaSourceRect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
                var origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f);

                _spriteBatch.Draw(
                    mgTexture.Texture,
                    new Vector2(position.X, position.Y),
                    xnaSourceRect,
                    ToXnaColor(tint),
                    rotation,
                    origin,
                    scaleVector,
                    SpriteEffects.None,
                    0f
                );
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
            // Draw triangle outline using lines
            if (points.Length >= 3)
            {
                DrawLine(points[0] + position, points[1] + position, color, 1f);
                DrawLine(points[1] + position, points[2] + position, color, 1f);
                DrawLine(points[2] + position, points[0] + position, color, 1f);
            }
        }

        public void DrawTriangleFilled(SystemVector2[] points, SystemVector2 position, float rotation, SystemColor color)
        {
            // For now, just draw outline - filled triangles require more complex rendering
            DrawTriangle(points, position, rotation, color);
        }

        public void DrawLine(SystemVector2 start, SystemVector2 end, SystemColor color, float thickness = 1f)
        {
            var distance = SystemVector2.Distance(start, end);
            var angle = (float)System.Math.Atan2(end.Y - start.Y, end.X - start.X);

            _spriteBatch.Draw(
                _pixelTexture,
                new Vector2(start.X, start.Y),
                null,
                ToXnaColor(color),
                angle,
                Vector2.Zero,
                new Vector2(distance, thickness),
                SpriteEffects.None,
                0f
            );
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
