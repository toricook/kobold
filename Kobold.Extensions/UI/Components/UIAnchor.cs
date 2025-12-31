using System.Numerics;

namespace Kobold.Extensions.UI.Components
{
    /// <summary>
    /// Component that anchors a UI element to a specific point on the screen.
    /// This allows UI to be positioned relative to screen edges or center,
    /// making layouts responsive to different screen sizes.
    ///
    /// The UIAnchorSystem will automatically update the entity's Transform.Position
    /// based on the anchor point and screen size.
    /// </summary>
    public struct UIAnchor
    {
        /// <summary>
        /// The anchor point on the screen to position this element relative to.
        /// </summary>
        public AnchorPoint AnchorPoint;

        /// <summary>
        /// Offset from the anchor point in pixels.
        /// Positive X moves right, positive Y moves down.
        ///
        /// Examples:
        /// - (0, 0) = exactly at the anchor point
        /// - (10, 10) = 10 pixels right and down from anchor
        /// - (-10, -10) = 10 pixels left and up from anchor
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// Creates a new UIAnchor with the specified anchor point and offset.
        /// </summary>
        /// <param name="anchorPoint">The anchor point on the screen</param>
        /// <param name="offset">Offset from the anchor point in pixels</param>
        public UIAnchor(AnchorPoint anchorPoint, Vector2 offset = default)
        {
            AnchorPoint = anchorPoint;
            Offset = offset;
        }

        /// <summary>
        /// Calculates the world position for this anchor based on screen size.
        /// This is called by UIAnchorSystem to position the entity.
        /// </summary>
        /// <param name="screenSize">Current screen/viewport size</param>
        /// <returns>World position for the entity</returns>
        public readonly Vector2 GetAnchoredPosition(Vector2 screenSize)
        {
            Vector2 basePosition = AnchorPoint switch
            {
                AnchorPoint.TopLeft => new Vector2(0, 0),
                AnchorPoint.TopCenter => new Vector2(screenSize.X / 2f, 0),
                AnchorPoint.TopRight => new Vector2(screenSize.X, 0),

                AnchorPoint.MiddleLeft => new Vector2(0, screenSize.Y / 2f),
                AnchorPoint.Center => new Vector2(screenSize.X / 2f, screenSize.Y / 2f),
                AnchorPoint.MiddleRight => new Vector2(screenSize.X, screenSize.Y / 2f),

                AnchorPoint.BottomLeft => new Vector2(0, screenSize.Y),
                AnchorPoint.BottomCenter => new Vector2(screenSize.X / 2f, screenSize.Y),
                AnchorPoint.BottomRight => new Vector2(screenSize.X, screenSize.Y),

                _ => Vector2.Zero
            };

            return basePosition + Offset;
        }

        /// <summary>
        /// Returns a human-readable string representation of this anchor.
        /// </summary>
        public override readonly string ToString()
        {
            if (Offset == Vector2.Zero)
                return $"UIAnchor({AnchorPoint})";
            else
                return $"UIAnchor({AnchorPoint}, Offset: {Offset.X}, {Offset.Y})";
        }
    }

    /// <summary>
    /// Defines the nine standard anchor points on a screen or viewport.
    /// These represent the corners, edges, and center of the screen.
    /// </summary>
    public enum AnchorPoint
    {
        /// <summary>Top-left corner (0, 0)</summary>
        TopLeft,

        /// <summary>Top edge, centered horizontally</summary>
        TopCenter,

        /// <summary>Top-right corner</summary>
        TopRight,

        /// <summary>Left edge, centered vertically</summary>
        MiddleLeft,

        /// <summary>Center of screen, both horizontally and vertically</summary>
        Center,

        /// <summary>Right edge, centered vertically</summary>
        MiddleRight,

        /// <summary>Bottom-left corner</summary>
        BottomLeft,

        /// <summary>Bottom edge, centered horizontally</summary>
        BottomCenter,

        /// <summary>Bottom-right corner</summary>
        BottomRight
    }
}
