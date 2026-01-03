using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Extensions.UI.Components;
using System.Numerics;

namespace Kobold.Extensions.UI.Systems
{
    /// <summary>
    /// System that positions UI elements based on their UIAnchor component.
    /// Updates Transform.Position to match the anchored position relative to screen size.
    ///
    /// Call SetScreenSize() when the window/viewport is resized to update all anchored elements.
    /// </summary>
    public class UIAnchorSystem : ISystem
    {
        private readonly World _world;
        private Vector2 _screenSize;
        private bool _needsUpdate;

        /// <summary>
        /// Creates a new UIAnchorSystem.
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="initialScreenSize">Initial screen/viewport size</param>
        public UIAnchorSystem(World world, Vector2 initialScreenSize)
        {
            _world = world;
            _screenSize = initialScreenSize;
            _needsUpdate = true; // Update on first frame
        }

        /// <summary>
        /// Updates the screen size and marks anchored elements for repositioning.
        /// Call this when the window or viewport is resized.
        /// </summary>
        /// <param name="screenSize">New screen/viewport size</param>
        public void SetScreenSize(Vector2 screenSize)
        {
            if (_screenSize != screenSize)
            {
                _screenSize = screenSize;
                _needsUpdate = true;
            }
        }

        /// <summary>
        /// Gets the current screen size.
        /// </summary>
        public Vector2 GetScreenSize() => _screenSize;

        public void Update(float deltaTime)
        {
            // Update every frame to handle dynamically created UI elements
            // This ensures newly created anchored entities are positioned immediately
            UpdateAnchoredPositions();
        }

        /// <summary>
        /// Updates all anchored UI elements to their correct positions.
        /// </summary>
        private void UpdateAnchoredPositions()
        {
            // Handle entities with UIBounds (need to center based on size)
            var queryWithBounds = new QueryDescription().WithAll<Transform, UIAnchor, UIBounds>();
            _world.Query(in queryWithBounds, (ref Transform transform, ref UIAnchor anchor, ref UIBounds bounds) =>
            {
                Vector2 anchorPos = anchor.GetAnchoredPosition(_screenSize);

                // Adjust position based on anchor point to properly center/align the element
                // For center-based anchors, offset by half the size so the element is truly centered
                Vector2 sizeOffset = anchor.AnchorPoint switch
                {
                    AnchorPoint.Center => new Vector2(-bounds.Size.X / 2f, -bounds.Size.Y / 2f),
                    AnchorPoint.TopCenter => new Vector2(-bounds.Size.X / 2f, 0),
                    AnchorPoint.BottomCenter => new Vector2(-bounds.Size.X / 2f, -bounds.Size.Y),
                    AnchorPoint.MiddleLeft => new Vector2(0, -bounds.Size.Y / 2f),
                    AnchorPoint.MiddleRight => new Vector2(-bounds.Size.X, -bounds.Size.Y / 2f),
                    AnchorPoint.TopRight => new Vector2(-bounds.Size.X, 0),
                    AnchorPoint.BottomLeft => new Vector2(0, -bounds.Size.Y),
                    AnchorPoint.BottomRight => new Vector2(-bounds.Size.X, -bounds.Size.Y),
                    _ => Vector2.Zero // TopLeft needs no adjustment
                };

                transform.Position = anchorPos + sizeOffset;
            });

            // Handle entities without UIBounds (e.g., text-only elements)
            var queryWithoutBounds = new QueryDescription().WithAll<Transform, UIAnchor>().WithNone<UIBounds>();
            _world.Query(in queryWithoutBounds, (ref Transform transform, ref UIAnchor anchor) =>
            {
                transform.Position = anchor.GetAnchoredPosition(_screenSize);
            });
        }

        /// <summary>
        /// Forces an immediate update of all anchored positions.
        /// Useful when adding new anchored elements that need immediate positioning.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateAnchoredPositions();
        }
    }
}
