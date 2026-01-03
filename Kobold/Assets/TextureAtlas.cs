using Kobold.Core.Abstractions.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Kobold.Core.Assets
{
    /// <summary>
    /// Represents a texture atlas - a single texture containing multiple named sprite regions.
    /// Unlike sprite sheets (which use uniform grids), atlases support irregular sprite sizes
    /// and efficient packing.
    /// </summary>
    public class TextureAtlas
    {
        /// <summary>
        /// The texture containing all sprites
        /// </summary>
        public ITexture Texture { get; }

        /// <summary>
        /// Configuration defining named regions in the atlas
        /// </summary>
        public TextureAtlasConfig Config { get; }

        private readonly Dictionary<string, AtlasRegion> _regions;

        /// <summary>
        /// Create a new texture atlas with a texture and configuration
        /// </summary>
        /// <param name="texture">The texture containing the sprites</param>
        /// <param name="config">Configuration defining sprite regions</param>
        public TextureAtlas(ITexture texture, TextureAtlasConfig config)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            Config = config ?? throw new ArgumentNullException(nameof(config));

            // Build lookup dictionary for fast region access
            _regions = new Dictionary<string, AtlasRegion>();
            foreach (var region in config.Regions)
            {
                _regions[region.Name] = region;
            }
        }

        /// <summary>
        /// Get a sprite region by name
        /// </summary>
        /// <param name="name">Name of the sprite region</param>
        /// <returns>The atlas region</returns>
        /// <exception cref="KeyNotFoundException">Thrown if region name doesn't exist</exception>
        public AtlasRegion GetRegion(string name)
        {
            if (!_regions.TryGetValue(name, out var region))
            {
                throw new KeyNotFoundException($"Atlas region '{name}' not found in texture atlas.");
            }
            return region;
        }

        /// <summary>
        /// Try to get a sprite region by name
        /// </summary>
        /// <param name="name">Name of the sprite region</param>
        /// <param name="region">The found region, or default if not found</param>
        /// <returns>True if the region was found</returns>
        public bool TryGetRegion(string name, out AtlasRegion region)
        {
            return _regions.TryGetValue(name, out region);
        }

        /// <summary>
        /// Check if a region with the given name exists
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns>True if the region exists</returns>
        public bool HasRegion(string name)
        {
            return _regions.ContainsKey(name);
        }

        /// <summary>
        /// Get all region names in the atlas
        /// </summary>
        /// <returns>Collection of all region names</returns>
        public IEnumerable<string> GetRegionNames()
        {
            return _regions.Keys;
        }

        /// <summary>
        /// Get all regions in the atlas
        /// </summary>
        /// <returns>Collection of all regions</returns>
        public IEnumerable<AtlasRegion> GetAllRegions()
        {
            return _regions.Values;
        }

        /// <summary>
        /// Get the source rectangle for a named region
        /// </summary>
        /// <param name="name">Name of the region</param>
        /// <returns>Rectangle defining the region's bounds in the texture</returns>
        public Rectangle GetSourceRect(string name)
        {
            return GetRegion(name).Bounds;
        }
    }

    /// <summary>
    /// Configuration for a texture atlas defining all sprite regions
    /// </summary>
    public class TextureAtlasConfig
    {
        /// <summary>
        /// List of all sprite regions in the atlas
        /// </summary>
        public List<AtlasRegion> Regions { get; set; } = new List<AtlasRegion>();

        /// <summary>
        /// Optional metadata about the atlas
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Defines a named sprite region within a texture atlas
    /// </summary>
    public class AtlasRegion
    {
        /// <summary>
        /// Unique name for this sprite region
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Rectangle defining the region's bounds in the texture (X, Y, Width, Height)
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Optional pivot point for the sprite (0,0 = top-left, 0.5,0.5 = center, 1,1 = bottom-right)
        /// </summary>
        public Vector2 Pivot { get; set; } = new Vector2(0.5f, 0.5f);

        /// <summary>
        /// Whether the sprite is rotated 90 degrees clockwise in the atlas (for packed atlases)
        /// </summary>
        public bool Rotated { get; set; } = false;

        /// <summary>
        /// Original size before trimming (if the sprite was trimmed for packing efficiency)
        /// </summary>
        public Rectangle? OriginalSize { get; set; }

        /// <summary>
        /// Optional tags for categorizing this sprite
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Optional custom properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Create a simple atlas region with just a name and bounds
        /// </summary>
        /// <param name="name">Region name</param>
        /// <param name="x">X position in texture</param>
        /// <param name="y">Y position in texture</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <returns>A new atlas region</returns>
        public static AtlasRegion Create(string name, int x, int y, int width, int height)
        {
            return new AtlasRegion
            {
                Name = name,
                Bounds = new Rectangle(x, y, width, height)
            };
        }

        /// <summary>
        /// Create an atlas region with a custom pivot point
        /// </summary>
        /// <param name="name">Region name</param>
        /// <param name="x">X position in texture</param>
        /// <param name="y">Y position in texture</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="pivotX">Pivot X (0-1)</param>
        /// <param name="pivotY">Pivot Y (0-1)</param>
        /// <returns>A new atlas region</returns>
        public static AtlasRegion CreateWithPivot(string name, int x, int y, int width, int height, float pivotX, float pivotY)
        {
            return new AtlasRegion
            {
                Name = name,
                Bounds = new Rectangle(x, y, width, height),
                Pivot = new Vector2(pivotX, pivotY)
            };
        }
    }
}
