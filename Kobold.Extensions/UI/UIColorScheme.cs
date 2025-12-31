using System.Drawing;

namespace Kobold.Extensions.UI
{
    /// <summary>
    /// Predefined color schemes for UI wireframing.
    /// Provides consistent, professional-looking color palettes for buttons, panels, and text.
    ///
    /// Use these when creating UI elements for quick, cohesive styling.
    /// You can also create custom color schemes by copying and modifying these patterns.
    /// </summary>
    public static class UIColorScheme
    {
        /// <summary>
        /// Dark theme color scheme.
        /// Good for modern interfaces and reducing eye strain.
        /// </summary>
        public static class Dark
        {
            /// <summary>Dark gray button in normal state</summary>
            public static readonly Color ButtonNormal = Color.FromArgb(60, 60, 60);

            /// <summary>Lighter gray for hover state</summary>
            public static readonly Color ButtonHover = Color.FromArgb(80, 80, 80);

            /// <summary>Darker gray for pressed state</summary>
            public static readonly Color ButtonPressed = Color.FromArgb(40, 40, 40);

            /// <summary>Desaturated gray for disabled state</summary>
            public static readonly Color ButtonDisabled = Color.FromArgb(50, 50, 50);

            /// <summary>Very dark gray for panel backgrounds</summary>
            public static readonly Color Panel = Color.FromArgb(30, 30, 30);

            /// <summary>Slightly lighter panel for layering</summary>
            public static readonly Color PanelLight = Color.FromArgb(45, 45, 45);

            /// <summary>White text for good contrast on dark backgrounds</summary>
            public static readonly Color Text = Color.White;

            /// <summary>Dimmed text for secondary information</summary>
            public static readonly Color TextSecondary = Color.FromArgb(180, 180, 180);

            /// <summary>Accent color for highlights and important actions</summary>
            public static readonly Color Accent = Color.FromArgb(100, 150, 255);

            /// <summary>Success/positive action color (green)</summary>
            public static readonly Color Success = Color.FromArgb(80, 200, 120);

            /// <summary>Warning/caution color (yellow)</summary>
            public static readonly Color Warning = Color.FromArgb(255, 200, 80);

            /// <summary>Error/destructive action color (red)</summary>
            public static readonly Color Error = Color.FromArgb(220, 80, 80);
        }

        /// <summary>
        /// Light theme color scheme.
        /// Traditional, clean look with good readability.
        /// </summary>
        public static class Light
        {
            /// <summary>Light gray button in normal state</summary>
            public static readonly Color ButtonNormal = Color.FromArgb(220, 220, 220);

            /// <summary>Brighter for hover state</summary>
            public static readonly Color ButtonHover = Color.FromArgb(235, 235, 235);

            /// <summary>Slightly darker for pressed state</summary>
            public static readonly Color ButtonPressed = Color.FromArgb(200, 200, 200);

            /// <summary>Muted gray for disabled state</summary>
            public static readonly Color ButtonDisabled = Color.FromArgb(210, 210, 210);

            /// <summary>Very light gray for panel backgrounds</summary>
            public static readonly Color Panel = Color.FromArgb(245, 245, 245);

            /// <summary>White panel for layering</summary>
            public static readonly Color PanelLight = Color.White;

            /// <summary>Black text for good contrast on light backgrounds</summary>
            public static readonly Color Text = Color.Black;

            /// <summary>Gray text for secondary information</summary>
            public static readonly Color TextSecondary = Color.FromArgb(100, 100, 100);

            /// <summary>Accent color for highlights and important actions</summary>
            public static readonly Color Accent = Color.FromArgb(50, 100, 200);

            /// <summary>Success/positive action color (green)</summary>
            public static readonly Color Success = Color.FromArgb(60, 180, 100);

            /// <summary>Warning/caution color (orange)</summary>
            public static readonly Color Warning = Color.FromArgb(230, 150, 50);

            /// <summary>Error/destructive action color (red)</summary>
            public static readonly Color Error = Color.FromArgb(200, 60, 60);
        }

        /// <summary>
        /// Blue theme color scheme.
        /// Vibrant and modern with blue accents.
        /// </summary>
        public static class Blue
        {
            /// <summary>Blue button in normal state</summary>
            public static readonly Color ButtonNormal = Color.FromArgb(70, 130, 200);

            /// <summary>Brighter blue for hover state</summary>
            public static readonly Color ButtonHover = Color.FromArgb(90, 150, 220);

            /// <summary>Darker blue for pressed state</summary>
            public static readonly Color ButtonPressed = Color.FromArgb(50, 110, 180);

            /// <summary>Desaturated blue for disabled state</summary>
            public static readonly Color ButtonDisabled = Color.FromArgb(120, 140, 160);

            /// <summary>Dark blue-gray panel background</summary>
            public static readonly Color Panel = Color.FromArgb(40, 50, 70);

            /// <summary>Lighter blue-gray panel for layering</summary>
            public static readonly Color PanelLight = Color.FromArgb(55, 65, 85);

            /// <summary>White text</summary>
            public static readonly Color Text = Color.White;

            /// <summary>Light blue-gray text for secondary information</summary>
            public static readonly Color TextSecondary = Color.FromArgb(160, 180, 200);

            /// <summary>Bright blue accent</summary>
            public static readonly Color Accent = Color.FromArgb(100, 180, 255);

            /// <summary>Success color (cyan)</summary>
            public static readonly Color Success = Color.FromArgb(80, 220, 200);

            /// <summary>Warning color (amber)</summary>
            public static readonly Color Warning = Color.FromArgb(255, 180, 80);

            /// <summary>Error color (pink-red)</summary>
            public static readonly Color Error = Color.FromArgb(255, 100, 120);
        }
    }
}
