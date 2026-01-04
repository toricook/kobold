using System;
using System.Numerics;
using Arch.Core;
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Services;
using Kobold.Core.Systems;
using Kobold.Extensions.Combat.Components;
using Kobold.Extensions.Combat.Data;
using Kobold.Extensions.Collision.Systems;
using Kobold.Extensions.Collision.Components;

using Kobold.Extensions.Physics.Components;
namespace Kobold.Extensions.Combat.Spawning
{
    /// <summary>
    /// Creates monster entities from monster definitions.
    /// Instantiates complete entities with all required components.
    /// </summary>
    public class MonsterFactory
    {
        private readonly World _world;
        private readonly AssetManager _assetManager;

        /// <summary>
        /// Creates a new MonsterFactory
        /// </summary>
        /// <param name="world">ECS world for entity creation</param>
        /// <param name="assetManager">Asset manager for loading sprites</param>
        public MonsterFactory(World world, AssetManager assetManager)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _assetManager = assetManager ?? throw new ArgumentNullException(nameof(assetManager));
        }

        /// <summary>
        /// Create a monster entity from monster definition
        /// </summary>
        /// <param name="monster">Monster definition to instantiate</param>
        /// <param name="position">World position to spawn at</param>
        /// <returns>Created entity reference</returns>
        public Entity CreateMonsterEntity(MonsterDefinition monster, Vector2 position)
        {
            if (monster == null)
                throw new ArgumentNullException(nameof(monster));

            // Load sprite sheet
            var spriteSheet = _assetManager.GetSpriteSheet(monster.Sprite.TexturePath);
            if (spriteSheet == null)
                throw new InvalidOperationException($"Sprite sheet not loaded: {monster.Sprite.TexturePath}");

            // Parse collision layer
            var collisionLayer = ParseCollisionLayer(monster.Collision.CollisionLayer);

            // Create entity with all components
            var entity = _world.Create(
                new Transform(position),
                new Velocity(Vector2.Zero),
                new SpriteRenderer(
                    spriteSheet.Texture,
                    spriteSheet.GetNamedRegion(monster.Sprite.RegionName),
                    new Vector2(monster.Sprite.Scale.X, monster.Sprite.Scale.Y)
                ),
                new BoxCollider(
                    monster.Collision.Width,
                    monster.Collision.Height,
                    new Vector2(monster.Collision.Offset.X, monster.Collision.Offset.Y)
                ),
                new CollisionLayerComponent(collisionLayer),
                new HealthComponent(
                    maxHealth: monster.Health.MaxHealth,
                    invulnerabilityDuration: monster.Health.InvulnerabilityDuration
                ),
                new SimpleAIComponent(
                    detectionRange: monster.AI.DetectionRange,
                    moveSpeed: monster.AI.MoveSpeed,
                    attackRange: monster.AI.AttackRange,
                    attackDamage: monster.AI.AttackDamage,
                    attackCooldown: monster.AI.AttackCooldown
                )
            );

            // Add tag component based on monster tag
            AddTagComponent(entity, monster.Tag);

            return entity;
        }

        /// <summary>
        /// Add appropriate tag component to the entity
        /// </summary>
        private void AddTagComponent(Entity entity, string tag)
        {
            switch (tag.ToLower())
            {
                case "enemy":
                    _world.Add(entity, new Enemy());
                    break;
                case "player":
                    _world.Add(entity, new Player());
                    break;
                default:
                    // Default to Enemy tag if unknown
                    _world.Add(entity, new Enemy());
                    Console.WriteLine($"Warning: Unknown tag '{tag}', using Enemy as default");
                    break;
            }
        }

        /// <summary>
        /// Parse collision layer name from string
        /// </summary>
        private CollisionLayer ParseCollisionLayer(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                return CollisionLayer.Enemy; // Default for monsters

            if (Enum.TryParse<CollisionLayer>(layerName, ignoreCase: true, out var layer))
                return layer;

            Console.WriteLine($"Warning: Unknown collision layer '{layerName}', using Enemy as default");
            return CollisionLayer.Enemy;
        }
    }
}
