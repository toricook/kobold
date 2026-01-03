using Kobold.Core;

namespace Kobold.Extensions.Tilemaps.Loaders
{
    /// <summary>
    /// Interface for tilemap loaders that support various file formats.
    /// Implementations load tilemaps from files and return structured data.
    /// </summary>
    public interface ITilemapLoader
    {
        /// <summary>
        /// Loads a tilemap from the specified path.
        /// </summary>
        /// <param name="path">Path to the tilemap file</param>
        /// <param name="assetManager">Asset manager for loading textures and other assets</param>
        /// <returns>Loaded tilemap data including TileMap, TileSet, object layers, and metadata</returns>
        TilemapData Load(string path, AssetManager assetManager);

        /// <summary>
        /// Gets the file extension this loader supports (e.g., ".tmx", ".json")
        /// </summary>
        string SupportedExtension { get; }

        /// <summary>
        /// Gets the format name for this loader (e.g., "Tiled", "Custom")
        /// </summary>
        string FormatName { get; }
    }
}
