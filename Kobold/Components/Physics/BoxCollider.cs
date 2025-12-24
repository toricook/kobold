using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Component defining a rectangular collision boundary for an entity.
    /// This creates an axis-aligned bounding box (AABB) used by collision detection systems
    /// to determine when entities are overlapping or touching.
    /// 
    /// The collision box is positioned relative to the entity's Transform.Position,
    /// with an optional offset to fine-tune the collision area. The box doesn't rotate
    /// with the entity - it's always axis-aligned for performance.
    /// </summary>
    public struct BoxCollider
    {
        /// <summary>
        /// The width and height of the collision box in pixels.
        /// 
        /// X component: Width of the collision box
        /// Y component: Height of the collision box
        /// 
        /// This defines the collision area regardless of any visual scaling or rotation.
        /// Should match the logical size of the entity for collision purposes.
        /// 
        /// Examples:
        /// - (20, 20) = 20x20 pixel square collision box
        /// - (50, 10) = 50 pixels wide, 10 pixels tall (like a paddle)
        /// - (5, 100) = 5 pixels wide, 100 pixels tall (like a thin laser beam)
        /// 
        /// Values should be positive. Zero or negative values may cause undefined behavior
        /// in collision detection systems.
        /// </summary>
        public Vector2 Size;

        /// <summary>
        /// Offset from the entity's Transform.Position to the collision box's top-left corner.
        /// This allows fine-tuning the collision area relative to the entity's logical position.
        /// 
        /// X component: Horizontal offset in pixels (positive = right, negative = left)
        /// Y component: Vertical offset in pixels (positive = down, negative = up)
        /// 
        /// Common uses:
        /// - (0, 0) = collision box starts at entity position (default)
        /// - (-Size.X/2, -Size.Y/2) = center the collision box on the entity position
        /// - (5, 10) = move collision box 5 pixels right and 10 pixels down from entity position
        /// 
        /// Useful when the entity's Transform.Position represents the center, but you want
        /// the collision box positioned differently, or when the visual sprite doesn't
        /// perfectly match the collision area you want.
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// Creates a new BoxCollider with the specified size and optional offset.
        /// </summary>
        /// <param name="size">Width and height of the collision box in pixels</param>
        /// <param name="offset">Offset from entity position to collision box (default: 0,0)</param>
        public BoxCollider(Vector2 size, Vector2 offset = default)
        {
            Size = size;
            Offset = offset;
        }

        /// <summary>
        /// Creates a new BoxCollider with the specified width, height, and optional offset.
        /// Convenience constructor for when you have separate width and height values.
        /// </summary>
        /// <param name="width">Width of the collision box in pixels</param>
        /// <param name="height">Height of the collision box in pixels</param>
        /// <param name="offset">Offset from entity position to collision box (default: 0,0)</param>
        public BoxCollider(float width, float height, Vector2 offset = default)
        {
            Size = new Vector2(width, height);
            Offset = offset;
        }

        /// <summary>
        /// Gets the actual world position of the collision box's top-left corner.
        /// This combines the entity's transform position with the collider's offset.
        /// </summary>
        /// <param name="entityPosition">The entity's Transform.Position</param>
        /// <returns>World position of the collision box's top-left corner</returns>
        public readonly Vector2 GetWorldPosition(Vector2 entityPosition)
        {
            return entityPosition + Offset;
        }

        /// <summary>
        /// Gets the world-space bounds of this collision box.
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
        /// Gets the center point of the collision box in world coordinates.
        /// </summary>
        /// <param name="entityPosition">The entity's Transform.Position</param>
        /// <returns>World position of the collision box center</returns>
        public readonly Vector2 GetWorldCenter(Vector2 entityPosition)
        {
            return GetWorldPosition(entityPosition) + Size / 2f;
        }

        /// <summary>
        /// Checks if a world point is inside this collision box.
        /// </summary>
        /// <param name="entityPosition">The entity's Transform.Position</param>
        /// <param name="point">World point to test</param>
        /// <returns>True if the point is inside the collision box</returns>
        public readonly bool Contains(Vector2 entityPosition, Vector2 point)
        {
            var (left, top, right, bottom) = GetWorldBounds(entityPosition);
            return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
        }

        /// <summary>
        /// Checks if this collision box overlaps with another collision box.
        /// Both boxes are positioned using their respective entity positions.
        /// </summary>
        /// <param name="entityPosition">This entity's Transform.Position</param>
        /// <param name="other">The other collision box</param>
        /// <param name="otherEntityPosition">The other entity's Transform.Position</param>
        /// <returns>True if the collision boxes overlap</returns>
        public readonly bool Overlaps(Vector2 entityPosition, BoxCollider other, Vector2 otherEntityPosition)
        {
            var (left1, top1, right1, bottom1) = GetWorldBounds(entityPosition);
            var (left2, top2, right2, bottom2) = other.GetWorldBounds(otherEntityPosition);

            return left1 <= right2 && right1 >= left2 && top1 <= bottom2 && bottom1 >= top2;
        }

        /// <summary>
        /// Creates a BoxCollider that matches the size of a rendered rectangle.
        /// Useful when you want collision to exactly match the visual representation.
        /// </summary>
        /// <param name="renderSize">Size of the rendered rectangle</param>
        /// <param name="centered">If true, centers the collision box on the entity position</param>
        /// <returns>BoxCollider matching the render size</returns>
        public static BoxCollider FromRenderSize(Vector2 renderSize, bool centered = false)
        {
            var offset = centered ? -renderSize / 2f : Vector2.Zero;
            return new BoxCollider(renderSize, offset);
        }

        /// <summary>
        /// Creates a square BoxCollider with the specified size.
        /// </summary>
        /// <param name="size">Width and height of the square collision box</param>
        /// <param name="centered">If true, centers the collision box on the entity position</param>
        /// <returns>Square BoxCollider</returns>
        public static BoxCollider Square(float size, bool centered = false)
        {
            return FromRenderSize(new Vector2(size, size), centered);
        }

        /// <summary>
        /// Creates a BoxCollider slightly smaller than the render size.
        /// Useful for making collisions feel more forgiving in games.
        /// </summary>
        /// <param name="renderSize">Size of the rendered rectangle</param>
        /// <param name="shrinkAmount">Amount to shrink the collision box on all sides</param>
        /// <param name="centered">If true, centers the collision box on the entity position</param>
        /// <returns>Shrunken BoxCollider</returns>
        public static BoxCollider Shrunken(Vector2 renderSize, float shrinkAmount, bool centered = false)
        {
            var collisionSize = new Vector2(
                Math.Max(1, renderSize.X - shrinkAmount * 2),
                Math.Max(1, renderSize.Y - shrinkAmount * 2)
            );
            var offset = centered ? -collisionSize / 2f : new Vector2(shrinkAmount, shrinkAmount);
            return new BoxCollider(collisionSize, offset);
        }

        /// <summary>
        /// Returns a human-readable string representation of this collision box.
        /// Useful for debugging and logging.
        /// </summary>
        public override readonly string ToString()
        {
            if (Offset == Vector2.Zero)
                return $"BoxCollider({Size.X}x{Size.Y})";
            else
                return $"BoxCollider({Size.X}x{Size.Y}, Offset: {Offset.X}, {Offset.Y})";
        }
    }
}
