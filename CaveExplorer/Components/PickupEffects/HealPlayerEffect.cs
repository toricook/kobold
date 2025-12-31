using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Extensions.Pickups;
using System;

namespace CaveExplorer.Components.PickupEffects
{
    /// <summary>
    /// Pickup effect that heals the player by a specified amount.
    /// Implements the IPickupEffect interface from Kobold.Extensions.Pickups.
    /// </summary>
    public class HealPlayerEffect : IPickupEffect
    {
        /// <summary>
        /// The amount of health points to restore
        /// </summary>
        public int HealAmount { get; set; }

        public HealPlayerEffect(int healAmount = 25)
        {
            HealAmount = healAmount;
        }

        /// <summary>
        /// Applies the healing effect to the collector entity
        /// </summary>
        public void Apply(World world, Entity pickupEntity, Entity collectorEntity)
        {
            // Check if collector has Health component
            if (!world.Has<Health>(collectorEntity))
            {
                Console.WriteLine("Warning: Collector doesn't have Health component!");
                return;
            }

            // Heal the entity
            ref var health = ref world.Get<Health>(collectorEntity);
            int actualHealed = health.Heal(HealAmount);

            Console.WriteLine($"Health potion used! +{actualHealed} HP | Current: {health.CurrentHealth}/{health.MaxHealth}");
        }

        /// <summary>
        /// Returns a description of this pickup for UI/logging
        /// </summary>
        public string GetDescription()
        {
            return $"Health Potion (+{HealAmount} HP)";
        }

        /// <summary>
        /// Health potions are stackable items
        /// </summary>
        public bool IsStackable => true;
    }
}
