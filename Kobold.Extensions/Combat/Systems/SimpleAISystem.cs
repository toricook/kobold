using System;
using System.Numerics;
using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.Combat.Components;
using Kobold.Extensions.Combat.Events;

namespace Kobold.Extensions.Combat.Systems
{
    /// <summary>
    /// System that handles simple AI behavior for enemies.
    /// Makes enemies chase and attack the player when in range.
    /// </summary>
    public class SimpleAISystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        public SimpleAISystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;
        }

        public void Update(float deltaTime)
        {
            // Find the player position
            Vector2? playerPosition = GetPlayerPosition();
            if (!playerPosition.HasValue)
                return; // No player found

            // Query all AI entities
            var aiQuery = new QueryDescription()
                .WithAll<Transform, SimpleAIComponent, Velocity>()
                .WithNone<AttackCooldownComponent>(); // Only process if not on cooldown

            _world.Query(in aiQuery, (Entity entity, ref Transform transform, ref SimpleAIComponent ai, ref Velocity velocity) =>
            {
                float distanceToPlayer = Vector2.Distance(transform.Position, playerPosition.Value);

                // Update chase state based on detection range
                ai.IsChasing = distanceToPlayer <= ai.DetectionRange;
                ai.IsInAttackRange = distanceToPlayer <= ai.AttackRange;

                if (ai.IsInAttackRange)
                {
                    // Stop moving and attack
                    velocity.Value = Vector2.Zero;

                    // Publish attack event
                    _eventBus.Publish(new AttackRequestedEvent(
                        entity,
                        transform.Position,
                        ai.AttackRange,
                        ai.AttackDamage,
                        "Player", // Target players
                        false // Don't damage self
                    ));

                    // Add cooldown
                    _world.Add(entity, new AttackCooldownComponent(ai.AttackCooldown));
                }
                else if (ai.IsChasing)
                {
                    // Move towards player
                    Vector2 direction = Vector2.Normalize(playerPosition.Value - transform.Position);
                    velocity.Value = direction * ai.MoveSpeed;
                }
                else
                {
                    // Not chasing, stop moving
                    velocity.Value = Vector2.Zero;
                }
            });

            // Also handle AI entities that are on cooldown (update their movement but not attacks)
            var aiCooldownQuery = new QueryDescription()
                .WithAll<Transform, SimpleAIComponent, Velocity, AttackCooldownComponent>();

            _world.Query(in aiCooldownQuery, (Entity entity, ref Transform transform, ref SimpleAIComponent ai, ref Velocity velocity) =>
            {
                float distanceToPlayer = Vector2.Distance(transform.Position, playerPosition.Value);

                ai.IsChasing = distanceToPlayer <= ai.DetectionRange;
                ai.IsInAttackRange = distanceToPlayer <= ai.AttackRange;

                if (ai.IsInAttackRange)
                {
                    // Stop moving when in attack range (waiting for cooldown)
                    velocity.Value = Vector2.Zero;
                }
                else if (ai.IsChasing)
                {
                    // Continue chasing even while on cooldown
                    Vector2 direction = Vector2.Normalize(playerPosition.Value - transform.Position);
                    velocity.Value = direction * ai.MoveSpeed;
                }
                else
                {
                    // Not chasing
                    velocity.Value = Vector2.Zero;
                }
            });
        }

        private Vector2? GetPlayerPosition()
        {
            var playerQuery = new QueryDescription().WithAll<Transform, Player>();
            Vector2? position = null;

            _world.Query(in playerQuery, (ref Transform transform) =>
            {
                position = transform.Position;
            });

            return position;
        }
    }
}
