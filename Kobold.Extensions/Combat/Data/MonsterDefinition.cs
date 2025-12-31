using System.Collections.Generic;
using Kobold.Core.Assets;

namespace Kobold.Extensions.Combat.Data
{
    /// <summary>
    /// Complete monster definition loaded from JSON.
    /// Defines all properties needed to spawn and configure a monster entity.
    /// </summary>
    public class MonsterDefinition
    {
        /// <summary>Unique identifier for this monster type</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Display name for UI</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Description text for tooltips/UI</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Sprite rendering configuration</summary>
        public SpriteData Sprite { get; set; } = new SpriteData();

        /// <summary>Health and damage resistance configuration</summary>
        public HealthData Health { get; set; } = new HealthData();

        /// <summary>AI behavior configuration</summary>
        public AIData AI { get; set; } = new AIData();

        /// <summary>Collision box configuration</summary>
        public CollisionData Collision { get; set; } = new CollisionData();

        /// <summary>Relative spawn weight (higher = more common)</summary>
        public int SpawnWeight { get; set; } = 100;

        /// <summary>Monster tag for targeting systems (e.g., "Enemy")</summary>
        public string Tag { get; set; } = "Enemy";
    }

    /// <summary>
    /// Sprite rendering configuration for a monster
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
    /// Health configuration for a monster
    /// </summary>
    public class HealthData
    {
        /// <summary>Maximum health points</summary>
        public int MaxHealth { get; set; } = 30;

        /// <summary>Duration of invulnerability after taking damage (in seconds)</summary>
        public float InvulnerabilityDuration { get; set; } = 0.2f;
    }

    /// <summary>
    /// AI behavior configuration
    /// </summary>
    public class AIData
    {
        /// <summary>Detection range in pixels (how close player must be to start chasing)</summary>
        public float DetectionRange { get; set; } = 150f;

        /// <summary>Movement speed in pixels per second</summary>
        public float MoveSpeed { get; set; } = 60f;

        /// <summary>Attack range in pixels (how close to attack)</summary>
        public float AttackRange { get; set; } = 35f;

        /// <summary>Damage dealt per attack</summary>
        public int AttackDamage { get; set; } = 15;

        /// <summary>Cooldown between attacks in seconds</summary>
        public float AttackCooldown { get; set; } = 1.5f;
    }

    /// <summary>
    /// Collision box configuration
    /// </summary>
    public class CollisionData
    {
        /// <summary>Collision box width in pixels</summary>
        public float Width { get; set; } = 28f;

        /// <summary>Collision box height in pixels</summary>
        public float Height { get; set; } = 28f;

        /// <summary>Collision box offset from entity position</summary>
        public Vector2Data Offset { get; set; } = new Vector2Data { X = -14f, Y = -14f };

        /// <summary>Collision layer name (e.g., "Enemy", "Default")</summary>
        public string CollisionLayer { get; set; } = "Enemy";
    }

    /// <summary>
    /// Root collection for JSON deserialization
    /// </summary>
    public class MonsterDefinitionCollection
    {
        /// <summary>List of all monster definitions</summary>
        public List<MonsterDefinition> Monsters { get; set; } = new List<MonsterDefinition>();
    }
}
