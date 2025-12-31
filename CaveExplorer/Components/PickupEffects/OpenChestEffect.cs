using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Components;
using Kobold.Extensions.Items.Data;
using Kobold.Extensions.Items.Registry;
using Kobold.Extensions.Items.Spawning;
using Kobold.Extensions.Pickups;
using System;
using System.Numerics;

namespace CaveExplorer.Components.PickupEffects
{
    /// <summary>
    /// Pickup effect that opens a treasure chest and spawns loot from a loot table.
    /// Implements the IPickupEffect interface from Kobold.Extensions.Pickups.
    /// </summary>
    public class OpenChestEffect : IPickupEffect
    {
        /// <summary>
        /// The loot table ID to use when spawning items
        /// </summary>
        public string LootTableId { get; set; }

        private readonly LootTableRegistry _lootTableRegistry;
        private readonly ItemSpawner _itemSpawner;
        private readonly ItemFactory _itemFactory;
        private readonly Random _random;

        public OpenChestEffect(
            string lootTableId,
            LootTableRegistry lootTableRegistry,
            ItemSpawner itemSpawner,
            ItemFactory itemFactory,
            Random random = null)
        {
            LootTableId = lootTableId;
            _lootTableRegistry = lootTableRegistry ?? throw new ArgumentNullException(nameof(lootTableRegistry));
            _itemSpawner = itemSpawner ?? throw new ArgumentNullException(nameof(itemSpawner));
            _itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory));
            _random = random ?? Random.Shared;
        }

        /// <summary>
        /// Applies the effect by opening the chest and spawning loot
        /// </summary>
        public void Apply(World world, Entity pickupEntity, Entity collectorEntity)
        {
            // Get chest position
            Vector2 chestPosition = Vector2.Zero;
            if (world.Has<Transform>(pickupEntity))
            {
                chestPosition = world.Get<Transform>(pickupEntity).Position;
            }

            Console.WriteLine($"Opening treasure chest at {chestPosition}...");

            // Get loot table
            if (!_lootTableRegistry.TryGetLootTable(LootTableId, out var lootTable))
            {
                Console.WriteLine($"Warning: Loot table not found: {LootTableId}");
                return;
            }

            // Roll loot and spawn items
            var lootItems = RollLootTable(lootTable);
            SpawnLoot(lootItems, chestPosition, offsetRadius: 32f);

            Console.WriteLine($"Chest opened! Spawned {lootItems.Count} item types");
        }

        /// <summary>
        /// Returns a description of this pickup for UI/logging
        /// </summary>
        public string GetDescription()
        {
            return $"Treasure Chest ({LootTableId})";
        }

        /// <summary>
        /// Chests are non-stackable
        /// </summary>
        public bool IsStackable => false;

        private System.Collections.Generic.List<(ItemDefinition item, int quantity)> RollLootTable(LootTableDefinition table)
        {
            // Use the same rolling logic as LootDropSystem
            return table.RollMode switch
            {
                LootRollMode.Independent => RollIndependent(table),
                LootRollMode.Sequential => RollSequential(table),
                LootRollMode.OneOf => RollOneOf(table),
                _ => new System.Collections.Generic.List<(ItemDefinition, int)>()
            };
        }

        private System.Collections.Generic.List<(ItemDefinition, int)> RollIndependent(LootTableDefinition table)
        {
            var results = new System.Collections.Generic.List<(ItemDefinition, int)>();

            foreach (var entry in table.Entries)
            {
                if (_random.NextDouble() <= entry.DropChance)
                {
                    var item = GetItemFromEntry(entry);
                    if (item != null)
                    {
                        int quantity = _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);
                        results.Add((item, quantity));
                    }
                }

                if (results.Count >= table.MaxDrops)
                    break;
            }

            return results;
        }

        private System.Collections.Generic.List<(ItemDefinition, int)> RollSequential(LootTableDefinition table)
        {
            var results = new System.Collections.Generic.List<(ItemDefinition, int)>();
            int guaranteedCount = 0;

            foreach (var entry in table.Entries)
            {
                bool shouldDrop = guaranteedCount < table.GuaranteedDropCount ||
                                 _random.NextDouble() <= entry.DropChance;

                if (shouldDrop)
                {
                    var item = GetItemFromEntry(entry);
                    if (item != null)
                    {
                        int quantity = _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);
                        results.Add((item, quantity));
                        guaranteedCount++;
                    }
                }

                if (results.Count >= table.MaxDrops)
                    break;
            }

            return results;
        }

        private System.Collections.Generic.List<(ItemDefinition, int)> RollOneOf(LootTableDefinition table)
        {
            if (table.Entries.Count == 0)
                return new System.Collections.Generic.List<(ItemDefinition, int)>();

            var entry = table.Entries[_random.Next(table.Entries.Count)];
            var item = GetItemFromEntry(entry);

            if (item == null)
                return new System.Collections.Generic.List<(ItemDefinition, int)>();

            int quantity = _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);
            return new System.Collections.Generic.List<(ItemDefinition, int)> { (item, quantity) };
        }

        private ItemDefinition GetItemFromEntry(LootEntry entry)
        {
            try
            {
                if (entry.RarityBased && entry.Rarity.HasValue)
                {
                    return _itemSpawner.SpawnRandomItemByRarity(entry.Rarity.Value);
                }
                else if (!string.IsNullOrEmpty(entry.ItemId))
                {
                    return _itemSpawner.GetItemById(entry.ItemId);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting item from loot entry: {ex.Message}");
                return null;
            }
        }

        private void SpawnLoot(System.Collections.Generic.List<(ItemDefinition item, int quantity)> lootItems, Vector2 position, float offsetRadius)
        {
            foreach (var (itemDef, quantity) in lootItems)
            {
                for (int i = 0; i < quantity; i++)
                {
                    var offset = GetRandomOffset(offsetRadius);
                    var spawnPos = position + offset;
                    _itemFactory.CreatePickupEntity(itemDef, spawnPos);
                }
            }
        }

        private Vector2 GetRandomOffset(float radius)
        {
            if (radius <= 0)
                return Vector2.Zero;

            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float distance = (float)(_random.NextDouble() * radius);

            return new Vector2(
                MathF.Cos(angle) * distance,
                MathF.Sin(angle) * distance
            );
        }
    }
}
