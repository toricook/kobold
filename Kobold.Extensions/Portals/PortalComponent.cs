using Arch.Core;
using System;
using System.Collections.Generic;

namespace Kobold.Extensions.Portals
{
    /// <summary>
    /// Portal component that teleports entities using the trigger system.
    /// Must be paired with a TriggerComponent to detect when entities enter the portal.
    /// Entities with this component respond to trigger events by teleporting the activating entity.
    /// </summary>
    public struct PortalComponent
    {
        /// <summary>
        /// Where this portal leads (coordinate, level generation, or another entity)
        /// </summary>
        public PortalDestination Destination { get; set; }

        /// <summary>
        /// Whether this portal is currently active and can teleport entities
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Cooldown duration (in seconds) after teleporting an entity.
        /// Prevents the same entity from being immediately teleported back (bounce-back prevention).
        /// </summary>
        public float TeleportCooldown { get; set; }

        /// <summary>
        /// Tracks cooldown timers for individual entities that have recently been teleported.
        /// Key: Entity that was teleported, Value: Remaining cooldown time in seconds
        /// </summary>
        public Dictionary<Entity, float> CooldownTimers { get; set; }

        /// <summary>
        /// Optional identifier for visual/audio effects to play on teleport.
        /// Game code can subscribe to PortalTeleportEvent and use this to trigger effects.
        /// </summary>
        public string? TeleportEffect { get; set; }

        /// <summary>
        /// Optional tag to categorize this portal (e.g., "exit", "entrance", "secret_area")
        /// </summary>
        public string? PortalTag { get; set; }

        /// <summary>
        /// Creates a new PortalComponent with the specified destination
        /// </summary>
        public PortalComponent(
            PortalDestination destination,
            float teleportCooldown = 1f,
            string? portalTag = null,
            string? teleportEffect = null)
        {
            Destination = destination;
            IsActive = true;
            TeleportCooldown = teleportCooldown;
            CooldownTimers = new Dictionary<Entity, float>();
            TeleportEffect = teleportEffect;
            PortalTag = portalTag;
        }
    }
}
