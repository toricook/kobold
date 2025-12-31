using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    public class RenderSystem : IRenderSystem
    {
        private readonly IRenderer _renderer;
        private readonly World _world;

        public RenderSystem(IRenderer renderer, World world)
        {
            _renderer = renderer;
            _world = world;
        }

        public void Render()
        {
            // Note: Begin/End are called by GameEngineBase, not by individual render systems

            // Get camera (if exists)
            Camera? camera = GetCamera();

            // Collect all renderable entities with their layers
            var renderableEntities = new List<RenderableEntity>();

            // Collect rectangles - layer is built into the component
            var rectangleQuery = new QueryDescription().WithAll<Transform, RectangleRenderer>();
            _world.Query(in rectangleQuery, (Entity entity, ref Transform transform, ref RectangleRenderer rectangleRenderer) =>
            {
                renderableEntities.Add(new RenderableEntity
                {
                    Entity = entity,
                    Layer = rectangleRenderer.Layer, // Layer from component
                    RenderType = RenderType.Rectangle,
                    Transform = transform,
                    RectangleRenderer = rectangleRenderer
                });
            });

            // Collect text - layer is built into the component
            var textQuery = new QueryDescription().WithAll<Transform, TextRenderer>();
            _world.Query(in textQuery, (Entity entity, ref Transform transform, ref TextRenderer textRenderer) =>
            {
                renderableEntities.Add(new RenderableEntity
                {
                    Entity = entity,
                    Layer = textRenderer.Layer, // Layer from component
                    RenderType = RenderType.Text,
                    Transform = transform,
                    TextRenderer = textRenderer
                });
            });

            // Collect sprites - layer is built into the component
            var spriteQuery = new QueryDescription().WithAll<Transform, SpriteRenderer>();
            _world.Query(in spriteQuery, (Entity entity, ref Transform transform, ref SpriteRenderer spriteRenderer) =>
            {
                renderableEntities.Add(new RenderableEntity
                {
                    Entity = entity,
                    Layer = spriteRenderer.Layer, // Layer from component
                    RenderType = RenderType.Sprite,
                    Transform = transform,
                    SpriteRenderer = spriteRenderer
                });
            });

            // Sort by layer (lower layers render first, appear behind)
            renderableEntities.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            // Render in layer order
            foreach (var renderable in renderableEntities)
            {
                // Convert world position to screen position
                // UI layer (100+) should render in screen space, not affected by camera
                bool isUILayer = renderable.Layer >= RenderLayers.UI;
                Vector2 screenPosition = (camera.HasValue && !isUILayer)
                    ? camera.Value.WorldToScreen(renderable.Transform.Position)
                    : renderable.Transform.Position;

                // Round to integer pixel coordinates to prevent sub-pixel rendering artifacts
                screenPosition = new Vector2(MathF.Round(screenPosition.X), MathF.Round(screenPosition.Y));

                switch (renderable.RenderType)
                {
                    case RenderType.Rectangle:
                        _renderer.DrawRectangle(screenPosition,
                            renderable.RectangleRenderer.Size,
                            renderable.RectangleRenderer.Color);
                        break;

                    case RenderType.Text:
                        _renderer.DrawText(renderable.TextRenderer.Text,
                            screenPosition,
                            renderable.TextRenderer.Color,
                            renderable.TextRenderer.FontSize);
                        break;

                    case RenderType.Sprite:
                        _renderer.DrawSprite(renderable.SpriteRenderer.Texture,
                            screenPosition,
                            renderable.SpriteRenderer.SourceRect,
                            renderable.SpriteRenderer.Scale,
                            renderable.SpriteRenderer.Rotation,
                            renderable.SpriteRenderer.Tint);
                        break;
                }
            }
        }

        private Camera? GetCamera()
        {
            var cameraQuery = new QueryDescription().WithAll<Camera>();
            Camera? result = null;

            _world.Query(in cameraQuery, (ref Camera camera) =>
            {
                result = camera;
            });

            return result;
        }

        private struct RenderableEntity
        {
            public Entity Entity;
            public int Layer;
            public RenderType RenderType;
            public Transform Transform;
            public RectangleRenderer RectangleRenderer;
            public TextRenderer TextRenderer;
            public SpriteRenderer SpriteRenderer;
        }

        private enum RenderType
        {
            Rectangle,
            Text,
            Sprite
        }
    }
}
