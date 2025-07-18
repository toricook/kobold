using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using System.Drawing;

namespace Pong.Systems
{
    public class RenderSystem : ISystem
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

            // Render rectangles
            var rectangleQuery = new QueryDescription().WithAll<Transform, RectangleRenderer>();
            _world.Query(in rectangleQuery, (ref Transform transform, ref RectangleRenderer rectangleRenderer) =>
            {
                _renderer.DrawRectangle(transform.Position, rectangleRenderer.Size, rectangleRenderer.Color);
            });

            // Render text
            var textQuery = new QueryDescription().WithAll<Transform, TextRenderer>();
            _world.Query(in textQuery, (ref Transform transform, ref TextRenderer textRenderer) =>
            {
                _renderer.DrawText(textRenderer.Text, transform.Position, textRenderer.Color, textRenderer.FontSize);
            });

            _renderer.End();
        }

        public void Update(float deltaTime)
        {
            // pass
        }
    }
}