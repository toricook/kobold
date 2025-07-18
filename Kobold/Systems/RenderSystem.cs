using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    /// <summary>
    /// Generic rendering system that handles all basic render components
    /// </summary>
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

            // Future: Add more render types here
            // - Sprites/Textures
            // - Circles
            // - Lines
            // - Particles

            _renderer.End();
        }
    }
}
