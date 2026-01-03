using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components;
using Kobold.Core.Components.Gameplay;
using Kobold.Core.Events;
using System;
using System.Collections.Generic;

namespace Kobold.Core.Systems
{
    /// <summary>
    /// Processes trigger zones and publishes events when conditions are met.
    /// Integrates with CollisionSystem via EventBus subscriptions.
    /// Requires entities to have: TriggerComponent, Trigger tag, BoxCollider, Transform
    /// </summary>
    public class TriggerSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly IInputManager? _inputManager;
        private readonly KeyCode _interactKey;

        public TriggerSystem(
            World world,
            EventBus eventBus,
            IInputManager? inputManager = null,
            KeyCode interactKey = KeyCode.E)
        {
            _world = world;
            _eventBus = eventBus;
            _inputManager = inputManager;
            _interactKey = interactKey;

            // Subscribe to collision events to detect when entities enter triggers
            _eventBus.Subscribe<CollisionEvent>(OnCollision);
        }

        public void Update(float deltaTime)
        {
            // Update cooldowns and check for button-based triggers
            var query = new QueryDescription()
                .WithAll<TriggerComponent, BoxCollider, Transform, Trigger>();

            _world.Query(in query, (Entity triggerEntity, ref TriggerComponent trigger, ref BoxCollider collider, ref Transform transform) =>
            {
                if (!trigger.IsActive)
                    return;

                // Update cooldown timer
                if (trigger.CooldownTimer > 0)
                {
                    trigger.CooldownTimer -= deltaTime;
                }

                // Handle OnStay triggers
                if (trigger.Mode.HasFlag(TriggerMode.OnStay) &&
                    trigger.EntitiesInside.Count > 0 &&
                    trigger.CooldownTimer <= 0)
                {
                    bool buttonRequired = trigger.Mode.HasFlag(TriggerMode.RequiresButton);
                    bool buttonPressed = !buttonRequired ||
                                       (_inputManager?.IsKeyPressed(_interactKey) ?? false);

                    System.Console.WriteLine($"[TriggerSystem OnStay] Tag: {trigger.TriggerTag}, ButtonRequired: {buttonRequired}, ButtonPressed: {buttonPressed}, Entities: {trigger.EntitiesInside.Count}");

                    if (buttonPressed)
                    {
                        // Use Interact event type when button is required, Stay otherwise
                        var eventType = buttonRequired ? TriggerEventType.Interact : TriggerEventType.Stay;
                        System.Console.WriteLine($"[TriggerSystem OnStay] Publishing {eventType} event for: {trigger.TriggerTag}");
                        foreach (var entity in trigger.EntitiesInside)
                        {
                            if (_world.IsAlive(entity))
                            {
                                PublishTriggerEvent(triggerEntity, entity, eventType, ref trigger);
                            }
                        }
                    }
                }

                // Handle RequiresButton for entities currently inside (OnEnter with button delay)
                if (trigger.Mode.HasFlag(TriggerMode.RequiresButton) &&
                    !trigger.Mode.HasFlag(TriggerMode.OnStay) &&
                    trigger.EntitiesInside.Count > 0 &&
                    trigger.CooldownTimer <= 0)
                {
                    bool keyPressed = _inputManager?.IsKeyPressed(_interactKey) ?? false;
                    System.Console.WriteLine($"[TriggerSystem] E key pressed: {keyPressed}, Entities in trigger: {trigger.EntitiesInside.Count}, Tag: {trigger.TriggerTag}");

                    if (keyPressed)
                    {
                        System.Console.WriteLine($"[TriggerSystem] Publishing Interact event for trigger: {trigger.TriggerTag}");
                        foreach (var entity in trigger.EntitiesInside)
                        {
                            if (_world.IsAlive(entity))
                            {
                                PublishTriggerEvent(triggerEntity, entity, TriggerEventType.Interact, ref trigger);
                            }
                        }
                    }
                }
            });

            // Detect entities that have exited trigger zones
            DetectExits();
        }

        private void OnCollision(CollisionEvent collision)
        {
            // Check if either entity is a trigger
            ProcessPotentialTrigger(collision.Entity1, collision.Entity2);
            ProcessPotentialTrigger(collision.Entity2, collision.Entity1);
        }

        private void ProcessPotentialTrigger(Entity potentialTrigger, Entity otherEntity)
        {
            // Must have TriggerComponent and Trigger tag
            if (!_world.Has<TriggerComponent>(potentialTrigger) ||
                !_world.Has<Trigger>(potentialTrigger))
                return;

            ref var trigger = ref _world.Get<TriggerComponent>(potentialTrigger);

            if (!trigger.IsActive)
                return;

            // Check if other entity's layer can activate this trigger
            if (!CanActivate(otherEntity, trigger.ActivationLayers))
                return;

            // Track entry/exit
            bool wasInside = trigger.EntitiesInside.Contains(otherEntity);

            if (!wasInside)
            {
                // Entity entered trigger
                trigger.EntitiesInside.Add(otherEntity);
                System.Console.WriteLine($"[TriggerSystem] Entity entered trigger zone! Tag: {trigger.TriggerTag}, Mode: {trigger.Mode}");

                if (trigger.Mode.HasFlag(TriggerMode.OnEnter) &&
                    trigger.CooldownTimer <= 0)
                {
                    bool buttonRequired = trigger.Mode.HasFlag(TriggerMode.RequiresButton);
                    bool buttonPressed = !buttonRequired ||
                                       (_inputManager?.IsKeyPressed(_interactKey) ?? false);

                    if (buttonPressed)
                    {
                        PublishTriggerEvent(potentialTrigger, otherEntity, TriggerEventType.Enter, ref trigger);
                    }
                }
            }
        }

        /// <summary>
        /// Detects entities that are no longer colliding with trigger zones.
        /// Since CollisionSystem only publishes events for active collisions,
        /// we need to manually check each frame for entities that have left.
        /// </summary>
        private void DetectExits()
        {
            var query = new QueryDescription()
                .WithAll<TriggerComponent, BoxCollider, Transform, Trigger>();

            _world.Query(in query, (Entity triggerEntity, ref TriggerComponent trigger, ref BoxCollider collider, ref Transform transform) =>
            {
                if (!trigger.IsActive)
                    return;

                var toRemove = new List<Entity>();

                foreach (var entity in trigger.EntitiesInside)
                {
                    // Check if entity still exists and has required components
                    if (!_world.IsAlive(entity) ||
                        !_world.Has<Transform>(entity) ||
                        !_world.Has<BoxCollider>(entity))
                    {
                        toRemove.Add(entity);
                        continue;
                    }

                    // Check if still colliding
                    var entityTransform = _world.Get<Transform>(entity);
                    var entityCollider = _world.Get<BoxCollider>(entity);

                    if (!collider.Overlaps(transform.Position, entityCollider, entityTransform.Position))
                    {
                        toRemove.Add(entity);

                        if (trigger.Mode.HasFlag(TriggerMode.OnExit) &&
                            trigger.CooldownTimer <= 0)
                        {
                            PublishTriggerEvent(triggerEntity, entity, TriggerEventType.Exit, ref trigger);
                        }
                    }
                }

                // Remove entities that exited
                foreach (var entity in toRemove)
                {
                    trigger.EntitiesInside.Remove(entity);
                }
            });
        }

        private bool CanActivate(Entity entity, CollisionLayer mask)
        {
            // Get the entity's collision layer
            var entityLayer = GetCollisionLayer(entity);

            // Check if the entity's layer matches the activation mask
            return (mask & entityLayer) != 0;
        }

        private CollisionLayer GetCollisionLayer(Entity entity)
        {
            if (_world.Has<CollisionLayerComponent>(entity))
            {
                return _world.Get<CollisionLayerComponent>(entity).Layer;
            }

            // Auto-detect layer based on tag components
            if (_world.Has<Player>(entity))
                return CollisionLayer.Player;
            if (_world.Has<Enemy>(entity))
                return CollisionLayer.Enemy;
            if (_world.Has<Projectile>(entity))
                return CollisionLayer.Projectile;
            if (_world.Has<Static>(entity))
                return CollisionLayer.Environment;

            return CollisionLayer.Default;
        }

        private void PublishTriggerEvent(Entity trigger, Entity activator, TriggerEventType eventType, ref TriggerComponent triggerComp)
        {
            var evt = new TriggerEvent(trigger, activator, eventType, triggerComp.TriggerTag);
            _eventBus.Publish(evt);

            // Start cooldown
            if (triggerComp.Cooldown > 0)
            {
                triggerComp.CooldownTimer = triggerComp.Cooldown;
            }
        }
    }
}
