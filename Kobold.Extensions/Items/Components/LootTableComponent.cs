namespace Kobold.Extensions.Items.Components
{
    /// <summary>
    /// Component that marks an entity as having loot to drop on destruction.
    /// When the entity is destroyed, LootDropSystem will roll the specified loot table
    /// and spawn items at the entity's death position.
    /// </summary>
    public struct LootTableComponent
    {
        /// <summary>ID of the loot table to roll when entity is destroyed</summary>
        public string LootTableId { get; set; }

        /// <summary>
        /// Radius for random position offset when dropping items (in pixels).
        /// Items are scattered within this radius for visual variety.
        /// </summary>
        public float DropPositionOffsetRadius { get; set; }

        /// <summary>
        /// Creates a new LootTableComponent
        /// </summary>
        /// <param name="lootTableId">ID of the loot table to use</param>
        /// <param name="offsetRadius">Scatter radius for dropped items (default: 16 pixels)</param>
        public LootTableComponent(string lootTableId, float offsetRadius = 16f)
        {
            LootTableId = lootTableId;
            DropPositionOffsetRadius = offsetRadius;
        }
    }
}
