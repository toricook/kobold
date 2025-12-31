using Arch.Core;
using Kobold.Core.Systems;
using System;
using System.Collections.Generic;

namespace Kobold.Core.Components.Gameplay
{
    /// <summary>
    /// Defines a trigger zone that detects entity entry/exit/stay events.
    /// Non-solid collision zone that publishes events when conditions are met.
    /// Requires the Trigger tag component and a BoxCollider to define the trigger area.
    /// </summary>
    public struct TriggerComponent
    {
        /// <summary>
        /// Trigger activation mode
        /// </summary>
        public TriggerMode Mode { get; set; }

        /// <summary>
        /// Which collision layer(s) can activate this trigger.
        /// Uses the CollisionLayer enum from CollisionSystem.
        /// </summary>
        public CollisionLayer ActivationLayers { get; set; }

        /// <summary>
        /// Whether this trigger is currently active (can be toggled on/off)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Cooldown between activations in seconds (0 = no cooldown)
        /// </summary>
        public float Cooldown { get; set; }

        /// <summary>
        /// Time remaining until trigger can activate again
        /// </summary>
        public float CooldownTimer { get; set; }

        /// <summary>
        /// Optional tag to identify trigger type (e.g., "door", "checkpoint", "zone")
        /// </summary>
        public string? TriggerTag { get; set; }

        /// <summary>
        /// Entities currently inside this trigger zone (tracked automatically by TriggerSystem)
        /// </summary>
        public HashSet<Entity> EntitiesInside { get; set; }

        /// <summary>
        /// Creates a new TriggerComponent with the specified parameters
        /// </summary>
        public TriggerComponent(
            TriggerMode mode = TriggerMode.OnEnter,
            CollisionLayer activationLayers = CollisionLayer.Player,
            string? triggerTag = null,
            float cooldown = 0f)
        {
            Mode = mode;
            ActivationLayers = activationLayers;
            IsActive = true;
            Cooldown = cooldown;
            CooldownTimer = 0f;
            TriggerTag = triggerTag;
            EntitiesInside = new HashSet<Entity>();
        }
    }

    /// <summary>
    /// Trigger activation modes.
    /// Can be combined using bitwise OR (e.g., OnEnter | OnExit)
    /// </summary>
    [Flags]
    public enum TriggerMode
    {
        /// <summary>Trigger when entity first enters the zone</summary>
        OnEnter = 1 << 0,

        /// <summary>Trigger continuously while entity is inside the zone</summary>
        OnStay = 1 << 1,

        /// <summary>Trigger when entity exits the zone</summary>
        OnExit = 1 << 2,

        /// <summary>Requires button press while inside (must be combined with OnEnter or OnStay)</summary>
        RequiresButton = 1 << 3,

        /// <summary>Trigger on entry, but only if button is pressed</summary>
        OnEnterWithButton = OnEnter | RequiresButton,

        /// <summary>Trigger while inside and button is held</summary>
        OnStayWithButton = OnStay | RequiresButton
    }
}
