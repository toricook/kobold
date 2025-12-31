using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Kobold.Extensions.Items.Data;

namespace Kobold.Extensions.Items.Registry
{
    /// <summary>
    /// Registry for loot table definitions.
    /// Loads from JSON and provides lookup API for loot tables.
    /// </summary>
    public class LootTableRegistry
    {
        private readonly Dictionary<string, LootTableDefinition> _lootTables = new Dictionary<string, LootTableDefinition>();
        private readonly string _contentRoot;

        /// <summary>
        /// Creates a new LootTableRegistry with the specified content root path
        /// </summary>
        /// <param name="contentRoot">Root directory for content files (default: "Content")</param>
        public LootTableRegistry(string contentRoot = "Content")
        {
            _contentRoot = contentRoot;
        }

        /// <summary>
        /// Load loot table definitions from a JSON file
        /// </summary>
        /// <param name="path">Path to JSON file (relative to content root or absolute)</param>
        public void LoadLootTablesFromJson(string path)
        {
            string fullPath = Path.IsPathRooted(path) ? path : Path.Combine(_contentRoot, path);
            if (!Path.HasExtension(fullPath))
                fullPath += ".json";

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Loot table file not found: {fullPath}");

            var json = File.ReadAllText(fullPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };

            var collection = JsonSerializer.Deserialize<LootTableCollection>(json, options);

            if (collection?.LootTables == null)
                throw new InvalidOperationException($"Failed to load loot tables from: {fullPath}");

            foreach (var kvp in collection.LootTables)
            {
                RegisterLootTable(kvp.Value);
            }

            Console.WriteLine($"Loaded {collection.LootTables.Count} loot tables from {fullPath}");
        }

        /// <summary>
        /// Register a single loot table definition
        /// </summary>
        /// <param name="table">Loot table definition to register</param>
        public void RegisterLootTable(LootTableDefinition table)
        {
            if (string.IsNullOrEmpty(table.Id))
                throw new ArgumentException("Loot table ID cannot be null or empty");

            if (_lootTables.ContainsKey(table.Id))
                Console.WriteLine($"Warning: Overwriting existing loot table: {table.Id}");

            _lootTables[table.Id] = table;
        }

        /// <summary>
        /// Get loot table definition by ID
        /// </summary>
        /// <param name="tableId">Loot table ID to look up</param>
        /// <returns>Loot table definition</returns>
        /// <exception cref="KeyNotFoundException">If loot table not found</exception>
        public LootTableDefinition GetLootTable(string tableId)
        {
            if (_lootTables.TryGetValue(tableId, out var table))
                return table;

            throw new KeyNotFoundException($"Loot table not found: {tableId}");
        }

        /// <summary>
        /// Try to get loot table definition by ID
        /// </summary>
        /// <param name="tableId">Loot table ID to look up</param>
        /// <param name="table">Output loot table definition if found</param>
        /// <returns>True if loot table was found, false otherwise</returns>
        public bool TryGetLootTable(string tableId, out LootTableDefinition table)
        {
            return _lootTables.TryGetValue(tableId, out table);
        }

        /// <summary>
        /// Get all registered loot tables
        /// </summary>
        /// <returns>Read-only collection of all loot tables</returns>
        public IReadOnlyCollection<LootTableDefinition> GetAllLootTables()
        {
            return _lootTables.Values;
        }

        /// <summary>
        /// Check if a loot table is registered
        /// </summary>
        /// <param name="tableId">Loot table ID to check</param>
        /// <returns>True if the loot table exists in the registry</returns>
        public bool HasLootTable(string tableId)
        {
            return _lootTables.ContainsKey(tableId);
        }

        /// <summary>
        /// Get the count of registered loot tables
        /// </summary>
        public int LootTableCount => _lootTables.Count;

        /// <summary>
        /// Clear all registered loot tables
        /// </summary>
        public void Clear()
        {
            _lootTables.Clear();
        }
    }
}
