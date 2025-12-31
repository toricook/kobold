using System.Linq;

namespace Kobold.Extensions.Pickups
{
    /// <summary>
    /// Extension methods and helper utilities for querying and manipulating inventory data.
    /// Provides a convenient API for UI systems and gameplay logic.
    /// </summary>
    public static class InventoryHelpers
    {
        /// <summary>
        /// Gets the count/quantity of a specific item type in the inventory.
        /// For stackable items, returns the stack count.
        /// For non-stackable items, returns the number of individual instances.
        /// </summary>
        /// <param name="inventory">The inventory to query</param>
        /// <param name="itemTag">The item tag to search for</param>
        /// <returns>The count of the specified item type</returns>
        public static int GetItemCount(this InventoryComponent inventory, string itemTag)
        {
            // Check stackable items first
            if (inventory.StackableItems.ContainsKey(itemTag))
                return inventory.StackableItems[itemTag];

            // Count non-stackable items with matching tag
            return inventory.NonStackableItems.Count(s => s.PickupTag == itemTag);
        }

        /// <summary>
        /// Checks if the inventory contains at least one of the specified item type.
        /// </summary>
        /// <param name="inventory">The inventory to query</param>
        /// <param name="itemTag">The item tag to search for</param>
        /// <returns>True if the item exists in the inventory, false otherwise</returns>
        public static bool HasItem(this InventoryComponent inventory, string itemTag)
        {
            return inventory.StackableItems.ContainsKey(itemTag) ||
                   inventory.NonStackableItems.Any(s => s.PickupTag == itemTag);
        }

        /// <summary>
        /// Gets the total number of occupied inventory slots.
        /// Each stackable item type counts as 1 slot regardless of quantity.
        /// Each non-stackable item counts as 1 slot.
        /// </summary>
        /// <param name="inventory">The inventory to query</param>
        /// <returns>The number of occupied slots</returns>
        public static int GetTotalItemCount(this InventoryComponent inventory)
        {
            return inventory.CurrentItemCount;
        }

        /// <summary>
        /// Checks if the inventory is at maximum capacity.
        /// </summary>
        /// <param name="inventory">The inventory to query</param>
        /// <returns>True if the inventory is full, false otherwise (or if unlimited capacity)</returns>
        public static bool IsFull(this InventoryComponent inventory)
        {
            return inventory.MaxCapacity > 0 &&
                   inventory.CurrentItemCount >= inventory.MaxCapacity;
        }

        /// <summary>
        /// Gets the number of available (empty) inventory slots.
        /// </summary>
        /// <param name="inventory">The inventory to query</param>
        /// <returns>The number of available slots, or int.MaxValue if unlimited capacity</returns>
        public static int GetAvailableSlots(this InventoryComponent inventory)
        {
            if (inventory.MaxCapacity <= 0)
                return int.MaxValue; // Unlimited

            return inventory.MaxCapacity - inventory.CurrentItemCount;
        }

        /// <summary>
        /// Gets all unique item tags in the inventory (both stackable and non-stackable).
        /// </summary>
        /// <param name="inventory">The inventory to query</param>
        /// <returns>A collection of unique item tags</returns>
        public static string[] GetAllItemTags(this InventoryComponent inventory)
        {
            var stackableTags = inventory.StackableItems.Keys;
            var nonStackableTags = inventory.NonStackableItems.Select(s => s.PickupTag).Distinct();

            return stackableTags.Concat(nonStackableTags).Distinct().ToArray();
        }

        /// <summary>
        /// Gets the fill percentage of the inventory (0.0 to 1.0).
        /// Returns 0.0 for unlimited inventories.
        /// </summary>
        /// <param name="inventory">The inventory to query</param>
        /// <returns>Fill percentage as a float from 0.0 (empty) to 1.0 (full)</returns>
        public static float GetFillPercentage(this InventoryComponent inventory)
        {
            if (inventory.MaxCapacity <= 0)
                return 0.0f; // Unlimited capacity

            return (float)inventory.CurrentItemCount / inventory.MaxCapacity;
        }
    }
}
