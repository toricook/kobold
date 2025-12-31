using Arch.Core;
using Kobold.Core.Events;

namespace Kobold.Extensions.Pickups
{
    /// <summary>
    /// Type of change that occurred in the inventory
    /// </summary>
    public enum InventoryChangeType
    {
        /// <summary>An item was added to the inventory</summary>
        ItemAdded,

        /// <summary>An item was removed from the inventory</summary>
        ItemRemoved,

        /// <summary>An item was used from the inventory</summary>
        ItemUsed
    }

    /// <summary>
    /// Published when the contents of an inventory change.
    /// UI systems can subscribe to this event for reactive inventory display updates.
    /// </summary>
    public class InventoryChangedEvent : BaseEvent
    {
        /// <summary>The entity whose inventory changed (typically the player)</summary>
        public Entity InventoryEntity { get; }

        /// <summary>The type of change that occurred</summary>
        public InventoryChangeType ChangeType { get; }

        /// <summary>The tag of the item that changed</summary>
        public string ItemTag { get; }

        /// <summary>The new quantity/count for this item type</summary>
        public int NewCount { get; }

        public InventoryChangedEvent(
            Entity inventoryEntity,
            InventoryChangeType changeType,
            string itemTag,
            int newCount)
        {
            InventoryEntity = inventoryEntity;
            ChangeType = changeType;
            ItemTag = itemTag;
            NewCount = newCount;
        }
    }
}
