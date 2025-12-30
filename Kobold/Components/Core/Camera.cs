using System.Numerics;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Camera component that defines the viewport and follows a target.
    /// When present, the RenderSystem will automatically transform world coordinates to screen coordinates.
    /// </summary>
    public struct Camera
    {
        public Vector2 Position;           // Camera center position in world space
        public float ViewportWidth;        // Width of the viewport in pixels
        public float ViewportHeight;       // Height of the viewport in pixels
        public float SmoothSpeed;          // Speed of smooth following (0 = instant, higher = slower)
        public Vector2 MinBounds;          // Minimum world position the camera can reach
        public Vector2 MaxBounds;          // Maximum world position the camera can reach
        public bool FollowTarget;          // Whether to follow a target entity

        public Camera(float viewportWidth, float viewportHeight, float smoothSpeed = 5f)
        {
            Position = Vector2.Zero;
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            SmoothSpeed = smoothSpeed;
            MinBounds = Vector2.Zero;
            MaxBounds = new Vector2(float.MaxValue, float.MaxValue);
            FollowTarget = true;
        }

        /// <summary>
        /// Gets the camera's viewport bounds in world space.
        /// </summary>
        public readonly (float left, float top, float right, float bottom) GetViewportBounds()
        {
            float halfWidth = ViewportWidth / 2f;
            float halfHeight = ViewportHeight / 2f;

            return (
                left: Position.X - halfWidth,
                top: Position.Y - halfHeight,
                right: Position.X + halfWidth,
                bottom: Position.Y + halfHeight
            );
        }

        /// <summary>
        /// Converts world position to screen position.
        /// </summary>
        public readonly Vector2 WorldToScreen(Vector2 worldPosition)
        {
            float halfWidth = ViewportWidth / 2f;
            float halfHeight = ViewportHeight / 2f;

            return new Vector2(
                worldPosition.X - Position.X + halfWidth,
                worldPosition.Y - Position.Y + halfHeight
            );
        }

        /// <summary>
        /// Converts screen position to world position.
        /// </summary>
        public readonly Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            float halfWidth = ViewportWidth / 2f;
            float halfHeight = ViewportHeight / 2f;

            return new Vector2(
                screenPosition.X + Position.X - halfWidth,
                screenPosition.Y + Position.Y - halfHeight
            );
        }

        /// <summary>
        /// Sets the camera bounds based on the map size.
        /// </summary>
        public void SetBounds(float mapWidth, float mapHeight)
        {
            // Clamp camera so it never shows area outside the map
            float halfViewWidth = ViewportWidth / 2f;
            float halfViewHeight = ViewportHeight / 2f;

            MinBounds = new Vector2(halfViewWidth, halfViewHeight);
            MaxBounds = new Vector2(
                mapWidth - halfViewWidth,
                mapHeight - halfViewHeight
            );
        }
    }
}
