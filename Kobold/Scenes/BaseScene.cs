using Kobold.Core.Services;

namespace Kobold.Core.Scenes;

/// <summary>
/// Base implementation of IScene with sensible defaults.
/// Inherit from this to create your scenes more easily.
/// </summary>
public abstract class BaseScene : IScene
{
    /// <summary>
    /// Unique identifier for this scene. Must be overridden.
    /// </summary>
    public abstract string SceneName { get; }

    /// <summary>
    /// Override to specify which system group runs for this scene.
    /// Default: Playing
    /// </summary>
    public virtual SystemGroup ActiveSystemGroup => SystemGroup.Playing;

    /// <summary>
    /// Override to control if overlays can be pushed on this scene.
    /// Default: true (allows pause menus, etc.)
    /// </summary>
    public virtual bool AllowsOverlay => true;

    /// <summary>
    /// Override to mark this as an overlay scene.
    /// Default: false
    /// </summary>
    public virtual bool IsOverlay => false;

    /// <summary>
    /// Implement this to load your scene (create entities, load assets).
    /// </summary>
    /// <param name="context">Scene loading context with services and utilities.</param>
    public abstract void Load(SceneContext context);

    /// <summary>
    /// Implement this to clean up your scene (optional - resources auto-cleanup).
    /// </summary>
    /// <param name="context">Scene unloading context.</param>
    public virtual void Unload(SceneContext context)
    {
        // Default: no custom cleanup needed (resources auto-unload)
    }

    /// <summary>
    /// Override to add per-frame scene logic (most logic should be in systems).
    /// </summary>
    /// <param name="deltaTime">Time since last frame in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        // Default: no per-frame logic
    }

    /// <summary>
    /// Override to react to scene being paused.
    /// </summary>
    public virtual void OnPause()
    {
        // Default: no reaction
    }

    /// <summary>
    /// Override to react to scene being resumed.
    /// </summary>
    public virtual void OnResume()
    {
        // Default: no reaction
    }
}
