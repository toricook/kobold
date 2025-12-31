using System.Drawing;

namespace Kobold.Extensions.UI.Components
{
    /// <summary>
    /// Component that defines button-specific properties and visual states.
    /// This component works together with UIInteractive to provide visual feedback
    /// for different interaction states (normal, hover, pressed, disabled).
    ///
    /// For wireframe mode, the colors define the button's appearance in each state.
    /// When using sprites, these colors can be used as tint multipliers.
    /// </summary>
    public struct UIButton
    {
        /// <summary>
        /// Optional identifier for this button.
        /// Useful for distinguishing buttons in event handlers.
        /// If not specified, a unique ID will be generated.
        /// </summary>
        public string Id;

        /// <summary>
        /// Color when the button is in normal state (not hovered or pressed).
        /// </summary>
        public Color NormalColor;

        /// <summary>
        /// Color when the mouse is hovering over the button.
        /// </summary>
        public Color HoverColor;

        /// <summary>
        /// Color when the button is being pressed down.
        /// </summary>
        public Color PressedColor;

        /// <summary>
        /// Color when the button is disabled (UIInteractive.IsEnabled = false).
        /// </summary>
        public Color DisabledColor;

        /// <summary>
        /// Creates a new UIButton with the specified ID and colors.
        /// </summary>
        /// <param name="id">Unique identifier for this button</param>
        /// <param name="normalColor">Color in normal state</param>
        /// <param name="hoverColor">Color when hovered</param>
        /// <param name="pressedColor">Color when pressed</param>
        /// <param name="disabledColor">Color when disabled</param>
        public UIButton(
            string id,
            Color normalColor,
            Color hoverColor,
            Color pressedColor,
            Color disabledColor)
        {
            Id = id;
            NormalColor = normalColor;
            HoverColor = hoverColor;
            PressedColor = pressedColor;
            DisabledColor = disabledColor;
        }

        /// <summary>
        /// Creates a new UIButton with a single base color.
        /// Hover, pressed, and disabled colors are automatically derived.
        /// </summary>
        /// <param name="id">Unique identifier for this button</param>
        /// <param name="baseColor">Base color (normal state)</param>
        /// <returns>UIButton with derived state colors</returns>
        public static UIButton WithDerivedColors(string id, Color baseColor)
        {
            return new UIButton
            {
                Id = id,
                NormalColor = baseColor,
                HoverColor = ColorUtils.Lighten(baseColor, 0.15f),
                PressedColor = ColorUtils.Darken(baseColor, 0.15f),
                DisabledColor = ColorUtils.Desaturate(baseColor, 0.5f)
            };
        }

        /// <summary>
        /// Returns a human-readable string representation of this button.
        /// </summary>
        public override readonly string ToString()
        {
            return $"UIButton({Id})";
        }
    }

    /// <summary>
    /// Utility methods for color manipulation.
    /// Used to derive button state colors from a base color.
    /// </summary>
    public static class ColorUtils
    {
        /// <summary>
        /// Lightens a color by the specified amount.
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="amount">Amount to lighten (0.0 to 1.0)</param>
        /// <returns>Lightened color</returns>
        public static Color Lighten(Color color, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            return Color.FromArgb(
                color.A,
                (int)Math.Clamp(color.R + (255 - color.R) * amount, 0, 255),
                (int)Math.Clamp(color.G + (255 - color.G) * amount, 0, 255),
                (int)Math.Clamp(color.B + (255 - color.B) * amount, 0, 255)
            );
        }

        /// <summary>
        /// Darkens a color by the specified amount.
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="amount">Amount to darken (0.0 to 1.0)</param>
        /// <returns>Darkened color</returns>
        public static Color Darken(Color color, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            return Color.FromArgb(
                color.A,
                (int)(color.R * (1f - amount)),
                (int)(color.G * (1f - amount)),
                (int)(color.B * (1f - amount))
            );
        }

        /// <summary>
        /// Desaturates a color by moving it toward grayscale.
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="amount">Amount to desaturate (0.0 = no change, 1.0 = full grayscale)</param>
        /// <returns>Desaturated color</returns>
        public static Color Desaturate(Color color, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            int gray = (int)(color.R * 0.299f + color.G * 0.587f + color.B * 0.114f);
            return Color.FromArgb(
                color.A,
                (int)(color.R + (gray - color.R) * amount),
                (int)(color.G + (gray - color.G) * amount),
                (int)(color.B + (gray - color.B) * amount)
            );
        }
    }
}
