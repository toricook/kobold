namespace Kobold.Extensions.UI.Components
{
    /// <summary>
    /// Component that tracks the interaction state of a UI element.
    /// This component is updated by UIInputSystem based on mouse input and hit detection.
    ///
    /// Add this component to any entity that should respond to mouse interaction
    /// (buttons, clickable panels, etc.).
    /// </summary>
    public struct UIInteractive
    {
        /// <summary>
        /// True if the mouse is currently hovering over this UI element.
        /// Updated by UIInputSystem each frame based on mouse position and UIBounds.
        /// </summary>
        public bool IsHovered;

        /// <summary>
        /// True if the mouse button is currently being held down while over this element.
        /// Becomes true when mouse is pressed down while hovering, becomes false when released.
        /// </summary>
        public bool IsPressed;

        /// <summary>
        /// True for one frame when this element is clicked (mouse down and up while hovering).
        /// This is the most common state to check for button actions.
        ///
        /// Automatically reset to false by UIInputSystem each frame before processing input.
        /// </summary>
        public bool WasClicked;

        /// <summary>
        /// Whether this UI element can be interacted with.
        /// When false, the element will not respond to mouse input.
        /// Useful for disabled buttons or UI states.
        /// </summary>
        public bool IsEnabled;

        /// <summary>
        /// Internal state tracking for click detection.
        /// Tracks whether the element was pressed last frame to detect release events.
        /// </summary>
        internal bool WasPressedLastFrame;

        /// <summary>
        /// Creates a new UIInteractive component with the specified enabled state.
        /// </summary>
        /// <param name="isEnabled">Whether the element starts enabled (default: true)</param>
        public UIInteractive(bool isEnabled = true)
        {
            IsHovered = false;
            IsPressed = false;
            WasClicked = false;
            IsEnabled = isEnabled;
            WasPressedLastFrame = false;
        }

        /// <summary>
        /// Creates a disabled UIInteractive component.
        /// Useful for creating buttons that start in a disabled state.
        /// </summary>
        public static UIInteractive Disabled()
        {
            return new UIInteractive(isEnabled: false);
        }

        /// <summary>
        /// Returns a human-readable string representation of this interaction state.
        /// Useful for debugging UI behavior.
        /// </summary>
        public override readonly string ToString()
        {
            if (!IsEnabled)
                return "UIInteractive(Disabled)";

            var states = new List<string>();
            if (IsHovered) states.Add("Hovered");
            if (IsPressed) states.Add("Pressed");
            if (WasClicked) states.Add("Clicked");

            return states.Count > 0
                ? $"UIInteractive({string.Join(", ", states)})"
                : "UIInteractive(Idle)";
        }
    }
}
