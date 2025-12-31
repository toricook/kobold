using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Kobold.Extensions.Items.Data;

namespace Kobold.Extensions.Items.Registry
{
    /// <summary>
    /// Central registry for all item definitions.
    /// Loads from JSON and provides query API for item lookups.
    /// </summary>
    public class ItemRegistry
    {
        private readonly Dictionary<string, ItemDefinition> _itemsById = new Dictionary<string, ItemDefinition>();
        private readonly Dictionary<ItemRarity, List<ItemDefinition>> _itemsByRarity = new Dictionary<ItemRarity, List<ItemDefinition>>();
        private readonly string _contentRoot;

        /// <summary>
        /// Creates a new ItemRegistry with the specified content root path
        /// </summary>
        /// <param name="contentRoot">Root directory for content files (default: "Content")</param>
        public ItemRegistry(string contentRoot = "Content")
        {
            _contentRoot = contentRoot;
            InitializeRarityLists();
        }

        private void InitializeRarityLists()
        {
            foreach (ItemRarity rarity in Enum.GetValues<ItemRarity>())
            {
                _itemsByRarity[rarity] = new List<ItemDefinition>();
            }
        }

        /// <summary>
        /// Load item definitions from a JSON file
        /// </summary>
        /// <param name="path">Path to JSON file (relative to content root or absolute)</param>
        public void LoadItemsFromJson(string path)
        {
            string fullPath = Path.IsPathRooted(path) ? path : Path.Combine(_contentRoot, path);
            if (!Path.HasExtension(fullPath))
                fullPath += ".json";

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Item definition file not found: {fullPath}");

            var json = File.ReadAllText(fullPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };

            var collection = JsonSerializer.Deserialize<ItemDefinitionCollection>(json, options);

            if (collection?.Items == null)
                throw new InvalidOperationException($"Failed to load items from: {fullPath}");

            foreach (var item in collection.Items)
            {
                RegisterItem(item);
            }

            Console.WriteLine($"Loaded {collection.Items.Count} items from {fullPath}");
        }

        /// <summary>
        /// Register a single item definition
        /// </summary>
        /// <param name="item">Item definition to register</param>
        public void RegisterItem(ItemDefinition item)
        {
            if (string.IsNullOrEmpty(item.Id))
                throw new ArgumentException("Item ID cannot be null or empty");

            if (_itemsById.ContainsKey(item.Id))
                Console.WriteLine($"Warning: Overwriting existing item definition: {item.Id}");

            _itemsById[item.Id] = item;

            // Add to rarity index if not already present
            if (!_itemsByRarity[item.Rarity].Contains(item))
            {
                _itemsByRarity[item.Rarity].Add(item);
            }
        }

        /// <summary>
        /// Get item definition by ID
        /// </summary>
        /// <param name="itemId">Item ID to look up</param>
        /// <returns>Item definition</returns>
        /// <exception cref="KeyNotFoundException">If item not found</exception>
        public ItemDefinition GetItem(string itemId)
        {
            if (_itemsById.TryGetValue(itemId, out var item))
                return item;

            throw new KeyNotFoundException($"Item not found: {itemId}");
        }

        /// <summary>
        /// Try to get item definition by ID
        /// </summary>
        /// <param name="itemId">Item ID to look up</param>
        /// <param name="item">Output item definition if found</param>
        /// <returns>True if item was found, false otherwise</returns>
        public bool TryGetItem(string itemId, out ItemDefinition item)
        {
            return _itemsById.TryGetValue(itemId, out item);
        }

        /// <summary>
        /// Get all items of a specific rarity tier
        /// </summary>
        /// <param name="rarity">Rarity tier to filter by</param>
        /// <returns>Read-only list of items with the specified rarity</returns>
        public IReadOnlyList<ItemDefinition> GetItemsByRarity(ItemRarity rarity)
        {
            return _itemsByRarity[rarity];
        }

        /// <summary>
        /// Get all registered items
        /// </summary>
        /// <returns>Read-only collection of all items</returns>
        public IReadOnlyCollection<ItemDefinition> GetAllItems()
        {
            return _itemsById.Values;
        }

        /// <summary>
        /// Check if an item is registered
        /// </summary>
        /// <param name="itemId">Item ID to check</param>
        /// <returns>True if the item exists in the registry</returns>
        public bool HasItem(string itemId)
        {
            return _itemsById.ContainsKey(itemId);
        }

        /// <summary>
        /// Get the count of registered items
        /// </summary>
        public int ItemCount => _itemsById.Count;

        /// <summary>
        /// Clear all registered items
        /// </summary>
        public void Clear()
        {
            _itemsById.Clear();
            foreach (var list in _itemsByRarity.Values)
            {
                list.Clear();
            }
        }
    }
}
