using System.Collections.Generic;

namespace Kobold.Extensions.Pickups
{
    /// <summary>
    /// Component that stores inventory items on an entity (typically the player).
    /// Supports both stackable items (counted by tag) and non-stackable items (tracked individually).
    /// </summary>
    public struct InventoryComponent
    {
        /// <summary>
        /// Stackable items tracked by PickupTag with their quantities.
        /// Key: PickupTag (e.g., "coin", "health_potion")
        /// Value: Quantity count
        /// </summary>
        public Dictionary<string, int> StackableItems { get; set; }

        /// <summary>
        /// Non-stackable items tracked individually with their full metadata.
        /// </summary>
        public List<InventorySlot> NonStackableItems { get; set; }

        /// <summary>
        /// Maximum number of inventory slots (stacks count as 1 slot each).
        /// Set to 0 for unlimited capacity.
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Current number of occupied inventory slots.
        /// Stackable items count as 1 slot per unique tag.
        /// Non-stackable items count as 1 slot each.
        /// </summary>
        public int CurrentItemCount { get; private set; }

        /// <summary>
        /// Creates a new InventoryComponent with the specified maximum capacity.
        /// </summary>
        /// <param name="maxCapacity">Maximum inventory slots (0 = unlimited)</param>
        public InventoryComponent(int maxCapacity = 0)
        {
            StackableItems = new Dictionary<string, int>();
            NonStackableItems = new List<InventorySlot>();
            MaxCapacity = maxCapacity;
            CurrentItemCount = 0;
        }

        /// <summary>
        /// Internal method to increment the item count when a new slot is occupied.
        /// </summary>
        internal void IncrementItemCount()
        {
            CurrentItemCount++;
        }

        /// <summary>
        /// Internal method to decrement the item count when a slot is freed.
        /// </summary>
        internal void DecrementItemCount()
        {
            if (CurrentItemCount > 0)
                CurrentItemCount--;
        }
    }

    /// <summary>
    /// Represents a single non-stackable item in the inventory.
    /// Stores the full metadata for the item including its effect.
    /// </summary>
    public struct InventorySlot
    {
        /// <summary>
        /// The pickup tag identifier for this item.
        /// </summary>
        public string PickupTag { get; set; }

        /// <summary>
        /// The pickup effect associated with this item.
        /// Can be used for future "use item from inventory" functionality.
        /// </summary>
        public IPickupEffect Effect { get; set; }

        /// <summary>
        /// Human-readable description of the item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates a new InventorySlot with the specified properties.
        /// </summary>
        public InventorySlot(string pickupTag, IPickupEffect effect, string description)
        {
            PickupTag = pickupTag;
            Effect = effect;
            Description = description;
        }
    }
}
