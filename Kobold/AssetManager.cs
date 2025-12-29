using Kobold.Core.Abstractions.Core;
using Kobold.Core.Abstractions.Rendering;
using System.Collections.Generic;

namespace Kobold.Core
{
    /// <summary>
    /// Manages loading and caching of game assets (textures, etc.)
    /// </summary>
    public class AssetManager
    {
        private readonly IContentLoader _contentLoader;
        private readonly Dictionary<string, ITexture> _textureCache;

        public AssetManager(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
            _textureCache = new Dictionary<string, ITexture>();
        }

        /// <summary>
        /// Load a texture, or return cached version if already loaded
        /// </summary>
        /// <param name="path">Path to the texture file</param>
        /// <returns>The loaded texture</returns>
        public ITexture LoadTexture(string path)
        {
            // Check if already cached
            if (_textureCache.TryGetValue(path, out var cachedTexture))
            {
                return cachedTexture;
            }

            // Load and cache
            var texture = _contentLoader.LoadTexture(path);
            _textureCache[path] = texture;
            return texture;
        }

        /// <summary>
        /// Get a previously loaded texture from cache
        /// </summary>
        /// <param name="path">Path to the texture</param>
        /// <returns>The cached texture, or null if not loaded</returns>
        public ITexture? GetTexture(string path)
        {
            _textureCache.TryGetValue(path, out var texture);
            return texture;
        }

        /// <summary>
        /// Check if a texture is already loaded in cache
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the texture is cached</returns>
        public bool IsTextureLoaded(string path)
        {
            return _textureCache.ContainsKey(path);
        }

        /// <summary>
        /// Preload multiple textures at once (useful for loading screens)
        /// </summary>
        /// <param name="paths">Array of texture paths to preload</param>
        public void PreloadTextures(params string[] paths)
        {
            foreach (var path in paths)
            {
                LoadTexture(path);
            }
        }

        /// <summary>
        /// Unload a specific texture from cache
        /// </summary>
        /// <param name="path">Path to the texture to unload</param>
        /// <returns>True if the texture was unloaded, false if it wasn't cached</returns>
        public bool UnloadTexture(string path)
        {
            return _textureCache.Remove(path);
        }

        /// <summary>
        /// Unload all cached textures
        /// </summary>
        public void UnloadAllTextures()
        {
            _textureCache.Clear();
        }

        /// <summary>
        /// Get count of currently cached textures
        /// </summary>
        public int CachedTextureCount => _textureCache.Count;

        /// <summary>
        /// Get all currently cached texture paths
        /// </summary>
        public IEnumerable<string> GetCachedTexturePaths()
        {
            return _textureCache.Keys;
        }
    }
}
