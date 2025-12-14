using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
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
            _renderer.Begin();

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

            // Sort by layer (lower layers render first, appear behind)
            renderableEntities.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            // Render in layer order
            foreach (var renderable in renderableEntities)
            {
                switch (renderable.RenderType)
                {
                    case RenderType.Rectangle:
                        _renderer.DrawRectangle(renderable.Transform.Position,
                            renderable.RectangleRenderer.Size,
                            renderable.RectangleRenderer.Color);
                        break;

                    case RenderType.Text:
                        _renderer.DrawText(renderable.TextRenderer.Text,
                            renderable.Transform.Position,
                            renderable.TextRenderer.Color,
                            renderable.TextRenderer.FontSize);
                        break;
                }
            }

            _renderer.End();

        }

        private struct RenderableEntity
        {
            public Entity Entity;
            public int Layer;
            public RenderType RenderType;
            public Transform Transform;
            public RectangleRenderer RectangleRenderer;
            public TextRenderer TextRenderer;
        }

        private enum RenderType
        {
            Rectangle,
            Text
        }
    }
}
