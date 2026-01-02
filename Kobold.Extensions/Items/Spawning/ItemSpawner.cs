using System;
using System.Collections.Generic;
using Kobold.Extensions.Items.Data;
using Kobold.Extensions.Items.Registry;

namespace Kobold.Extensions.Items.Spawning
{
    /// <summary>
    /// Handles weighted random item selection by rarity and specific IDs.
    /// Uses WeightedRandomSelector for probabilistic spawning.
    /// </summary>
    public class ItemSpawner
    {
        private readonly ItemRegistry _itemRegistry;
        private readonly Random _random;

        /// <summary>
        /// Creates a new ItemSpawner
        /// </summary>
        /// <param name="itemRegistry">Item registry to pull items from</param>
        /// <param name="random">Random number generator (uses Random.Shared if null)</param>
        public ItemSpawner(ItemRegistry itemRegistry, Random random = null)
        {
            _itemRegistry = itemRegistry ?? throw new ArgumentNullException(nameof(itemRegistry));
            _random = random ?? Random.Shared;
        }

        /// <summary>
        /// Spawn a random item of specific rarity using weighted selection
        /// </summary>
        /// <param name="rarity">Rarity tier to select from</param>
        /// <returns>Randomly selected item definition</returns>
        /// <exception cref="InvalidOperationException">If no items exist for the rarity</exception>
        public ItemDefinition SpawnRandomItemByRarity(ItemRarity rarity)
        {
            var items = _itemRegistry.GetItemsByRarity(rarity);
            if (items.Count == 0)
                throw new InvalidOperationException($"No items registered for rarity: {rarity}");

            return WeightedRandomSelector.SelectWeighted(
                items,
                item => item.SpawnWeight,
                _random
            );
        }

        /// <summary>
        /// Spawn a random item from any rarity (weighted by rarity tier weights)
        /// </summary>
        /// <param name="rarityWeights">Custom rarity weights (uses defaults if null)</param>
        /// <returns>Randomly selected item definition</returns>
        public ItemDefinition SpawnRandomItem(Dictionary<ItemRarity, int> rarityWeights = null)
        {
            rarityWeights ??= GetDefaultRarityWeights();

            // Filter out rarities that have no items
            var availableRarities = new Dictionary<ItemRarity, int>();
            foreach (var kvp in rarityWeights)
            {
                var items = _itemRegistry.GetItemsByRarity(kvp.Key);
                if (items.Count > 0)
                {
                    availableRarities[kvp.Key] = kvp.Value;
                }
            }

            // If no items available at all, throw exception
            if (availableRarities.Count == 0)
                throw new InvalidOperationException("No items registered in the item registry");

            // First, select a rarity tier based on rarity weights (only from available rarities)
            var rarity = WeightedRandomSelector.SelectWeighted(
                availableRarities.Keys,
                r => availableRarities[r],
                _random
            );

            // Then, select an item from that rarity tier
            return SpawnRandomItemByRarity(rarity);
        }

        /// <summary>
        /// Get specific item by ID (for manual spawning)
        /// </summary>
        /// <param name="itemId">Item ID to retrieve</param>
        /// <returns>Item definition</returns>
        public ItemDefinition GetItemById(string itemId)
        {
            return _itemRegistry.GetItem(itemId);
        }

        /// <summary>
        /// Try to get specific item by ID
        /// </summary>
        /// <param name="itemId">Item ID to retrieve</param>
        /// <param name="item">Output item definition if found</param>
        /// <returns>True if item was found</returns>
        public bool TryGetItemById(string itemId, out ItemDefinition item)
        {
            return _itemRegistry.TryGetItem(itemId, out item);
        }

        /// <summary>
        /// Default rarity weights used when no custom weights provided.
        /// Common items are most frequent, Legendary items are rarest.
        /// </summary>
        private static Dictionary<ItemRarity, int> GetDefaultRarityWeights()
        {
            return new Dictionary<ItemRarity, int>
            {
                { ItemRarity.Common, 100 },
                { ItemRarity.Uncommon, 50 },
                { ItemRarity.Rare, 20 },
                { ItemRarity.Epic, 5 },
                { ItemRarity.Legendary, 1 }
            };
        }
    }
}
