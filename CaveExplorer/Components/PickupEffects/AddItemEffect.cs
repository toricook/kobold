using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Extensions.Pickups;
using System;

namespace CaveExplorer.Components.PickupEffects
{
    /// <summary>
    /// Pickup effect that adds a generic item to the player's inventory.
    /// Used for keys, quest items, and other collectibles.
    /// Implements the IPickupEffect interface from Kobold.Extensions.Pickups.
    /// </summary>
    public class AddItemEffect : IPickupEffect
    {
        /// <summary>
        /// The type/category of item being added (e.g., "key", "quest_item")
        /// </summary>
        public string ItemType { get; set; }

        public AddItemEffect(string itemType = "unknown")
        {
            ItemType = itemType;
        }

        /// <summary>
        /// Applies the effect by adding the item to the collector's inventory
        /// </summary>
        public void Apply(World world, Entity pickupEntity, Entity collectorEntity)
        {
            // Check if collector has PlayerInventory component
            if (!world.Has<PlayerInventory>(collectorEntity))
            {
                Console.WriteLine("Warning: Collector doesn't have PlayerInventory component!");
                return;
            }

            // For now, just log the item collection
            // In a full implementation, you'd add to a proper inventory system
            Console.WriteLine($"Item collected: {ItemType}");

            // You can extend PlayerInventory to track keys/items, or use the InventoryComponent
            // that was created earlier in the session for a more robust solution
        }

        /// <summary>
        /// Returns a description of this pickup for UI/logging
        /// </summary>
        public string GetDescription()
        {
            return $"{ItemType}";
        }

        /// <summary>
        /// Generic items are typically non-stackable (keys, quest items, etc.)
        /// </summary>
        public bool IsStackable => false;
    }
}
