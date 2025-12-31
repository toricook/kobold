namespace Kobold.Extensions.Items
{
    /// <summary>
    /// Defines item rarity tiers for loot system.
    /// Higher rarity values represent rarer items.
    /// </summary>
    public enum ItemRarity
    {
        /// <summary>Common items - most frequently found</summary>
        Common = 0,

        /// <summary>Uncommon items - occasionally found</summary>
        Uncommon = 1,

        /// <summary>Rare items - infrequently found</summary>
        Rare = 2,

        /// <summary>Epic items - very rare</summary>
        Epic = 3,

        /// <summary>Legendary items - extremely rare</summary>
        Legendary = 4
    }
}
