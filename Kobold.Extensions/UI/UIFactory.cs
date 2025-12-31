using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Components;
using Kobold.Extensions.UI.Components;
using System.Drawing;
using System.Numerics;

namespace Kobold.Extensions.UI
{
    /// <summary>
    /// Factory class for creating common UI elements.
    /// Provides convenient methods to create buttons, panels, and other UI components
    /// with sensible defaults.
    /// </summary>
    public static class UIFactory
    {
        /// <summary>
        /// Creates a clickable button entity with visual state feedback.
        /// The button will change color based on interaction (hover, pressed, disabled).
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="position">Position of the button (top-left corner)</param>
        /// <param name="size">Size of the button (width and height)</param>
        /// <param name="text">Text to display on the button</param>
        /// <param name="normalColor">Background color in normal state</param>
        /// <param name="buttonId">Optional unique identifier for the button</param>
        /// <param name="textColor">Color of the button text (default: White)</param>
        /// <param name="fontSize">Font size of the text (default: 16)</param>
        /// <param name="layer">Render layer (default: UI layer)</param>
        /// <returns>The created button entity</returns>
        public static Entity CreateButton(
            World world,
            Vector2 position,
            Vector2 size,
            string text,
            Color normalColor,
            string? buttonId = null,
            Color? textColor = null,
            float fontSize = 16f,
            int layer = RenderLayers.UI)
        {
            // Use white text by default
            var finalTextColor = textColor ?? Color.White;

            // Generate button ID if not provided
            var finalButtonId = buttonId ?? Guid.NewGuid().ToString();

            // Create button with automatic color derivation
            var button = UIButton.WithDerivedColors(finalButtonId, normalColor);

            // Create the button entity with all necessary components
            var buttonEntity = world.Create(
                new Transform(position),
                new UIBounds(size, Vector2.Zero),
                new UIInteractive(isEnabled: true),
                button,
                RectangleRenderer.UI(size, normalColor),
                TextRenderer.UIText(text, finalTextColor, fontSize)
            );

            return buttonEntity;
        }

        /// <summary>
        /// Creates a panel entity - a non-interactive background or container.
        /// Panels are useful for backgrounds, grouping UI elements, or modal overlays.
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="position">Position of the panel (top-left corner)</param>
        /// <param name="size">Size of the panel (width and height)</param>
        /// <param name="backgroundColor">Background color of the panel</param>
        /// <param name="blocksClicks">Whether this panel blocks clicks to elements behind it (default: false)</param>
        /// <param name="order">Panel ordering for layering (higher = on top, default: 0)</param>
        /// <param name="layer">Render layer (default: UI layer)</param>
        /// <returns>The created panel entity</returns>
        public static Entity CreatePanel(
            World world,
            Vector2 position,
            Vector2 size,
            Color backgroundColor,
            bool blocksClicks = false,
            int order = 0,
            int layer = RenderLayers.UI)
        {
            var panelEntity = world.Create(
                new Transform(position),
                new UIPanel(backgroundColor, blocksClicks, order),
                new UIBounds(size, Vector2.Zero), // Always add UIBounds for proper anchoring
                RectangleRenderer.UI(size, backgroundColor)
            );

            // If panel blocks clicks, add UIInteractive (but disabled)
            // This ensures the input system detects it but doesn't make it clickable
            if (blocksClicks)
            {
                world.Add(panelEntity, UIInteractive.Disabled());
            }

            return panelEntity;
        }

        /// <summary>
        /// Adds an anchor component to an existing entity.
        /// This will make the entity's position update based on screen size.
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="entity">The entity to anchor</param>
        /// <param name="anchorPoint">The anchor point on the screen</param>
        /// <param name="offset">Offset from the anchor point (default: 0,0)</param>
        public static void AddAnchor(World world, Entity entity, AnchorPoint anchorPoint, Vector2 offset = default)
        {
            world.Add(entity, new UIAnchor(anchorPoint, offset));
        }

        /// <summary>
        /// Creates a button anchored to a specific screen position.
        /// Combines CreateButton and AddAnchor in one convenient call.
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="anchorPoint">Where to anchor the button on screen</param>
        /// <param name="offset">Offset from anchor point</param>
        /// <param name="size">Size of the button</param>
        /// <param name="text">Button text</param>
        /// <param name="normalColor">Button color</param>
        /// <param name="buttonId">Optional button ID</param>
        /// <param name="textColor">Text color (default: White)</param>
        /// <param name="fontSize">Font size (default: 16)</param>
        /// <returns>The created anchored button entity</returns>
        public static Entity CreateAnchoredButton(
            World world,
            AnchorPoint anchorPoint,
            Vector2 offset,
            Vector2 size,
            string text,
            Color normalColor,
            string? buttonId = null,
            Color? textColor = null,
            float fontSize = 16f)
        {
            var button = CreateButton(world, Vector2.Zero, size, text, normalColor, buttonId, textColor, fontSize);
            AddAnchor(world, button, anchorPoint, offset);
            return button;
        }

        /// <summary>
        /// Creates a panel anchored to a specific screen position.
        /// Combines CreatePanel and AddAnchor in one convenient call.
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="anchorPoint">Where to anchor the panel on screen</param>
        /// <param name="offset">Offset from anchor point</param>
        /// <param name="size">Size of the panel</param>
        /// <param name="backgroundColor">Panel background color</param>
        /// <param name="blocksClicks">Whether panel blocks clicks (default: false)</param>
        /// <param name="order">Panel order (default: 0)</param>
        /// <returns>The created anchored panel entity</returns>
        public static Entity CreateAnchoredPanel(
            World world,
            AnchorPoint anchorPoint,
            Vector2 offset,
            Vector2 size,
            Color backgroundColor,
            bool blocksClicks = false,
            int order = 0)
        {
            var panel = CreatePanel(world, Vector2.Zero, size, backgroundColor, blocksClicks, order);
            AddAnchor(world, panel, anchorPoint, offset);
            return panel;
        }
    }
}
