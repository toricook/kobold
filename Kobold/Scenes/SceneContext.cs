using System.Collections.Generic;
using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Events;
using Kobold.Core.Services;

namespace Kobold.Core.Scenes;

/// <summary>
/// Context provided to scenes during load/unload operations.
/// Contains references to all engine services and utilities.
/// </summary>
public class SceneContext
{
    /// <summary>
    /// The ECS world for creating and managing entities.
    /// </summary>
    public World World { get; }

    /// <summary>
    /// The asset manager for loading textures, sprites, and other resources.
    /// </summary>
    public AssetManager Assets { get; }

    /// <summary>
    /// The audio manager for playing sounds and music.
    /// </summary>
    public AudioManager Audio { get; }

    /// <summary>
    /// The event bus for publishing and subscribing to events.
    /// </summary>
    public EventBus EventBus { get; }

    /// <summary>
    /// The system manager for managing game systems.
    /// </summary>
    public SystemManager SystemManager { get; }

    /// <summary>
    /// The renderer for drawing graphics.
    /// </summary>
    public IRenderer Renderer { get; }

    /// <summary>
    /// The input manager for handling player input.
    /// </summary>
    public IInputManager InputManager { get; }

    /// <summary>
    /// Parameters passed to this scene (e.g., level number, difficulty).
    /// </summary>
    public Dictionary<string, object> Parameters { get; }

    /// <summary>
    /// Scene-specific resource tracker for automatic cleanup.
    /// </summary>
    public SceneResourceTracker Resources { get; }

    /// <summary>
    /// Creates a new scene context with all required services.
    /// </summary>
    public SceneContext(
        World world,
        AssetManager assets,
        AudioManager audio,
        EventBus eventBus,
        SystemManager systemManager,
        IRenderer renderer,
        IInputManager inputManager,
        Dictionary<string, object> parameters = null)
    {
        World = world;
        Assets = assets;
        Audio = audio;
        EventBus = eventBus;
        SystemManager = systemManager;
        Renderer = renderer;
        InputManager = inputManager;
        Parameters = parameters ?? new Dictionary<string, object>();
        Resources = new SceneResourceTracker(assets);
    }

    /// <summary>
    /// Get a typed parameter value with a default fallback.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="key">The parameter key.</param>
    /// <param name="defaultValue">The default value if the parameter is not found or has the wrong type.</param>
    /// <returns>The parameter value or the default value.</returns>
    public T GetParameter<T>(string key, T defaultValue = default)
    {
        if (Parameters.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    /// <summary>
    /// Set a parameter value.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    public void SetParameter(string key, object value)
    {
        Parameters[key] = value;
    }
}
