using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Kobold.Extensions.Items.Components;
using Kobold.Extensions.Items.Data;
using Kobold.Extensions.Items.Registry;
using Kobold.Extensions.Items.Spawning;

namespace Kobold.Extensions.Items.Systems
{
    /// <summary>
    /// System that spawns items when entities with LootTableComponent are destroyed.
    /// Subscribes to EntityDestroyedEvent and rolls loot tables to spawn appropriate items.
    /// </summary>
    public class LootDropSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly LootTableRegistry _lootTableRegistry;
        private readonly ItemSpawner _itemSpawner;
        private readonly ItemFactory _itemFactory;
        private readonly Random _random;

        public LootDropSystem(
            World world,
            EventBus eventBus,
            LootTableRegistry lootTableRegistry,
            ItemSpawner itemSpawner,
            ItemFactory itemFactory,
            Random random = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _lootTableRegistry = lootTableRegistry ?? throw new ArgumentNullException(nameof(lootTableRegistry));
            _itemSpawner = itemSpawner ?? throw new ArgumentNullException(nameof(itemSpawner));
            _itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory));
            _random = random ?? Random.Shared;

            // Subscribe to entity destruction events
            _eventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
        }

        public void Update(float deltaTime)
        {
            // Event-driven, no per-frame update logic needed
        }

        private void OnEntityDestroyed(EntityDestroyedEvent evt)
        {
            // Check if entity had loot table component
            if (!_world.Has<LootTableComponent>(evt.Entity))
                return;

            var lootTableComp = _world.Get<LootTableComponent>(evt.Entity);

            // Roll loot table and spawn items
            SpawnLoot(lootTableComp.LootTableId, evt.Position, lootTableComp.DropPositionOffsetRadius);
        }

        private void SpawnLoot(string lootTableId, Vector2 position, float offsetRadius)
        {
            if (!_lootTableRegistry.TryGetLootTable(lootTableId, out var table))
            {
                Console.WriteLine($"Warning: Loot table not found: {lootTableId}");
                return;
            }

            var droppedItems = RollLootTable(table);

            // Spawn each dropped item
            foreach (var (itemDef, quantity) in droppedItems)
            {
                for (int i = 0; i < quantity; i++)
                {
                    // Random offset for visual spread
                    var offset = GetRandomOffset(offsetRadius);
                    var spawnPos = position + offset;

                    _itemFactory.CreatePickupEntity(itemDef, spawnPos);
                }
            }

            if (droppedItems.Count > 0)
            {
                Console.WriteLine($"Dropped {droppedItems.Sum(d => d.quantity)} items from loot table: {lootTableId}");
            }
        }

        private List<(ItemDefinition item, int quantity)> RollLootTable(LootTableDefinition table)
        {
            return table.RollMode switch
            {
                LootRollMode.Independent => RollIndependent(table),
                LootRollMode.Sequential => RollSequential(table),
                LootRollMode.OneOf => RollOneOf(table),
                _ => new List<(ItemDefinition, int)>()
            };
        }

        /// <summary>
        /// Independent roll mode: Each entry rolls independently based on DropChance
        /// </summary>
        private List<(ItemDefinition, int)> RollIndependent(LootTableDefinition table)
        {
            var results = new List<(ItemDefinition, int)>();

            foreach (var entry in table.Entries)
            {
                // Roll for drop chance
                if (_random.NextDouble() <= entry.DropChance)
                {
                    var item = GetItemFromEntry(entry);
                    if (item != null)
                    {
                        int quantity = _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);
                        results.Add((item, quantity));
                    }
                }

                // Stop if max drops reached
                if (results.Count >= table.MaxDrops)
                    break;
            }

            return results;
        }

        /// <summary>
        /// Sequential roll mode: Roll entries in order until GuaranteedDropCount met or MaxDrops reached
        /// </summary>
        private List<(ItemDefinition, int)> RollSequential(LootTableDefinition table)
        {
            var results = new List<(ItemDefinition, int)>();
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

        /// <summary>
        /// OneOf roll mode: Pick exactly one entry randomly from the list
        /// </summary>
        private List<(ItemDefinition, int)> RollOneOf(LootTableDefinition table)
        {
            if (table.Entries.Count == 0)
                return new List<(ItemDefinition, int)>();

            // Pick random entry
            var entry = table.Entries[_random.Next(table.Entries.Count)];
            var item = GetItemFromEntry(entry);

            if (item == null)
                return new List<(ItemDefinition, int)>();

            int quantity = _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);
            return new List<(ItemDefinition, int)> { (item, quantity) };
        }

        /// <summary>
        /// Get item definition from loot entry (handles both specific items and rarity-based)
        /// </summary>
        private ItemDefinition GetItemFromEntry(LootEntry entry)
        {
            try
            {
                if (entry.RarityBased && entry.Rarity.HasValue)
                {
                    // Spawn random item from rarity tier
                    return _itemSpawner.SpawnRandomItemByRarity(entry.Rarity.Value);
                }
                else if (!string.IsNullOrEmpty(entry.ItemId))
                {
                    // Spawn specific item by ID
                    return _itemSpawner.GetItemById(entry.ItemId);
                }

                Console.WriteLine("Warning: Loot entry has neither RarityBased nor ItemId set");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting item from loot entry: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get random offset within radius for visual scatter
        /// </summary>
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
