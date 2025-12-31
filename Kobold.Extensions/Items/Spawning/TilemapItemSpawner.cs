using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Kobold.Extensions.Tilemaps;

namespace Kobold.Extensions.Items.Spawning
{
    /// <summary>
    /// Spawns items scattered across tilemap floor tiles.
    /// Provides methods for both probability-based and count-based spawning.
    /// </summary>
    public class TilemapItemSpawner
    {
        private readonly ItemSpawner _itemSpawner;
        private readonly ItemFactory _itemFactory;
        private readonly Random _random;

        public TilemapItemSpawner(
            ItemSpawner itemSpawner,
            ItemFactory itemFactory,
            Random random = null)
        {
            _itemSpawner = itemSpawner ?? throw new ArgumentNullException(nameof(itemSpawner));
            _itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory));
            _random = random ?? Random.Shared;
        }

        /// <summary>
        /// Spawn items randomly on floor tiles with a probability per tile
        /// </summary>
        /// <param name="tileMap">Tilemap to spawn items on</param>
        /// <param name="floorTileId">Tile ID that represents walkable floor</param>
        /// <param name="spawnChance">Probability (0.0 to 1.0) that an item spawns on each floor tile</param>
        /// <param name="rarityFilter">If specified, only spawn items of this rarity</param>
        /// <param name="rarityWeights">Custom rarity weights (if rarityFilter is null)</param>
        /// <returns>Number of items spawned</returns>
        public int SpawnItemsOnTilemap(
            TileMap tileMap,
            int floorTileId,
            float spawnChance,
            ItemRarity? rarityFilter = null,
            Dictionary<ItemRarity, int> rarityWeights = null)
        {
            if (tileMap == null)
                throw new ArgumentNullException(nameof(tileMap));

            if (spawnChance < 0f || spawnChance > 1f)
                throw new ArgumentOutOfRangeException(nameof(spawnChance), "Spawn chance must be between 0 and 1");

            int spawnedCount = 0;

            for (int x = 0; x < tileMap.Width; x++)
            {
                for (int y = 0; y < tileMap.Height; y++)
                {
                    // Check if this is a floor tile
                    if (tileMap.GetTile(0, x, y) != floorTileId)
                        continue;

                    // Roll for spawn chance
                    if (_random.NextDouble() >= spawnChance)
                        continue;

                    // Spawn item at tile center
                    var (worldX, worldY) = tileMap.TileToWorldCenter(x, y);
                    var position = new Vector2(worldX, worldY);

                    var itemDef = rarityFilter.HasValue
                        ? _itemSpawner.SpawnRandomItemByRarity(rarityFilter.Value)
                        : _itemSpawner.SpawnRandomItem(rarityWeights);

                    _itemFactory.CreatePickupEntity(itemDef, position);
                    spawnedCount++;
                }
            }

            Console.WriteLine($"Spawned {spawnedCount} items on tilemap using spawn chance {spawnChance:P0}");
            return spawnedCount;
        }

        /// <summary>
        /// Spawn a specific number of items at random valid positions on the tilemap
        /// </summary>
        /// <param name="tileMap">Tilemap to spawn items on</param>
        /// <param name="floorTileId">Tile ID that represents walkable floor</param>
        /// <param name="itemCount">Exact number of items to spawn</param>
        /// <param name="rarityFilter">If specified, only spawn items of this rarity</param>
        /// <param name="rarityWeights">Custom rarity weights (if rarityFilter is null)</param>
        /// <returns>Number of items actually spawned (may be less than itemCount if not enough floor tiles)</returns>
        public int SpawnItemsAtRandomPositions(
            TileMap tileMap,
            int floorTileId,
            int itemCount,
            ItemRarity? rarityFilter = null,
            Dictionary<ItemRarity, int> rarityWeights = null)
        {
            if (tileMap == null)
                throw new ArgumentNullException(nameof(tileMap));

            if (itemCount < 0)
                throw new ArgumentOutOfRangeException(nameof(itemCount), "Item count must be non-negative");

            // Get all valid floor positions
            var validPositions = GetValidTilePositions(tileMap, floorTileId);

            if (validPositions.Count == 0)
            {
                Console.WriteLine("Warning: No valid floor tiles found for item spawning");
                return 0;
            }

            // Limit spawn count to available positions
            int spawnCount = Math.Min(itemCount, validPositions.Count);

            // Shuffle and select positions
            var selectedPositions = validPositions
                .OrderBy(_ => _random.Next())
                .Take(spawnCount)
                .ToList();

            // Spawn items at selected positions
            foreach (var (x, y) in selectedPositions)
            {
                var (worldX, worldY) = tileMap.TileToWorldCenter(x, y);
                var position = new Vector2(worldX, worldY);

                var itemDef = rarityFilter.HasValue
                    ? _itemSpawner.SpawnRandomItemByRarity(rarityFilter.Value)
                    : _itemSpawner.SpawnRandomItem(rarityWeights);

                _itemFactory.CreatePickupEntity(itemDef, position);
            }

            Console.WriteLine($"Spawned {spawnCount} items at random positions on tilemap");
            return spawnCount;
        }

        /// <summary>
        /// Get list of all valid floor tile positions
        /// </summary>
        private List<(int x, int y)> GetValidTilePositions(TileMap tileMap, int floorTileId)
        {
            var positions = new List<(int, int)>();

            for (int x = 0; x < tileMap.Width; x++)
            {
                for (int y = 0; y < tileMap.Height; y++)
                {
                    if (tileMap.GetTile(0, x, y) == floorTileId)
                    {
                        positions.Add((x, y));
                    }
                }
            }

            return positions;
        }
    }
}
