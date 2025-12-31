using System;
using System.Collections.Generic;
using CaveExplorer.Components.PickupEffects;
using Kobold.Extensions.Items.Effects;
using Kobold.Extensions.Items.Registry;
using Kobold.Extensions.Items.Spawning;
using Kobold.Extensions.Pickups;

namespace CaveExplorer.Items
{
    /// <summary>
    /// Factory for creating CaveExplorer-specific pickup effects from JSON definitions.
    /// Registers all effect types used in items.json.
    /// </summary>
    public class CaveExplorerEffectFactory : PickupEffectFactory
    {
        private readonly LootTableRegistry _lootTableRegistry;
        private readonly ItemSpawner _itemSpawner;
        private readonly ItemFactory _itemFactory;
        private readonly Random _random;

        /// <summary>
        /// Creates a new CaveExplorerEffectFactory with all game-specific effects registered
        /// </summary>
        /// <param name="lootTableRegistry">Registry for loot tables (needed for chest effects)</param>
        /// <param name="itemSpawner">Item spawner (needed for chest effects)</param>
        /// <param name="itemFactory">Item factory (needed for chest effects)</param>
        /// <param name="random">Random number generator (optional)</param>
        public CaveExplorerEffectFactory(
            LootTableRegistry lootTableRegistry = null,
            ItemSpawner itemSpawner = null,
            ItemFactory itemFactory = null,
            Random random = null)
        {
            _lootTableRegistry = lootTableRegistry;
            _itemSpawner = itemSpawner;
            _itemFactory = itemFactory;
            _random = random ?? Random.Shared;

            RegisterAllEffects();
        }

        /// <summary>
        /// Register all CaveExplorer pickup effect types
        /// </summary>
        private void RegisterAllEffects()
        {
            // Coin collection effect
            RegisterEffectType("AddCoins", parameters =>
            {
                int amount = GetParam(parameters, "amount", 1);
                return new CoinPickupEffect(amount);
            });

            // Healing effect for health potions
            RegisterEffectType("HealPlayer", parameters =>
            {
                int healAmount = GetParam(parameters, "healAmount", 25);
                return new HealPlayerEffect(healAmount);
            });

            // Add item to inventory (keys, etc.)
            RegisterEffectType("AddItem", parameters =>
            {
                string itemType = GetParam(parameters, "itemType", "unknown");
                return new AddItemEffect(itemType);
            });

            // Open treasure chest with loot
            RegisterEffectType("OpenChest", parameters =>
            {
                string lootTable = GetParam(parameters, "lootTable", "default_chest_loot");

                // If dependencies are available, create full chest effect
                if (_lootTableRegistry != null && _itemSpawner != null && _itemFactory != null)
                {
                    return new OpenChestEffect(lootTable, _lootTableRegistry, _itemSpawner, _itemFactory, _random);
                }

                // Otherwise, create a simple effect that just logs (for testing)
                Console.WriteLine("Warning: OpenChest effect created without loot dependencies");
                return new AddItemEffect($"chest_{lootTable}"); // Fallback
            });
        }
    }
}
