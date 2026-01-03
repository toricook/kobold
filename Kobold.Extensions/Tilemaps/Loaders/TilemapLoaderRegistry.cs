using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kobold.Core;
using Kobold.Core.Services;

namespace Kobold.Extensions.Tilemaps.Loaders
{
    /// <summary>
    /// Registry for tilemap loaders supporting multiple file formats.
    /// Follows the same pattern as ItemRegistry and MonsterRegistry in the Kobold framework.
    /// </summary>
    public class TilemapLoaderRegistry
    {
        private readonly Dictionary<string, ITilemapLoader> _loadersByExtension = new();
        private readonly Dictionary<string, ITilemapLoader> _loadersByFormat = new();

        /// <summary>
        /// Register a tilemap loader for its supported file format.
        /// </summary>
        /// <param name="loader">The loader to register</param>
        public void RegisterLoader(ITilemapLoader loader)
        {
            if (loader == null)
                throw new ArgumentNullException(nameof(loader));

            var extension = loader.SupportedExtension.ToLowerInvariant();
            var format = loader.FormatName.ToLowerInvariant();

            _loadersByExtension[extension] = loader;
            _loadersByFormat[format] = loader;

            Console.WriteLine($"Registered tilemap loader: {loader.FormatName} ({loader.SupportedExtension})");
        }

        /// <summary>
        /// Load a tilemap using the appropriate loader based on file extension.
        /// </summary>
        /// <param name="path">Path to the tilemap file</param>
        /// <param name="assetManager">Asset manager for loading resources</param>
        /// <returns>Loaded tilemap data</returns>
        /// <exception cref="FileNotFoundException">If the file doesn't exist</exception>
        /// <exception cref="NotSupportedException">If no loader is registered for the file extension</exception>
        public TilemapData LoadTilemap(string path, AssetManager assetManager)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Tilemap file not found: {path}", path);

            var extension = Path.GetExtension(path).ToLowerInvariant();

            if (!_loadersByExtension.TryGetValue(extension, out var loader))
            {
                throw new NotSupportedException(
                    $"No loader registered for extension '{extension}'. " +
                    $"Supported extensions: {string.Join(", ", _loadersByExtension.Keys)}");
            }

            return loader.Load(path, assetManager);
        }

        /// <summary>
        /// Load a tilemap using a specific format loader by name.
        /// </summary>
        /// <param name="path">Path to the tilemap file</param>
        /// <param name="format">Format name (e.g., "Tiled", "Custom")</param>
        /// <param name="assetManager">Asset manager for loading resources</param>
        /// <returns>Loaded tilemap data</returns>
        /// <exception cref="FileNotFoundException">If the file doesn't exist</exception>
        /// <exception cref="NotSupportedException">If no loader is registered for the format</exception>
        public TilemapData LoadTilemapWithFormat(string path, string format, AssetManager assetManager)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Tilemap file not found: {path}", path);

            if (!_loadersByFormat.TryGetValue(format.ToLowerInvariant(), out var loader))
            {
                throw new NotSupportedException(
                    $"No loader registered for format '{format}'. " +
                    $"Supported formats: {string.Join(", ", _loadersByFormat.Keys)}");
            }

            return loader.Load(path, assetManager);
        }

        /// <summary>
        /// Check if a file extension is supported by any registered loader.
        /// </summary>
        /// <param name="extension">File extension (with or without leading dot)</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool IsExtensionSupported(string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            return _loadersByExtension.ContainsKey(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Get all supported file extensions.
        /// </summary>
        /// <returns>Collection of supported extensions (e.g., ".tmx", ".json")</returns>
        public IEnumerable<string> GetSupportedExtensions()
        {
            return _loadersByExtension.Keys;
        }

        /// <summary>
        /// Get all registered format names.
        /// </summary>
        /// <returns>Collection of format names (e.g., "Tiled", "Custom")</returns>
        public IEnumerable<string> GetSupportedFormats()
        {
            return _loadersByFormat.Keys;
        }

        /// <summary>
        /// Get the loader for a specific file extension.
        /// </summary>
        /// <param name="extension">File extension (with or without leading dot)</param>
        /// <param name="loader">The found loader, or null if not found</param>
        /// <returns>True if a loader was found, false otherwise</returns>
        public bool TryGetLoaderByExtension(string extension, out ITilemapLoader? loader)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            return _loadersByExtension.TryGetValue(extension.ToLowerInvariant(), out loader);
        }

        /// <summary>
        /// Get the loader for a specific format name.
        /// </summary>
        /// <param name="format">Format name (e.g., "Tiled", "Custom")</param>
        /// <param name="loader">The found loader, or null if not found</param>
        /// <returns>True if a loader was found, false otherwise</returns>
        public bool TryGetLoaderByFormat(string format, out ITilemapLoader? loader)
        {
            return _loadersByFormat.TryGetValue(format.ToLowerInvariant(), out loader);
        }
    }
}
