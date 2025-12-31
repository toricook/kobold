using System.Numerics;

namespace Kobold.Extensions.UI.Components
{
    /// <summary>
    /// Component defining a rectangular UI interaction boundary for an entity.
    /// This creates an axis-aligned bounding box (AABB) used by UI input systems
    /// to determine mouse hit detection and interaction.
    ///
    /// The UI bounds are positioned relative to the entity's Transform.Position,
    /// with an optional offset to fine-tune the interaction area.
    /// </summary>
    public struct UIBounds
    {
        /// <summary>
        /// The width and height of the UI interaction box in pixels.
        ///
        /// X component: Width of the interaction box
        /// Y component: Height of the interaction box
        ///
        /// This defines the interactive area for mouse input.
        ///
        /// Examples:
        /// - (200, 50) = 200 pixels wide, 50 pixels tall (like a button)
        /// - (100, 100) = 100x100 pixel square (like an icon)
        ///
        /// Values should be positive. Zero or negative values may cause undefined behavior
        /// in UI input systems.
        /// </summary>
        public Vector2 Size;

        /// <summary>
        /// Offset from the entity's Transform.Position to the UI bounds' top-left corner.
        /// This allows fine-tuning the interaction area relative to the entity's logical position.
        ///
        /// X component: Horizontal offset in pixels (positive = right, negative = left)
        /// Y component: Vertical offset in pixels (positive = down, negative = up)
        ///
        /// Common uses:
        /// - (0, 0) = UI bounds start at entity position (default)
        /// - (-Size.X/2, -Size.Y/2) = center the UI bounds on the entity position
        ///
        /// Useful when the entity's Transform.Position represents the center, but you want
        /// the interaction box positioned differently.
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// Creates a new UIBounds with the specified size and optional offset.
        /// </summary>
        /// <param name="size">Width and height of the interaction box in pixels</param>
        /// <param name="offset">Offset from entity position to interaction box (default: 0,0)</param>
        public UIBounds(Vector2 size, Vector2 offset = default)
        {
            Size = size;
            Offset = offset;
        }

        /// <summary>
        /// Creates a new UIBounds with the specified width, height, and optional offset.
        /// Convenience constructor for when you have separate width and height values.
        /// </summary>
        /// <param name="width">Width of the interaction box in pixels</param>
        /// <param name="height">Height of the interaction box in pixels</param>
        /// <param name="offset">Offset from entity position to interaction box (default: 0,0)</param>
        public UIBounds(float width, float height, Vector2 offset = default)
        {
            Size = new Vector2(width, height);
            Offset = offset;
        }

        /// <summary>
        /// Gets the actual world position of the UI bounds' top-left corner.
        /// This combines the entity's transform position with the bounds' offset.
        /// </summary>
        /// <param name="entityPosition">The entity's Transform.Position</param>
        /// <returns>World position of the UI bounds' top-left corner</returns>
        public readonly Vector2 GetWorldPosition(Vector2 entityPosition)
        {
            return entityPosition + Offset;
        }

        /// <summary>
        /// Gets the world-space bounds of this UI element.
        /// Returns the left, top, right, and bottom edges in world coordinates.
        /// </summary>
        /// <param name="entityPosition">The entity's Transform.Position</param>
        /// <returns>Tuple of (left, top, right, bottom) world coordinates</returns>
        public readonly (float left, float top, float right, float bottom) GetWorldBounds(Vector2 entityPosition)
        {
            var worldPos = GetWorldPosition(entityPosition);
            return (
                left: worldPos.X,
                top: worldPos.Y,
                right: worldPos.X + Size.X,
                bottom: worldPos.Y + Size.Y
            );
        }

        /// <summary>
        /// Gets the center point of the UI bounds in world coordinates.
        /// </summary>
        /// <param name="entityPosition">The entity's Transform.Position</param>
        /// <returns>World position of the UI bounds center</returns>
        public readonly Vector2 GetWorldCenter(Vector2 entityPosition)
        {
            return GetWorldPosition(entityPosition) + Size / 2f;
        }

        /// <summary>
        /// Checks if a world point is inside this UI bounds.
        /// Used for mouse hit detection.
        /// </summary>
        /// <param name="entityPosition">The entity's Transform.Position</param>
        /// <param name="point">World point to test (typically mouse position)</param>
        /// <returns>True if the point is inside the UI bounds</returns>
        public readonly bool Contains(Vector2 entityPosition, Vector2 point)
        {
            var (left, top, right, bottom) = GetWorldBounds(entityPosition);
            return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
        }

        /// <summary>
        /// Creates a UIBounds centered on the entity position.
        /// </summary>
        /// <param name="size">Width and height of the UI bounds</param>
        /// <returns>UIBounds centered on entity position</returns>
        public static UIBounds Centered(Vector2 size)
        {
            return new UIBounds(size, -size / 2f);
        }

        /// <summary>
        /// Returns a human-readable string representation of this UI bounds.
        /// Useful for debugging and logging.
        /// </summary>
        public override readonly string ToString()
        {
            if (Offset == Vector2.Zero)
                return $"UIBounds({Size.X}x{Size.Y})";
            else
                return $"UIBounds({Size.X}x{Size.Y}, Offset: {Offset.X}, {Offset.Y})";
        }
    }
}
