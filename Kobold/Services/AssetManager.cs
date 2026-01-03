using Kobold.Core.Abstractions.Assets;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Abstractions.Audio;
using Kobold.Core.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Kobold.Core.Services
{
    /// <summary>
    /// Manages loading and caching of game assets (textures, sprite sheets, etc.)
    /// </summary>
    public class AssetManager
    {
        private readonly IContentLoader _contentLoader;
        private readonly Dictionary<string, ITexture> _textureCache;
        private readonly Dictionary<string, SpriteSheet> _spriteSheetCache;
        private readonly Dictionary<string, ISoundEffect> _soundEffectCache;
        private readonly Dictionary<string, IMusic> _musicCache;
        private readonly string _contentRoot;

        public AssetManager(IContentLoader contentLoader, string contentRoot = "Content")
        {
            _contentLoader = contentLoader;
            _contentRoot = contentRoot;
            _textureCache = new Dictionary<string, ITexture>();
            _spriteSheetCache = new Dictionary<string, SpriteSheet>();
            _soundEffectCache = new Dictionary<string, ISoundEffect>();
            _musicCache = new Dictionary<string, IMusic>();
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

        // ===== SPRITE SHEET METHODS =====

        /// <summary>
        /// Load a sprite sheet with its configuration (expects .png and .json files)
        /// </summary>
        /// <param name="path">Path to the sprite sheet (without extension)</param>
        /// <returns>The loaded sprite sheet</returns>
        public SpriteSheet LoadSpriteSheet(string path)
        {
            // Check if already cached
            if (_spriteSheetCache.TryGetValue(path, out var cachedSheet))
            {
                return cachedSheet;
            }

            // Load texture
            var texture = LoadTexture(path);

            // Load config from JSON
            var configPath = GetConfigPath(path);
            var config = LoadSpriteSheetConfig(configPath);

            // Create and cache sprite sheet
            var spriteSheet = new SpriteSheet(texture, config);
            _spriteSheetCache[path] = spriteSheet;

            return spriteSheet;
        }

        /// <summary>
        /// Load a sprite sheet configuration from a JSON file
        /// </summary>
        /// <param name="configPath">Path to the JSON config file</param>
        /// <returns>The loaded configuration</returns>
        public SpriteSheetConfig LoadSpriteSheetConfig(string configPath)
        {
            string fullPath = configPath;
            if (!Path.IsPathRooted(configPath))
            {
                fullPath = Path.Combine(_contentRoot, configPath);
            }

            if (!Path.HasExtension(fullPath))
            {
                fullPath += ".json";
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Sprite sheet config file not found: {fullPath}");
            }

            var json = File.ReadAllText(fullPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<SpriteSheetConfig>(json, options);

            if (config == null)
            {
                throw new InvalidOperationException($"Failed to deserialize sprite sheet config from: {fullPath}");
            }

            return config;
        }

        /// <summary>
        /// Save a sprite sheet configuration to a JSON file
        /// </summary>
        /// <param name="config">The configuration to save</param>
        /// <param name="configPath">Path where to save the config</param>
        public void SaveSpriteSheetConfig(SpriteSheetConfig config, string configPath)
        {
            string fullPath = configPath;
            if (!Path.IsPathRooted(configPath))
            {
                fullPath = Path.Combine(_contentRoot, configPath);
            }

            if (!Path.HasExtension(fullPath))
            {
                fullPath += ".json";
            }

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(fullPath, json);
        }

        /// <summary>
        /// Get a previously loaded sprite sheet from cache
        /// </summary>
        /// <param name="path">Path to the sprite sheet</param>
        /// <returns>The cached sprite sheet, or null if not loaded</returns>
        public SpriteSheet? GetSpriteSheet(string path)
        {
            _spriteSheetCache.TryGetValue(path, out var sheet);
            return sheet;
        }

        /// <summary>
        /// Check if a sprite sheet is already loaded in cache
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the sprite sheet is cached</returns>
        public bool IsSpriteSheetLoaded(string path)
        {
            return _spriteSheetCache.ContainsKey(path);
        }

        /// <summary>
        /// Unload a specific sprite sheet from cache
        /// </summary>
        /// <param name="path">Path to the sprite sheet to unload</param>
        /// <returns>True if the sprite sheet was unloaded, false if it wasn't cached</returns>
        public bool UnloadSpriteSheet(string path)
        {
            return _spriteSheetCache.Remove(path);
        }

        /// <summary>
        /// Unload all cached sprite sheets
        /// </summary>
        public void UnloadAllSpriteSheets()
        {
            _spriteSheetCache.Clear();
        }

        // ===== SOUND EFFECT METHODS =====

        /// <summary>
        /// Load a sound effect, or return cached version if already loaded
        /// </summary>
        /// <param name="path">Path to the sound file</param>
        /// <returns>The loaded sound effect</returns>
        public ISoundEffect LoadSoundEffect(string path)
        {
            if (_soundEffectCache.TryGetValue(path, out var cachedSound))
                return cachedSound;

            var sound = _contentLoader.LoadSoundEffect(path);
            _soundEffectCache[path] = sound;
            return sound;
        }

        /// <summary>
        /// Get a previously loaded sound effect from cache
        /// </summary>
        /// <param name="path">Path to the sound effect</param>
        /// <returns>The cached sound effect, or null if not loaded</returns>
        public ISoundEffect? GetSoundEffect(string path)
        {
            _soundEffectCache.TryGetValue(path, out var sound);
            return sound;
        }

        /// <summary>
        /// Preload multiple sound effects at once
        /// </summary>
        /// <param name="paths">Array of sound effect paths to preload</param>
        public void PreloadSoundEffects(params string[] paths)
        {
            foreach (var path in paths)
                LoadSoundEffect(path);
        }

        /// <summary>
        /// Unload a specific sound effect from cache
        /// </summary>
        /// <param name="path">Path to the sound effect to unload</param>
        /// <returns>True if the sound effect was unloaded, false if it wasn't cached</returns>
        public bool UnloadSoundEffect(string path)
        {
            if (_soundEffectCache.TryGetValue(path, out var sound))
            {
                sound?.Dispose();
                return _soundEffectCache.Remove(path);
            }
            return false;
        }

        /// <summary>
        /// Unload all cached sound effects
        /// </summary>
        public void UnloadAllSoundEffects()
        {
            foreach (var sound in _soundEffectCache.Values)
                sound?.Dispose();
            _soundEffectCache.Clear();
        }

        // ===== MUSIC METHODS =====

        /// <summary>
        /// Load a music track, or return cached version if already loaded
        /// </summary>
        /// <param name="path">Path to the music file</param>
        /// <returns>The loaded music track</returns>
        public IMusic LoadMusic(string path)
        {
            if (_musicCache.TryGetValue(path, out var cachedMusic))
                return cachedMusic;

            var music = _contentLoader.LoadMusic(path);
            _musicCache[path] = music;
            return music;
        }

        /// <summary>
        /// Get a previously loaded music track from cache
        /// </summary>
        /// <param name="path">Path to the music track</param>
        /// <returns>The cached music track, or null if not loaded</returns>
        public IMusic? GetMusic(string path)
        {
            _musicCache.TryGetValue(path, out var music);
            return music;
        }

        /// <summary>
        /// Preload multiple music tracks at once
        /// </summary>
        /// <param name="paths">Array of music paths to preload</param>
        public void PreloadMusic(params string[] paths)
        {
            foreach (var path in paths)
                LoadMusic(path);
        }

        /// <summary>
        /// Unload a specific music track from cache
        /// </summary>
        /// <param name="path">Path to the music track to unload</param>
        /// <returns>True if the music track was unloaded, false if it wasn't cached</returns>
        public bool UnloadMusic(string path)
        {
            if (_musicCache.TryGetValue(path, out var music))
            {
                music?.Dispose();
                return _musicCache.Remove(path);
            }
            return false;
        }

        /// <summary>
        /// Unload all cached music tracks
        /// </summary>
        public void UnloadAllMusic()
        {
            foreach (var music in _musicCache.Values)
                music?.Dispose();
            _musicCache.Clear();
        }

        /// <summary>
        /// Unload all assets (textures, sprite sheets, and audio)
        /// </summary>
        public void UnloadAll()
        {
            UnloadAllTextures();
            UnloadAllSpriteSheets();
            UnloadAllSoundEffects();
            UnloadAllMusic();
        }

        /// <summary>
        /// Get count of currently cached sprite sheets
        /// </summary>
        public int CachedSpriteSheetCount => _spriteSheetCache.Count;

        /// <summary>
        /// Get all currently cached sprite sheet paths
        /// </summary>
        public IEnumerable<string> GetCachedSpriteSheetPaths()
        {
            return _spriteSheetCache.Keys;
        }

        /// <summary>
        /// Get count of currently cached sound effects
        /// </summary>
        public int CachedSoundEffectCount => _soundEffectCache.Count;

        /// <summary>
        /// Get count of currently cached music tracks
        /// </summary>
        public int CachedMusicCount => _musicCache.Count;

        private string GetConfigPath(string assetPath)
        {
            // Remove extension if present
            var pathWithoutExt = Path.GetFileNameWithoutExtension(assetPath);
            var directory = Path.GetDirectoryName(assetPath);

            if (string.IsNullOrEmpty(directory))
            {
                return pathWithoutExt + ".json";
            }

            return Path.Combine(directory, pathWithoutExt + ".json");
        }
    }
}
