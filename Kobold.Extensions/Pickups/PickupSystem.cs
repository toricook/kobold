using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using System;

namespace Kobold.Extensions.Pickups
{
    /// <summary>
    /// Handles pickup collection through both collision (automatic) and trigger (interactive) systems.
    ///
    /// Automatic Pickups (collision-based):
    /// - Entity must have PickupComponent with RequiresInteraction = false
    /// - Entity must have BoxCollider on CollisionLayer.Pickup
    /// - Collected automatically on collision with player
    ///
    /// Interactive Pickups (trigger-based):
    /// - Entity must have PickupComponent with RequiresInteraction = true
    /// - Entity must have TriggerComponent and Trigger tag
    /// - Requires button press (E key) to collect
    ///
    /// When collected, the PickupComponent.Effect is applied and ItemCollectedEvent is published.
    /// </summary>
    public class PickupSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        public PickupSystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;

            // Subscribe to collision events for automatic pickups
            _eventBus.Subscribe<CollisionEvent>(OnCollision);

            // Subscribe to trigger events for interactive pickups
            _eventBus.Subscribe<TriggerEvent>(OnTrigger);
        }

        public void Update(float deltaTime)
        {
            // All logic is event-driven
        }

        /// <summary>
        /// Handle automatic pickup collection via collision
        /// </summary>
        private void OnCollision(CollisionEvent evt)
        {
            // Try both directions - pickup could be Entity1 or Entity2
            TryCollectPickup(evt.Entity1, evt.Entity2, requiresInteraction: false);
            TryCollectPickup(evt.Entity2, evt.Entity1, requiresInteraction: false);
        }

        /// <summary>
        /// Handle interactive pickup collection via trigger interaction
        /// </summary>
        private void OnTrigger(TriggerEvent evt)
        {
            // Only handle Interact events (button press)
            if (evt.EventType != TriggerEventType.Interact)
                return;

            // The trigger entity is the pickup, activator is the collector
            TryCollectPickup(evt.TriggerEntity, evt.ActivatorEntity, requiresInteraction: true);
        }

        /// <summary>
        /// Attempt to collect a pickup if all conditions are met
        /// </summary>
        private void TryCollectPickup(Entity potentialPickup, Entity potentialCollector, bool requiresInteraction)
        {
            // Check if the potential pickup has a PickupComponent
            if (!_world.Has<PickupComponent>(potentialPickup))
                return;

            ref var pickup = ref _world.Get<PickupComponent>(potentialPickup);

            // Check if interaction requirement matches
            if (pickup.RequiresInteraction != requiresInteraction)
                return;

            // Check if collector has Player tag (or could be made more flexible)
            if (!_world.Has<Player>(potentialCollector))
                return;

            try
            {
                // Apply the pickup effect
                pickup.Effect.Apply(_world, potentialPickup, potentialCollector);

                // Publish collection event for visual/audio feedback
                _eventBus.Publish(new ItemCollectedEvent(
                    potentialPickup,
                    potentialCollector,
                    pickup.Effect,
                    pickup.PickupTag
                ));

                // Mark pickup for destruction (use Set to handle existing component)
                if (_world.Has<PendingDestruction>(potentialPickup))
                {
                    _world.Set(potentialPickup, new PendingDestruction(DestructionReason.Manual));
                }
                else
                {
                    _world.Add(potentialPickup, new PendingDestruction(DestructionReason.Manual));
                }

                Console.WriteLine($"Pickup collected and marked for destruction: {pickup.PickupTag}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying pickup effect: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
