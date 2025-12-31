using System.Collections.Generic;
using Kobold.Core.Assets;

namespace Kobold.Extensions.Items.Data
{
    /// <summary>
    /// Complete item definition loaded from JSON.
    /// Defines all properties needed to spawn and display an item.
    /// </summary>
    public class ItemDefinition
    {
        /// <summary>Unique identifier for this item</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Display name for UI</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Description text for tooltips/UI</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Item rarity tier (affects spawn weights)</summary>
        public ItemRarity Rarity { get; set; } = ItemRarity.Common;

        /// <summary>Sprite rendering configuration</summary>
        public SpriteData Sprite { get; set; } = new SpriteData();

        /// <summary>Pickup behavior configuration</summary>
        public PickupData Pickup { get; set; } = new PickupData();

        /// <summary>Collision box configuration</summary>
        public CollisionData Collision { get; set; } = new CollisionData();

        /// <summary>Relative spawn weight within rarity tier (higher = more common)</summary>
        public int SpawnWeight { get; set; } = 100;

        /// <summary>Maximum stack size (1 for non-stackable items)</summary>
        public int MaxStackSize { get; set; } = 1;
    }

    /// <summary>
    /// Sprite rendering configuration for an item
    /// </summary>
    public class SpriteData
    {
        /// <summary>Path to the sprite sheet asset (without extension)</summary>
        public string TexturePath { get; set; } = string.Empty;

        /// <summary>Named region in the sprite sheet</summary>
        public string RegionName { get; set; } = string.Empty;

        /// <summary>Rendering scale (1.0 = original size)</summary>
        public Vector2Data Scale { get; set; } = new Vector2Data { X = 1f, Y = 1f };
    }

    /// <summary>
    /// Pickup behavior configuration
    /// </summary>
    public class PickupData
    {
        /// <summary>Effect type name (mapped via IPickupEffectFactory)</summary>
        public string EffectType { get; set; } = string.Empty;

        /// <summary>Parameters passed to the effect factory</summary>
        public Dictionary<string, object> EffectParams { get; set; } = new Dictionary<string, object>();

        /// <summary>If true, requires button press to collect</summary>
        public bool RequiresInteraction { get; set; } = false;

        /// <summary>Tag identifier for this pickup type</summary>
        public string PickupTag { get; set; } = string.Empty;

        /// <summary>Whether this item stacks in inventory</summary>
        public bool IsStackable { get; set; } = true;
    }

    /// <summary>
    /// Collision box configuration
    /// </summary>
    public class CollisionData
    {
        /// <summary>Collision box width in pixels</summary>
        public float Width { get; set; } = 32f;

        /// <summary>Collision box height in pixels</summary>
        public float Height { get; set; } = 32f;

        /// <summary>Collision box offset from entity position</summary>
        public Vector2Data Offset { get; set; } = new Vector2Data { X = -16f, Y = -16f };

        /// <summary>Collision layer name (e.g., "Trigger", "Pickup")</summary>
        public string CollisionLayer { get; set; } = "Trigger";
    }

    /// <summary>
    /// Root collection for JSON deserialization
    /// </summary>
    public class ItemDefinitionCollection
    {
        /// <summary>List of all item definitions</summary>
        public List<ItemDefinition> Items { get; set; } = new List<ItemDefinition>();
    }
}
