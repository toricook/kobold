using Arch.Core;
using Kobold.Core.Events;
using System.Numerics;

namespace Kobold.Extensions.UI.Events
{
    /// <summary>
    /// Event published when a UI element is clicked (mouse pressed and released while hovering).
    /// This is the most common event to subscribe to for button actions.
    /// </summary>
    public class UIClickEvent : IEvent
    {
        /// <summary>
        /// The entity that was clicked.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// The mouse position when the click occurred (in world coordinates).
        /// </summary>
        public Vector2 MousePosition { get; set; }
    }

    /// <summary>
    /// Event published when the mouse begins hovering over a UI element.
    /// Useful for showing tooltips or playing hover sounds.
    /// </summary>
    public class UIHoverEnterEvent : IEvent
    {
        /// <summary>
        /// The entity that started being hovered.
        /// </summary>
        public Entity Entity { get; set; }
    }

    /// <summary>
    /// Event published when the mouse stops hovering over a UI element.
    /// Useful for hiding tooltips or reverting visual states.
    /// </summary>
    public class UIHoverExitEvent : IEvent
    {
        /// <summary>
        /// The entity that is no longer being hovered.
        /// </summary>
        public Entity Entity { get; set; }
    }

    /// <summary>
    /// Event published when a button is clicked.
    /// This is published in addition to UIClickEvent and includes the button's ID.
    /// </summary>
    public class UIButtonClickedEvent : IEvent
    {
        /// <summary>
        /// The button entity that was clicked.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// The ID of the button that was clicked (from UIButton.Id).
        /// </summary>
        public string ButtonId { get; set; } = string.Empty;
    }
}
