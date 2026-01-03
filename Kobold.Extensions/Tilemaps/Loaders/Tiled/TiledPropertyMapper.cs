using System;
using System.Collections.Generic;
using System.Linq;
using DotTiled;

namespace Kobold.Extensions.Tilemaps.Loaders.Tiled
{
    /// <summary>
    /// Maps Tiled custom properties to TileProperties.
    /// Uses convention-based mapping with extensibility for custom property handlers.
    ///
    /// NOTE: This is an initial implementation. The DotTiled property API may require
    /// adjustments based on testing with actual .tmx files that have custom tile properties.
    /// </summary>
    public class TiledPropertyMapper
    {
        private readonly Dictionary<string, Action<TileProperties, object>> _customMappers = new();

        /// <summary>
        /// Maps Tiled tile properties to TileProperties struct.
        /// </summary>
        public TileProperties MapProperties(Tile tile)
        {
            var props = new TileProperties();

            // Handle animations if present
            if (tile.Animation != null && tile.Animation.Count > 0)
            {
                MapAnimation(tile, ref props);
            }

            // TODO: Property mapping will be implemented once we can test with actual Tiled files
            // The DotTiled IProperty API needs to be verified with actual usage
            // For now, tiles will use default properties unless animations are present

            return props;
        }

        /// <summary>
        /// Maps Tiled tile animation to TileProperties animation fields.
        /// </summary>
        private void MapAnimation(Tile tile, ref TileProperties props)
        {
            if (tile.Animation == null || tile.Animation.Count == 0)
                return;

            props.IsAnimated = true;
            props.AnimationFrames = tile.Animation.Select(f => (int)f.TileID).ToArray();
            props.AnimationSpeed = CalculateAnimationSpeed(tile.Animation);
        }

        /// <summary>
        /// Calculates animation speed in frames per second from Tiled animation frames.
        /// </summary>
        private float CalculateAnimationSpeed(IList<Frame> frames)
        {
            if (frames == null || frames.Count == 0)
                return 0;

            var averageDurationMs = frames.Average(f => f.Duration);
            if (averageDurationMs > 0)
            {
                return 1000f / (float)averageDurationMs;
            }

            return 10f; // Default to 10 FPS
        }

        /// <summary>
        /// Parses a collision layer string to TileCollisionLayer enum.
        /// </summary>
        private TileCollisionLayer ParseCollisionLayer(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return TileCollisionLayer.None;

            if (Enum.TryParse<TileCollisionLayer>(value, ignoreCase: true, out var result))
                return result;

            return value.ToLowerInvariant() switch
            {
                "solid" or "wall" => TileCollisionLayer.Solid,
                "platform" or "oneway" => TileCollisionLayer.Platform,
                "trigger" or "sensor" => TileCollisionLayer.Trigger,
                "water" or "liquid" => TileCollisionLayer.Water,
                "ice" or "slippery" => TileCollisionLayer.Ice,
                "ladder" or "climb" => TileCollisionLayer.Ladder,
                _ => TileCollisionLayer.None
            };
        }

        /// <summary>
        /// Register a custom property mapper for specific property names.
        /// </summary>
        public void RegisterCustomMapper(string propertyName, Action<TileProperties, object> mapper)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            _customMappers[propertyName] = mapper;
        }
    }
}
