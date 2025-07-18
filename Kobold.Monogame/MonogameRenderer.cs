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

        private Color ToXnaColor(SystemColor color)
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
