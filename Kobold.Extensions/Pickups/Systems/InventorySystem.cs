using System;
using System.Linq;
using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Events;

namespace Kobold.Extensions.Pickups
{
    /// <summary>
    /// Manages inventory state for entities with InventoryComponent.
    /// Subscribes to ItemCollectedEvent and updates inventory accordingly.
    /// Handles both stackable items (counted by tag) and non-stackable items (tracked individually).
    /// Enforces capacity limits and publishes InventoryChangedEvent for UI updates.
    /// </summary>
    public class InventorySystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        public InventorySystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;

            // Subscribe to item collection events
            _eventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
        }

        public void Update(float deltaTime)
        {
            // Event-driven system - no per-frame updates needed
        }

        /// <summary>
        /// Handles item collection by adding items to the collector's inventory.
        /// </summary>
        private void OnItemCollected(ItemCollectedEvent evt)
        {
            // Verify collector has InventoryComponent
            if (!_world.Has<InventoryComponent>(evt.CollectorEntity))
                return;

            ref var inventory = ref _world.Get<InventoryComponent>(evt.CollectorEntity);

            // Use effect type name as fallback if PickupTag is null/empty
            string itemTag = !string.IsNullOrWhiteSpace(evt.PickupTag)
                ? evt.PickupTag
                : evt.Effect.GetType().Name;

            // Attempt to add item to inventory
            bool success = TryAddItem(
                ref inventory,
                itemTag,
                evt.Effect
            );

            if (success)
            {
                // Publish inventory changed event for UI updates
                _eventBus.Publish(new InventoryChangedEvent(
                    evt.CollectorEntity,
                    InventoryChangeType.ItemAdded,
                    itemTag,
                    GetCurrentCount(inventory, itemTag)
                ));

                Console.WriteLine($"Added '{itemTag}' to inventory. Total: {GetCurrentCount(inventory, itemTag)}");
            }
            else
            {
                // Inventory full - item was collected but not stored
                Console.WriteLine($"Inventory full! Cannot store '{itemTag}'. ({inventory.CurrentItemCount}/{inventory.MaxCapacity} slots occupied)");
            }
        }

        /// <summary>
        /// Attempts to add an item to the inventory.
        /// Returns true if successful, false if inventory is full.
        /// </summary>
        private bool TryAddItem(
            ref InventoryComponent inventory,
            string pickupTag,
            IPickupEffect effect)
        {
            // Check capacity constraints
            if (inventory.MaxCapacity > 0)
            {
                if (effect.IsStackable)
                {
                    // Stackable items only need a new slot if this is a new stack
                    if (!inventory.StackableItems.ContainsKey(pickupTag))
                    {
                        if (inventory.CurrentItemCount >= inventory.MaxCapacity)
                            return false; // No room for new stack
                    }
                    // Existing stacks don't need capacity check
                }
                else
                {
                    // Non-stackable items always need a new slot
                    if (inventory.CurrentItemCount >= inventory.MaxCapacity)
                        return false; // Inventory full
                }
            }

            // Add the item
            if (effect.IsStackable)
            {
                if (inventory.StackableItems.ContainsKey(pickupTag))
                {
                    // Increment existing stack
                    inventory.StackableItems[pickupTag]++;
                }
                else
                {
                    // Create new stack
                    inventory.StackableItems[pickupTag] = 1;
                    inventory.IncrementItemCount(); // New stack = new slot
                }
            }
            else
            {
                // Add as individual item
                inventory.NonStackableItems.Add(new InventorySlot(
                    pickupTag,
                    effect,
                    effect.GetDescription()
                ));
                inventory.IncrementItemCount();
            }

            return true;
        }

        /// <summary>
        /// Gets the current count/quantity of a specific item type in the inventory.
        /// </summary>
        private int GetCurrentCount(InventoryComponent inventory, string pickupTag)
        {
            // Check stackable items first
            if (inventory.StackableItems.ContainsKey(pickupTag))
                return inventory.StackableItems[pickupTag];

            // Count non-stackable items with matching tag
            return inventory.NonStackableItems.Count(s => s.PickupTag == pickupTag);
        }
    }
}
