using System.Collections.Generic;
using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Extensions.Combat.Components;

namespace Kobold.Extensions.Combat.Systems
{
    /// <summary>
    /// System that updates attack cooldown timers and removes the component when ready.
    /// Also updates invulnerability timers on entities with HealthComponent.
    /// </summary>
    public class AttackCooldownSystem : ISystem
    {
        private readonly World _world;

        public AttackCooldownSystem(World world)
        {
            _world = world;
        }

        public void Update(float deltaTime)
        {
            // Update attack cooldowns
            var cooldownQuery = new QueryDescription().WithAll<AttackCooldownComponent>();

            List<Entity> cooldownsToRemove = new List<Entity>();

            _world.Query(in cooldownQuery, (Entity entity, ref AttackCooldownComponent cooldown) =>
            {
                cooldown.RemainingCooldown -= deltaTime;

                // Remove cooldown component when timer expires
                if (cooldown.RemainingCooldown <= 0f)
                {
                    cooldownsToRemove.Add(entity);
                }
            });

            // Remove expired cooldown components
            foreach (var entity in cooldownsToRemove)
            {
                if (_world.IsAlive(entity))
                {
                    _world.Remove<AttackCooldownComponent>(entity);
                }
            }

            // Update invulnerability timers
            var healthQuery = new QueryDescription().WithAll<HealthComponent>();

            _world.Query(in healthQuery, (Entity entity, ref HealthComponent health) =>
            {
                if (health.InvulnerabilityTimer > 0f)
                {
                    health.InvulnerabilityTimer -= deltaTime;
                }
            });
        }
    }
}
