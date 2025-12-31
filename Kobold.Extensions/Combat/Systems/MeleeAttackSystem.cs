using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Kobold.Extensions.Combat.Components;
using Kobold.Extensions.Combat.Events;

namespace Kobold.Extensions.Combat.Systems
{
    /// <summary>
    /// System that processes melee attacks by finding targets in radius and applying damage.
    /// Subscribes to AttackRequestedEvent and handles damage calculation/application.
    /// </summary>
    public class MeleeAttackSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        public MeleeAttackSystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;

            // Subscribe to attack requests
            _eventBus.Subscribe<AttackRequestedEvent>(OnAttackRequested);
        }

        public void Update(float deltaTime)
        {
            // Event-driven, no per-frame logic
        }

        private void OnAttackRequested(AttackRequestedEvent evt)
        {
            // Find all entities with health in the attack radius
            var damageableQuery = new QueryDescription()
                .WithAll<Transform, HealthComponent>();

            List<Entity> hitEntities = new List<Entity>();

            _world.Query(in damageableQuery, (Entity target, ref Transform targetTransform, ref HealthComponent health) =>
            {
                // Skip if target is the attacker and weapon can't damage self
                if (target == evt.AttackerEntity && !evt.CanDamageSelf)
                    return;

                // Check if target is within attack radius
                float distance = Vector2.Distance(evt.AttackPosition, targetTransform.Position);
                if (distance <= evt.AttackRadius)
                {
                    // Check target tag filtering (if specified)
                    if (!string.IsNullOrEmpty(evt.TargetTag))
                    {
                        if (evt.TargetTag == "Enemy" && !_world.Has<Enemy>(target))
                            return;
                        // Add more tag checks as needed
                    }

                    // Apply damage
                    int damageDealt = health.TakeDamage(evt.Damage);

                    if (damageDealt > 0)
                    {
                        hitEntities.Add(target);

                        // Publish damage event
                        _eventBus.Publish(new DamageDealtEvent(
                            evt.AttackerEntity,
                            target,
                            damageDealt,
                            targetTransform.Position
                        ));

                        Console.WriteLine($"Dealt {damageDealt} damage to entity at {targetTransform.Position}");

                        // Check if entity died
                        if (!health.IsAlive)
                        {
                            // Mark for destruction
                            DestructionSystem.MarkForDestruction(_world, target, DestructionReason.Manual);

                            _eventBus.Publish(new EntityKilledEvent(
                                evt.AttackerEntity,
                                target,
                                targetTransform.Position
                            ));

                            Console.WriteLine($"Entity killed at {targetTransform.Position}");
                        }
                    }
                }
            });

            // Publish attack performed event (for effects/audio)
            _eventBus.Publish(new AttackPerformedEvent(
                evt.AttackerEntity,
                evt.AttackPosition,
                evt.AttackRadius,
                hitEntities.Count
            ));

            // Add cooldown component to attacker
            if (_world.IsAlive(evt.AttackerEntity) && _world.Has<MeleeWeaponComponent>(evt.AttackerEntity))
            {
                ref var weapon = ref _world.Get<MeleeWeaponComponent>(evt.AttackerEntity);

                if (_world.Has<AttackCooldownComponent>(evt.AttackerEntity))
                {
                    ref var cooldown = ref _world.Get<AttackCooldownComponent>(evt.AttackerEntity);
                    cooldown.RemainingCooldown = weapon.CooldownDuration;
                }
                else
                {
                    _world.Add(evt.AttackerEntity, new AttackCooldownComponent(weapon.CooldownDuration));
                }
            }
        }
    }
}
