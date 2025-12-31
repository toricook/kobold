using System.Drawing;

namespace Kobold.Extensions.UI.Components
{
    /// <summary>
    /// Component that defines a UI panel - a container or background element.
    /// Panels are typically used for backgrounds, containers, or modal dialogs.
    ///
    /// Unlike buttons, panels don't automatically respond to interaction,
    /// but they can block clicks to UI elements behind them.
    /// </summary>
    public struct UIPanel
    {
        /// <summary>
        /// Background color of the panel.
        /// </summary>
        public Color BackgroundColor;

        /// <summary>
        /// Whether this panel should block mouse clicks to UI elements behind it.
        /// Set to true for modal dialogs or overlays that should prevent interaction
        /// with underlying UI.
        /// </summary>
        public bool BlocksClicks;

        /// <summary>
        /// Ordering value for layering multiple panels.
        /// Panels with higher Order values are rendered on top.
        /// This is separate from the render layer and is used for panel-specific layering.
        /// </summary>
        public int Order;

        /// <summary>
        /// Creates a new UIPanel with the specified properties.
        /// </summary>
        /// <param name="backgroundColor">Background color of the panel</param>
        /// <param name="blocksClicks">Whether to block clicks to elements behind this panel</param>
        /// <param name="order">Ordering value for panel layering</param>
        public UIPanel(Color backgroundColor, bool blocksClicks = false, int order = 0)
        {
            BackgroundColor = backgroundColor;
            BlocksClicks = blocksClicks;
            Order = order;
        }

        /// <summary>
        /// Creates a simple background panel that doesn't block clicks.
        /// </summary>
        /// <param name="color">Background color</param>
        /// <returns>UIPanel configured as a background</returns>
        public static UIPanel Background(Color color)
        {
            return new UIPanel(color, blocksClicks: false, order: 0);
        }

        /// <summary>
        /// Creates a modal panel that blocks clicks to elements behind it.
        /// Useful for dialog boxes and overlays.
        /// </summary>
        /// <param name="color">Background color</param>
        /// <param name="order">Panel order (higher = on top)</param>
        /// <returns>UIPanel configured as a modal</returns>
        public static UIPanel Modal(Color color, int order = 100)
        {
            return new UIPanel(color, blocksClicks: true, order: order);
        }

        /// <summary>
        /// Returns a human-readable string representation of this panel.
        /// </summary>
        public override readonly string ToString()
        {
            var blocking = BlocksClicks ? ", Blocks Clicks" : "";
            return $"UIPanel(Order: {Order}{blocking})";
        }
    }
}
