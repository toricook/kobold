using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.UI.Components;
using Kobold.Extensions.UI.Events;

namespace Kobold.Extensions.UI.Systems
{
    /// <summary>
    /// System that manages button visual states based on interaction.
    /// Updates RectangleRenderer colors based on UIInteractive state and UIButton colors.
    /// Publishes UIButtonClickedEvent when buttons are clicked.
    ///
    /// This system should be updated after UIInputSystem so that interaction states
    /// are already updated before applying visual changes.
    /// </summary>
    public class UIButtonSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        /// <summary>
        /// Creates a new UIButtonSystem.
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="eventBus">Event bus for publishing button events</param>
        public UIButtonSystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;
        }

        public void Update(float deltaTime)
        {
            // Query all entities with button components
            var query = new QueryDescription()
                .WithAll<UIButton, UIInteractive, RectangleRenderer>();

            _world.Query(in query, (Entity entity, ref UIButton button, ref UIInteractive interactive, ref RectangleRenderer renderer) =>
            {
                // Update button color based on interaction state
                renderer.Color = GetButtonColor(ref button, ref interactive);

                // Publish button clicked event
                if (interactive.WasClicked)
                {
                    _eventBus.Publish(new UIButtonClickedEvent
                    {
                        Entity = entity,
                        ButtonId = button.Id
                    });
                }
            });
        }

        /// <summary>
        /// Determines the appropriate color for a button based on its interaction state.
        /// </summary>
        private static System.Drawing.Color GetButtonColor(ref UIButton button, ref UIInteractive interactive)
        {
            // Disabled state takes priority
            if (!interactive.IsEnabled)
                return button.DisabledColor;

            // Pressed state
            if (interactive.IsPressed)
                return button.PressedColor;

            // Hovered state
            if (interactive.IsHovered)
                return button.HoverColor;

            // Normal state
            return button.NormalColor;
        }
    }
}
