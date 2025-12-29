using Kobold.Core.Abstractions.Rendering;
using System.Drawing;
using System.Numerics;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Renders a sprite from a texture or sprite sheet
    /// </summary>
    public struct SpriteRenderer : IRenderable
    {
        public ITexture Texture;
        public Rectangle SourceRect; // The area of the texture to render (for sprite sheets)
        public Vector2 Scale;
        public float Rotation;
        public Color Tint;
        public int Layer { get; }
        public Vector2 Pivot; // Pivot point for rotation (0,0 = top-left, 0.5,0.5 = center)

        public SpriteRenderer(ITexture texture, Rectangle sourceRect, Vector2 scale, float rotation = 0f,
            Color? tint = null, int layer = RenderLayers.GameObjects, Vector2? pivot = null)
        {
            Texture = texture;
            SourceRect = sourceRect;
            Scale = scale;
            Rotation = rotation;
            Tint = tint ?? Color.White;
            Layer = layer;
            Pivot = pivot ?? new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// Create a sprite renderer for a full texture (not from a sprite sheet)
        /// </summary>
        public static SpriteRenderer FullTexture(ITexture texture, Vector2 scale, float rotation = 0f,
            Color? tint = null, int layer = RenderLayers.GameObjects)
        {
            var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            return new SpriteRenderer(texture, sourceRect, scale, rotation, tint, layer);
        }

        /// <summary>
        /// Create a sprite renderer for a single frame from a sprite sheet
        /// </summary>
        public static SpriteRenderer FromSpriteSheet(ITexture texture, Rectangle sourceRect, Vector2 scale,
            float rotation = 0f, Color? tint = null, int layer = RenderLayers.GameObjects)
        {
            return new SpriteRenderer(texture, sourceRect, scale, rotation, tint, layer);
        }

        // Convenience constructors for common layers
        public static SpriteRenderer Background(ITexture texture, Rectangle sourceRect, Vector2 scale)
            => new SpriteRenderer(texture, sourceRect, scale, 0f, Color.White, RenderLayers.Background);

        public static SpriteRenderer GameObject(ITexture texture, Rectangle sourceRect, Vector2 scale)
            => new SpriteRenderer(texture, sourceRect, scale, 0f, Color.White, RenderLayers.GameObjects);

        public static SpriteRenderer UI(ITexture texture, Rectangle sourceRect, Vector2 scale)
            => new SpriteRenderer(texture, sourceRect, scale, 0f, Color.White, RenderLayers.UI);
    }
}
