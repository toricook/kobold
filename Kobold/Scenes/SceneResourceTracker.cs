using System.Collections.Generic;
using Kobold.Core.Abstractions.Assets;
using Kobold.Core.Abstractions.Audio;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;
using Kobold.Core.Services;

namespace Kobold.Core.Scenes;

/// <summary>
/// Tracks resources loaded by a scene for automatic cleanup.
/// Automatically tracks texture/sound/music loads and unloads them when scene ends.
/// </summary>
public class SceneResourceTracker
{
    private readonly AssetManager _assetManager;
    private readonly HashSet<string> _trackedTextures = new();
    private readonly HashSet<string> _trackedSounds = new();
    private readonly HashSet<string> _trackedMusic = new();
    private readonly HashSet<string> _trackedSpriteSheets = new();

    /// <summary>
    /// Creates a new scene resource tracker.
    /// </summary>
    /// <param name="assetManager">The asset manager to use for loading/unloading.</param>
    public SceneResourceTracker(AssetManager assetManager)
    {
        _assetManager = assetManager;
    }

    /// <summary>
    /// Load and track a texture for this scene.
    /// </summary>
    /// <param name="path">The path to the texture.</param>
    /// <returns>The loaded texture.</returns>
    public ITexture LoadTexture(string path)
    {
        var texture = _assetManager.LoadTexture(path);
        _trackedTextures.Add(path);
        return texture;
    }

    /// <summary>
    /// Load and track a sprite sheet for this scene.
    /// </summary>
    /// <param name="path">The path to the sprite sheet.</param>
    /// <returns>The loaded sprite sheet.</returns>
    public SpriteSheet LoadSpriteSheet(string path)
    {
        var sheet = _assetManager.LoadSpriteSheet(path);
        _trackedSpriteSheets.Add(path);
        return sheet;
    }

    /// <summary>
    /// Load and track a sound effect for this scene.
    /// </summary>
    /// <param name="path">The path to the sound effect.</param>
    /// <returns>The loaded sound effect.</returns>
    public ISoundEffect LoadSoundEffect(string path)
    {
        var sound = _assetManager.LoadSoundEffect(path);
        _trackedSounds.Add(path);
        return sound;
    }

    /// <summary>
    /// Load and track music for this scene.
    /// </summary>
    /// <param name="path">The path to the music.</param>
    /// <returns>The loaded music.</returns>
    public IMusic LoadMusic(string path)
    {
        var music = _assetManager.LoadMusic(path);
        _trackedMusic.Add(path);
        return music;
    }

    /// <summary>
    /// Mark a resource as persistent (won't be unloaded with scene).
    /// </summary>
    /// <param name="path">The path to the resource.</param>
    public void MarkPersistent(string path)
    {
        _trackedTextures.Remove(path);
        _trackedSounds.Remove(path);
        _trackedMusic.Remove(path);
        _trackedSpriteSheets.Remove(path);
    }

    /// <summary>
    /// Unload all tracked resources.
    /// </summary>
    public void UnloadAll()
    {
        foreach (var path in _trackedTextures)
            _assetManager.UnloadTexture(path);

        foreach (var path in _trackedSpriteSheets)
            _assetManager.UnloadSpriteSheet(path);

        foreach (var path in _trackedSounds)
            _assetManager.UnloadSoundEffect(path);

        foreach (var path in _trackedMusic)
            _assetManager.UnloadMusic(path);

        _trackedTextures.Clear();
        _trackedSpriteSheets.Clear();
        _trackedSounds.Clear();
        _trackedMusic.Clear();
    }

    /// <summary>
    /// Get count of tracked resources.
    /// </summary>
    public int TrackedResourceCount =>
        _trackedTextures.Count + _trackedSpriteSheets.Count +
        _trackedSounds.Count + _trackedMusic.Count;
}
