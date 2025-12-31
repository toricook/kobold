using System;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Components.Gameplay;
using Kobold.Core.Systems;
using Kobold.Extensions.Items.Data;
using Kobold.Extensions.Items.Effects;
using Kobold.Extensions.Pickups;

namespace Kobold.Extensions.Items.Spawning
{
    /// <summary>
    /// Creates pickup entities from item definitions.
    /// Instantiates complete entities with all required components.
    /// </summary>
    public class ItemFactory
    {
        private readonly World _world;
        private readonly AssetManager _assetManager;
        private readonly IPickupEffectFactory _effectFactory;

        /// <summary>
        /// Creates a new ItemFactory
        /// </summary>
        /// <param name="world">ECS world for entity creation</param>
        /// <param name="assetManager">Asset manager for loading sprites</param>
        /// <param name="effectFactory">Factory for creating pickup effects</param>
        public ItemFactory(
            World world,
            AssetManager assetManager,
            IPickupEffectFactory effectFactory)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _assetManager = assetManager ?? throw new ArgumentNullException(nameof(assetManager));
            _effectFactory = effectFactory ?? throw new ArgumentNullException(nameof(effectFactory));
        }

        /// <summary>
        /// Create a pickup entity from item definition
        /// </summary>
        /// <param name="item">Item definition to instantiate</param>
        /// <param name="position">World position to spawn at</param>
        /// <returns>Created entity reference</returns>
        public Entity CreatePickupEntity(ItemDefinition item, Vector2 position)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Load sprite sheet
            var spriteSheet = _assetManager.GetSpriteSheet(item.Sprite.TexturePath);
            if (spriteSheet == null)
                throw new InvalidOperationException($"Sprite sheet not loaded: {item.Sprite.TexturePath}");

            // Create pickup effect
            var effect = _effectFactory.CreateEffect(
                item.Pickup.EffectType,
                item.Pickup.EffectParams
            );

            // Parse collision layer
            var collisionLayer = ParseCollisionLayer(item.Collision.CollisionLayer);

            // Create entity with base components
            var entity = _world.Create(
                new Transform(position),
                new SpriteRenderer(
                    spriteSheet.Texture,
                    spriteSheet.GetNamedRegion(item.Sprite.RegionName),
                    new Vector2(item.Sprite.Scale.X, item.Sprite.Scale.Y)
                ),
                new BoxCollider(
                    item.Collision.Width,
                    item.Collision.Height,
                    new Vector2(item.Collision.Offset.X, item.Collision.Offset.Y)
                ),
                new CollisionLayerComponent(collisionLayer),
                new PickupComponent(
                    effect: effect,
                    requiresInteraction: item.Pickup.RequiresInteraction,
                    pickupTag: item.Pickup.PickupTag
                ),
                new PowerUp() // Tag component for categorization
            );

            // Add trigger components if interactive pickup
            if (item.Pickup.RequiresInteraction)
            {
                _world.Add(entity, new Trigger());
                _world.Add(entity, new TriggerComponent(
                    mode: TriggerMode.OnStayWithButton,
                    activationLayers: CollisionLayer.Player,
                    triggerTag: item.Pickup.PickupTag
                ));
            }

            return entity;
        }

        /// <summary>
        /// Parse collision layer name from string
        /// </summary>
        private CollisionLayer ParseCollisionLayer(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                return CollisionLayer.Trigger; // Default

            if (Enum.TryParse<CollisionLayer>(layerName, ignoreCase: true, out var layer))
                return layer;

            Console.WriteLine($"Warning: Unknown collision layer '{layerName}', using Trigger as default");
            return CollisionLayer.Trigger;
        }
    }
}
