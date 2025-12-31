using Arch.Core;
using Kobold.Core.Events;
using System;
using System.Collections.Generic;

namespace Kobold.Extensions.Portals
{
    /// <summary>
    /// Published when an entity is teleported by a portal.
    /// Game code can subscribe to this event to trigger visual/audio effects,
    /// update UI, or perform other actions when teleportation occurs.
    /// </summary>
    public class PortalTeleportEvent : BaseEvent
    {
        /// <summary>The portal entity that performed the teleport</summary>
        public Entity PortalEntity { get; }

        /// <summary>The entity that was teleported</summary>
        public Entity TeleportedEntity { get; }

        /// <summary>Optional tag from PortalComponent.PortalTag (e.g., "exit", "secret")</summary>
        public string? PortalTag { get; }

        public PortalTeleportEvent(Entity portalEntity, Entity teleportedEntity, string? portalTag)
        {
            PortalEntity = portalEntity;
            TeleportedEntity = teleportedEntity;
            PortalTag = portalTag;
        }
    }

    /// <summary>
    /// Published when a portal requests level generation or transition.
    /// This event is published instead of directly teleporting the entity,
    /// allowing game code to control the level generation process.
    ///
    /// Example use cases:
    /// - Generate a new procedural level
    /// - Load a pre-designed level
    /// - Transition to a different game area
    /// - Progress to the next dungeon floor
    /// </summary>
    public class LevelGenerationRequestEvent : BaseEvent
    {
        /// <summary>The portal entity that triggered the request</summary>
        public Entity PortalEntity { get; }

        /// <summary>The entity requesting the level change (usually the player)</summary>
        public Entity RequestingEntity { get; }

        /// <summary>Identifier for the level to generate/load (e.g., "next_level", "boss_room")</summary>
        public string LevelId { get; }

        /// <summary>
        /// Optional parameters for level generation.
        /// Examples: difficulty, size, theme, seed, etc.
        /// Game code interprets these based on its generation logic.
        /// </summary>
        public Dictionary<string, object>? GenerationParams { get; }

        public LevelGenerationRequestEvent(
            Entity portalEntity,
            Entity requestingEntity,
            string levelId,
            Dictionary<string, object>? generationParams)
        {
            PortalEntity = portalEntity;
            RequestingEntity = requestingEntity;
            LevelId = levelId;
            GenerationParams = generationParams;
        }
    }
}
