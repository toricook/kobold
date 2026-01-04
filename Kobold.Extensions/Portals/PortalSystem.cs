using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Events;
using Kobold.Extensions.Triggers.Events;
using System;
using System.Collections.Generic;

namespace Kobold.Extensions.Portals
{
    /// <summary>
    /// Handles portal teleportation by subscribing to trigger events.
    /// Works in conjunction with TriggerSystem - when a trigger on a portal entity
    /// is activated, this system teleports the activating entity to the portal's destination.
    /// </summary>
    public class PortalSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        public PortalSystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;

            // Subscribe to trigger events to detect when entities enter portals
            _eventBus.Subscribe<TriggerEvent>(OnTriggerActivated);
        }

        public void Update(float deltaTime)
        {
            // Update cooldown timers for all portals
            var query = new QueryDescription().WithAll<PortalComponent>();

            _world.Query(in query, (ref PortalComponent portal) =>
            {
                if (portal.CooldownTimers.Count == 0)
                    return;

                var toRemove = new List<Entity>();

                // Decrement timers
                foreach (var kvp in portal.CooldownTimers)
                {
                    var newTime = kvp.Value - deltaTime;
                    if (newTime <= 0)
                    {
                        toRemove.Add(kvp.Key);
                    }
                    else
                    {
                        portal.CooldownTimers[kvp.Key] = newTime;
                    }
                }

                // Remove expired cooldowns
                foreach (var entity in toRemove)
                {
                    portal.CooldownTimers.Remove(entity);
                }
            });
        }

        private void OnTriggerActivated(TriggerEvent evt)
        {
            // Check if the trigger entity has a portal component
            if (!_world.Has<PortalComponent>(evt.TriggerEntity))
                return;

            ref var portal = ref _world.Get<PortalComponent>(evt.TriggerEntity);

            // Check if portal is active
            if (!portal.IsActive)
                return;

            // Check if this entity is on cooldown
            if (portal.CooldownTimers.ContainsKey(evt.ActivatorEntity))
                return;

            // Teleport the entity
            TeleportEntity(evt.TriggerEntity, evt.ActivatorEntity, ref portal);
        }

        private void TeleportEntity(Entity portalEntity, Entity targetEntity, ref PortalComponent portal)
        {
            // Handle level generation destination specially
            if (portal.Destination is LevelGenerationDestination levelDest)
            {
                // Publish event for game code to handle level generation
                var evt = new LevelGenerationRequestEvent(
                    portalEntity,
                    targetEntity,
                    levelDest.LevelId,
                    levelDest.GenerationParams);
                _eventBus.Publish(evt);

                // Don't apply cooldown for level generation - let game code handle it
                // (The level might be regenerated, destroying and recreating the portal)
                return;
            }

            // Apply the destination effect
            portal.Destination.Apply(_world, targetEntity);

            // Publish teleport event for visual/audio effects
            var teleportEvent = new PortalTeleportEvent(
                portalEntity,
                targetEntity,
                portal.PortalTag);
            _eventBus.Publish(teleportEvent);

            // Start cooldown to prevent immediate re-teleport
            if (portal.TeleportCooldown > 0)
            {
                portal.CooldownTimers[targetEntity] = portal.TeleportCooldown;
            }
        }
    }
}
