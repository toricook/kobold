using Arch.Core;
using Kobold.Core.Events;

namespace Kobold.Extensions.Triggers.Events
{
    /// <summary>
    /// Published when a trigger is activated by an entity.
    /// Subscribe to this event to respond to trigger interactions.
    /// </summary>
    public class TriggerEvent : BaseEvent
    {
        /// <summary>The trigger entity that was activated</summary>
        public Entity TriggerEntity { get; }

        /// <summary>The entity that activated the trigger</summary>
        public Entity ActivatorEntity { get; }

        /// <summary>Type of trigger event (Enter, Stay, Exit, Interact)</summary>
        public TriggerEventType EventType { get; }

        /// <summary>Optional tag identifying the trigger type (from TriggerComponent.TriggerTag)</summary>
        public string? TriggerTag { get; }

        public TriggerEvent(
            Entity triggerEntity,
            Entity activatorEntity,
            TriggerEventType eventType,
            string? triggerTag = null)
        {
            TriggerEntity = triggerEntity;
            ActivatorEntity = activatorEntity;
            EventType = eventType;
            TriggerTag = triggerTag;
        }
    }

    /// <summary>
    /// Type of trigger event
    /// </summary>
    public enum TriggerEventType
    {
        /// <summary>Entity entered the trigger zone</summary>
        Enter,

        /// <summary>Entity is staying inside the trigger zone</summary>
        Stay,

        /// <summary>Entity exited the trigger zone</summary>
        Exit,

        /// <summary>Button was pressed while entity is inside the trigger zone</summary>
        Interact
    }
}
