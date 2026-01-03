using System.Collections.Generic;
using System.Numerics;
using Kobold.Core.Abstractions.Rendering;

namespace Kobold.Extensions.Tilemaps.Loaders
{
    /// <summary>
    /// Container for all data loaded from a tilemap file.
    /// Includes the tilemap, tileset, object layers, and metadata.
    /// </summary>
    public class TilemapData
    {
        /// <summary>
        /// Core tilemap with tile grid data
        /// </summary>
        public TileMap TileMap { get; set; }

        /// <summary>
        /// Tileset with tile properties and configuration
        /// </summary>
        public TileSet TileSet { get; set; }

        /// <summary>
        /// List of object layers (spawn points, triggers, etc.)
        /// </summary>
        public List<TilemapObjectLayer> ObjectLayers { get; set; } = new();

        /// <summary>
        /// Custom properties from the map file
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();

        /// <summary>
        /// Asset paths that need to be loaded (textures, etc.)
        /// </summary>
        public List<string> RequiredAssets { get; set; } = new();

        /// <summary>
        /// Loaded textures for tilesets (optional, can be null if not loaded)
        /// </summary>
        public Dictionary<string, ITexture> LoadedTextures { get; set; } = new();

        /// <summary>
        /// Map metadata (author, version, etc.)
        /// </summary>
        public TilemapMetadata Metadata { get; set; } = new();

        public TilemapData(TileMap tileMap, TileSet tileSet)
        {
            TileMap = tileMap;
            TileSet = tileSet;
        }
    }

    /// <summary>
    /// Represents an object layer from a tilemap file.
    /// Object layers contain entities like spawn points, triggers, etc.
    /// </summary>
    public class TilemapObjectLayer
    {
        /// <summary>
        /// Name of the object layer
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Whether the layer is visible
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Layer opacity (0-1)
        /// </summary>
        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// Objects in this layer
        /// </summary>
        public List<TilemapObject> Objects { get; set; } = new();

        /// <summary>
        /// Custom properties for the layer
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Represents an object (entity) in a tilemap.
    /// Can be a point, rectangle, ellipse, or polygon.
    /// </summary>
    public class TilemapObject
    {
        /// <summary>
        /// Unique ID of the object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the object
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type identifier for the object (e.g., "player", "enemy", "trigger")
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// X position in pixels
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y position in pixels
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Width in pixels (0 for point objects)
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Height in pixels (0 for point objects)
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Rotation in degrees
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Whether the object is visible
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Shape of the object
        /// </summary>
        public TilemapObjectShape Shape { get; set; } = TilemapObjectShape.Rectangle;

        /// <summary>
        /// Points for polygon/polyline shapes (relative to X, Y)
        /// </summary>
        public List<Vector2> Points { get; set; } = new();

        /// <summary>
        /// Custom properties for the object
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Shape types for tilemap objects
    /// </summary>
    public enum TilemapObjectShape
    {
        /// <summary>Rectangle (default)</summary>
        Rectangle,

        /// <summary>Ellipse/circle</summary>
        Ellipse,

        /// <summary>Point (no size)</summary>
        Point,

        /// <summary>Closed polygon</summary>
        Polygon,

        /// <summary>Open polyline</summary>
        Polyline
    }

    /// <summary>
    /// Metadata about the tilemap file.
    /// </summary>
    public class TilemapMetadata
    {
        /// <summary>
        /// Tilemap format version
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Map orientation (e.g., "orthogonal", "isometric")
        /// </summary>
        public string Orientation { get; set; } = "orthogonal";

        /// <summary>
        /// Render order (e.g., "right-down")
        /// </summary>
        public string RenderOrder { get; set; } = "right-down";

        /// <summary>
        /// Tiled editor version (for Tiled maps)
        /// </summary>
        public int TiledVersion { get; set; }

        /// <summary>
        /// Additional custom metadata
        /// </summary>
        public Dictionary<string, string> CustomMetadata { get; set; } = new();
    }
}
