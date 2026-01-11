using Arch.Core;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;

namespace Kobold.Core.Systems
{
    /// <summary>
    /// Represents a single item to be rendered, with layer and Y-sort information
    /// </summary>
    public struct RenderableItem
    {
        /// <summary>
        /// The render layer (Background, GameObjects, UI, etc.)
        /// </summary>
        public int Layer;

        /// <summary>
        /// Whether this item participates in Y-sorting
        /// </summary>
        public bool YSort;

        /// <summary>
        /// The Y-sort value (typically the bottom Y position of the renderable)
        /// </summary>
        public float YSortValue;

        /// <summary>
        /// The action that performs the actual rendering
        /// </summary>
        public Action? RenderAction;
    }

    /// <summary>
    /// Interface for extensions to contribute custom renderable types to the RenderSystem.
    /// Implementations can collect entities and add them to the shared renderable list,
    /// allowing for unified Y-sorting with built-in renderables (sprites, rectangles, text).
    /// </summary>
    public interface IRenderableCollector
    {
        /// <summary>
        /// Collect renderables from the world and add them to the provided list.
        /// The RenderSystem will sort and render all collected items together.
        /// </summary>
        /// <param name="world">The ECS world to query entities from</param>
        /// <param name="camera">The active camera (if any) for world-to-screen conversion</param>
        /// <param name="renderables">The list to add renderable items to</param>
        void CollectRenderables(World world, Camera? camera, List<RenderableItem> renderables);
    }
}
