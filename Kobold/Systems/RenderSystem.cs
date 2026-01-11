using Arch.Core;
using Kobold.Core.Abstractions.Engine;
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
        private readonly List<IRenderableCollector> _customCollectors = new();

        public RenderSystem(IRenderer renderer, World world)
        {
            _renderer = renderer;
            _world = world;
        }

        /// <summary>
        /// Register a custom renderable collector that can contribute renderables to the unified render pipeline.
        /// This allows extensions to add new renderable types (e.g., tilemaps, particles) that participate in
        /// layer-based and Y-sorted rendering alongside built-in renderables.
        /// </summary>
        public void RegisterCollector(IRenderableCollector collector)
        {
            if (collector == null)
                throw new ArgumentNullException(nameof(collector));

            _customCollectors.Add(collector);
        }

        public void Render()
        {
            // Note: Begin/End are called by GameEngineBase, not by individual render systems

            // Get camera (if exists)
            Camera? camera = GetCamera();

            // Collect all renderables (built-in + custom collectors)
            var renderables = new List<RenderableItem>();

            // Collect built-in renderables
            CollectRectangles(camera, renderables);
            CollectText(camera, renderables);
            CollectSprites(camera, renderables);

            // Allow custom collectors to contribute
            foreach (var collector in _customCollectors)
            {
                collector.CollectRenderables(_world, camera, renderables);
            }

            // Sort by layer first, then by Y position if Y-sorting is enabled
            renderables.Sort((a, b) =>
            {
                // Primary sort: layer
                int layerCompare = a.Layer.CompareTo(b.Layer);
                if (layerCompare != 0)
                    return layerCompare;

                // Secondary sort: Y position (only matters if both have YSort enabled)
                if (a.YSort && b.YSort)
                    return a.YSortValue.CompareTo(b.YSortValue);

                // If only one has YSort, non-YSort renders first (behind)
                if (a.YSort && !b.YSort)
                    return 1;
                if (!a.YSort && b.YSort)
                    return -1;

                return 0;
            });

            // Render in sorted order
            foreach (var renderable in renderables)
            {
                renderable.RenderAction?.Invoke();
            }
        }

        private void CollectRectangles(Camera? camera, List<RenderableItem> renderables)
        {
            var query = new QueryDescription().WithAll<Transform, RectangleRenderer>();
            _world.Query(in query, (Entity entity, ref Transform transform, ref RectangleRenderer rectangleRenderer) =>
            {
                // Calculate Y position for Y-sorting (bottom of rectangle)
                float ySortValue = rectangleRenderer.YSort
                    ? transform.Position.Y + rectangleRenderer.Size.Y
                    : 0f;

                // Convert world position to screen position
                bool isUILayer = rectangleRenderer.Layer >= RenderLayers.UI;
                Vector2 screenPosition = (camera.HasValue && !isUILayer)
                    ? camera.Value.WorldToScreen(transform.Position)
                    : transform.Position;

                // Round to integer pixel coordinates
                screenPosition = new Vector2(MathF.Round(screenPosition.X), MathF.Round(screenPosition.Y));

                // Copy values for lambda capture
                var size = rectangleRenderer.Size;
                var color = rectangleRenderer.Color;

                renderables.Add(new RenderableItem
                {
                    Layer = rectangleRenderer.Layer,
                    YSort = rectangleRenderer.YSort,
                    YSortValue = ySortValue,
                    RenderAction = () => _renderer.DrawRectangle(screenPosition, size, color)
                });
            });
        }

        private void CollectText(Camera? camera, List<RenderableItem> renderables)
        {
            var query = new QueryDescription().WithAll<Transform, TextRenderer>();
            _world.Query(in query, (Entity entity, ref Transform transform, ref TextRenderer textRenderer) =>
            {
                // Convert world position to screen position
                bool isUILayer = textRenderer.Layer >= RenderLayers.UI;
                Vector2 screenPosition = (camera.HasValue && !isUILayer)
                    ? camera.Value.WorldToScreen(transform.Position)
                    : transform.Position;

                // Round to integer pixel coordinates
                screenPosition = new Vector2(MathF.Round(screenPosition.X), MathF.Round(screenPosition.Y));

                // Copy values for lambda capture
                var text = textRenderer.Text;
                var color = textRenderer.Color;
                var fontSize = textRenderer.FontSize;

                renderables.Add(new RenderableItem
                {
                    Layer = textRenderer.Layer,
                    YSort = false, // Text typically doesn't use Y-sorting
                    YSortValue = 0f,
                    RenderAction = () => _renderer.DrawText(text, screenPosition, color, fontSize)
                });
            });
        }

        private void CollectSprites(Camera? camera, List<RenderableItem> renderables)
        {
            var query = new QueryDescription().WithAll<Transform, SpriteRenderer>();
            _world.Query(in query, (Entity entity, ref Transform transform, ref SpriteRenderer spriteRenderer) =>
            {
                // Calculate Y position for Y-sorting (bottom of sprite)
                float ySortValue = spriteRenderer.YSort
                    ? transform.Position.Y + spriteRenderer.SourceRect.Height * spriteRenderer.Scale.Y
                    : 0f;

                // Convert world position to screen position
                bool isUILayer = spriteRenderer.Layer >= RenderLayers.UI;
                Vector2 screenPosition = (camera.HasValue && !isUILayer)
                    ? camera.Value.WorldToScreen(transform.Position)
                    : transform.Position;

                // Round to integer pixel coordinates
                screenPosition = new Vector2(MathF.Round(screenPosition.X), MathF.Round(screenPosition.Y));

                // Copy values for lambda capture
                var texture = spriteRenderer.Texture;
                var sourceRect = spriteRenderer.SourceRect;
                var scale = spriteRenderer.Scale;
                var rotation = spriteRenderer.Rotation;
                var tint = spriteRenderer.Tint;

                renderables.Add(new RenderableItem
                {
                    Layer = spriteRenderer.Layer,
                    YSort = spriteRenderer.YSort,
                    YSortValue = ySortValue,
                    RenderAction = () => _renderer.DrawSprite(texture, screenPosition, sourceRect, scale, rotation, tint)
                });
            });
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
    }
}
