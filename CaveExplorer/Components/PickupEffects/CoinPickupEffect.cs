using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Extensions.Pickups;
using System;

namespace CaveExplorer.Components.PickupEffects
{
    /// <summary>
    /// Pickup effect that adds coins to the player's inventory.
    /// Implements the IPickupEffect interface from Kobold.Extensions.Pickups.
    /// </summary>
    public class CoinPickupEffect : IPickupEffect
    {
        /// <summary>
        /// The number of coins this pickup is worth
        /// </summary>
        public int CoinValue { get; set; }

        public CoinPickupEffect(int coinValue = 1)
        {
            CoinValue = coinValue;
        }

        /// <summary>
        /// Applies the coin pickup effect by adding coins to the collector's inventory
        /// </summary>
        public void Apply(World world, Entity pickupEntity, Entity collectorEntity)
        {
            // Check if collector has PlayerInventory component
            if (!world.Has<PlayerInventory>(collectorEntity))
            {
                Console.WriteLine("Warning: Collector doesn't have PlayerInventory component!");
                return;
            }

            // Add coins to the player's inventory
            ref var inventory = ref world.Get<PlayerInventory>(collectorEntity);
            inventory.Coins += CoinValue;

            Console.WriteLine($"Coin collected! +{CoinValue} | Total coins: {inventory.Coins}");
        }

        /// <summary>
        /// Returns a description of this pickup for UI/logging
        /// </summary>
        public string GetDescription()
        {
            return $"Coin ({CoinValue})";
        }
    }
}
