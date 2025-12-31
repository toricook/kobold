using System.Collections.Generic;

namespace Kobold.Extensions.Items.Data
{
    /// <summary>
    /// Defines how items are dropped when an entity is destroyed.
    /// Supports multiple roll modes and both item-specific and rarity-based drops.
    /// </summary>
    public class LootTableDefinition
    {
        /// <summary>Unique identifier for this loot table</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>List of possible loot entries</summary>
        public List<LootEntry> Entries { get; set; } = new List<LootEntry>();

        /// <summary>Minimum number of items guaranteed to drop</summary>
        public int GuaranteedDropCount { get; set; } = 0;

        /// <summary>Maximum number of items that can drop</summary>
        public int MaxDrops { get; set; } = 10;

        /// <summary>How to roll for loot drops</summary>
        public LootRollMode RollMode { get; set; } = LootRollMode.Independent;
    }

    /// <summary>
    /// Single entry in a loot table (either specific item or rarity-based)
    /// </summary>
    public class LootEntry
    {
        /// <summary>Specific item ID to drop (mutually exclusive with RarityBased)</summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>If true, select random item from Rarity tier instead of specific item</summary>
        public bool RarityBased { get; set; } = false;

        /// <summary>Rarity tier to select from (only used if RarityBased = true)</summary>
        public ItemRarity? Rarity { get; set; }

        /// <summary>Minimum quantity to drop</summary>
        public int MinQuantity { get; set; } = 1;

        /// <summary>Maximum quantity to drop</summary>
        public int MaxQuantity { get; set; } = 1;

        /// <summary>Probability this entry drops (0.0 to 1.0)</summary>
        public float DropChance { get; set; } = 1.0f;

        /// <summary>Override the item's natural rarity for this drop (optional)</summary>
        public ItemRarity? RarityOverride { get; set; }
    }

    /// <summary>
    /// Defines how loot entries are rolled
    /// </summary>
    public enum LootRollMode
    {
        /// <summary>Each entry rolls independently based on DropChance</summary>
        Independent,

        /// <summary>Roll entries in order until MaxDrops or GuaranteedDropCount is reached</summary>
        Sequential,

        /// <summary>Pick exactly one entry randomly from the list</summary>
        OneOf
    }

    /// <summary>
    /// Root collection for JSON deserialization
    /// </summary>
    public class LootTableCollection
    {
        /// <summary>Dictionary of loot tables keyed by ID</summary>
        public Dictionary<string, LootTableDefinition> LootTables { get; set; } = new Dictionary<string, LootTableDefinition>();
    }
}
