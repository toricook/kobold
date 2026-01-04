using Kobold.Core.Services;

namespace Kobold.Core.Scenes;

/// <summary>
/// Represents a game scene with its own entities, resources, and lifecycle.
/// Scenes are managed by the SceneManager service.
/// </summary>
public interface IScene
{
    /// <summary>
    /// Unique identifier for this scene (e.g., "MainMenu", "Level1", "GameOver").
    /// </summary>
    string SceneName { get; }

    /// <summary>
    /// The system group that should be active when this scene is running.
    /// Determines which systems execute during this scene.
    /// </summary>
    SystemGroup ActiveSystemGroup { get; }

    /// <summary>
    /// Whether this scene allows other scenes to be stacked on top of it.
    /// Set to false for exclusive scenes like main menu, true for gameplay that can be paused.
    /// </summary>
    bool AllowsOverlay { get; }

    /// <summary>
    /// Whether this is an overlay scene (like pause menu) that renders on top of another scene.
    /// Overlay scenes don't unload the scene beneath them.
    /// </summary>
    bool IsOverlay { get; }

    /// <summary>
    /// Called when the scene is loaded. Create entities, load assets, set up the scene.
    /// </summary>
    /// <param name="context">Scene loading context with services and utilities.</param>
    void Load(SceneContext context);

    /// <summary>
    /// Called when the scene is about to be unloaded. Clean up resources, save state.
    /// </summary>
    /// <param name="context">Scene unloading context.</param>
    void Unload(SceneContext context);

    /// <summary>
    /// Called every frame while the scene is active. Optional - most logic should be in systems.
    /// Use for scene-specific coordination that doesn't fit in a system.
    /// </summary>
    /// <param name="deltaTime">Time since last frame in seconds.</param>
    void Update(float deltaTime);

    /// <summary>
    /// Called when this scene is paused (another scene is pushed on top).
    /// Optional - implement if scene needs to react to being paused.
    /// </summary>
    void OnPause();

    /// <summary>
    /// Called when this scene is resumed (overlay scene was popped).
    /// Optional - implement if scene needs to react to being resumed.
    /// </summary>
    void OnResume();
}
