using Arch.Core;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Kobold.Extensions.Portals
{
    /// <summary>
    /// Base class for portal destinations.
    /// Defines where a portal teleports entities.
    /// </summary>
    public abstract class PortalDestination
    {
        /// <summary>
        /// Applies the destination effect to the specified entity.
        /// </summary>
        /// <param name="world">The game world</param>
        /// <param name="entity">The entity to teleport</param>
        public abstract void Apply(World world, Entity entity);
    }

    /// <summary>
    /// Teleports an entity to specific world coordinates.
    /// Optionally sets the entity's velocity after teleportation.
    /// </summary>
    public class CoordinateDestination : PortalDestination
    {
        /// <summary>Target position in world coordinates</summary>
        public Vector2 TargetPosition { get; set; }

        /// <summary>Optional velocity to set after teleporting (null = keep current velocity)</summary>
        public Vector2? TargetVelocity { get; set; }

        public CoordinateDestination(Vector2 targetPosition, Vector2? targetVelocity = null)
        {
            TargetPosition = targetPosition;
            TargetVelocity = targetVelocity;
        }

        public override void Apply(World world, Entity entity)
        {
            // Update position
            if (world.Has<Transform>(entity))
            {
                ref var transform = ref world.Get<Transform>(entity);
                transform.Position = TargetPosition;
            }

            // Optionally update velocity
            if (TargetVelocity.HasValue && world.Has<Velocity>(entity))
            {
                ref var velocity = ref world.Get<Velocity>(entity);
                velocity.Value = TargetVelocity.Value;
            }
        }
    }

    /// <summary>
    /// Triggers level generation or transition.
    /// Does not directly modify the entity - instead publishes a LevelGenerationRequestEvent
    /// that game code can subscribe to for custom level generation logic.
    /// </summary>
    public class LevelGenerationDestination : PortalDestination
    {
        /// <summary>Identifier for the level to generate (e.g., "next_level", "boss_room")</summary>
        public string LevelId { get; set; }

        /// <summary>Optional parameters for level generation (e.g., difficulty, size, theme)</summary>
        public Dictionary<string, object>? GenerationParams { get; set; }

        public LevelGenerationDestination(string levelId, Dictionary<string, object>? generationParams = null)
        {
            LevelId = levelId;
            GenerationParams = generationParams;
        }

        public override void Apply(World world, Entity entity)
        {
            // This destination doesn't directly modify the entity
            // The PortalSystem will detect this type and publish a LevelGenerationRequestEvent instead
            // Game code subscribes to that event to handle level generation
        }
    }

    /// <summary>
    /// Teleports an entity to the position of another portal entity.
    /// Useful for creating linked portal pairs (e.g., Portal A -> Portal B).
    /// </summary>
    public class EntityDestination : PortalDestination
    {
        /// <summary>The target portal entity to teleport to</summary>
        public Entity TargetPortal { get; set; }

        /// <summary>Offset from the target portal's position (default: zero)</summary>
        public Vector2 Offset { get; set; }

        public EntityDestination(Entity targetPortal, Vector2 offset = default)
        {
            TargetPortal = targetPortal;
            Offset = offset;
        }

        public override void Apply(World world, Entity entity)
        {
            // Check if target portal still exists
            if (!world.IsAlive(TargetPortal) || !world.Has<Transform>(TargetPortal))
                return;

            var targetTransform = world.Get<Transform>(TargetPortal);

            // Teleport to target portal's position plus offset
            if (world.Has<Transform>(entity))
            {
                ref var transform = ref world.Get<Transform>(entity);
                transform.Position = targetTransform.Position + Offset;
            }
        }
    }
}
