using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Events;
using Kobold.Core.Scenes.Events;
using Kobold.Core.Services;

namespace Kobold.Core.Scenes;

/// <summary>
/// Manages scene lifecycle, transitions, and the scene stack.
/// Integrates with AssetManager, SystemManager, and EventBus.
/// </summary>
public class SceneManager
{
    private readonly World _world;
    private readonly AssetManager _assetManager;
    private readonly AudioManager _audioManager;
    private readonly EventBus _eventBus;
    private readonly SystemManager _systemManager;
    private readonly IRenderer _renderer;
    private readonly IInputManager _inputManager;

    private readonly Stack<SceneStackFrame> _sceneStack = new();
    private readonly Dictionary<string, IScene> _registeredScenes = new();

    private SceneStackFrame _currentScene = null;
    private SceneTransitionState _transitionState = null;

    /// <summary>
    /// The currently active scene (top of stack).
    /// </summary>
    public IScene CurrentScene => _currentScene?.Scene;

    /// <summary>
    /// Name of the current scene.
    /// </summary>
    public string CurrentSceneName => _currentScene?.Scene?.SceneName;

    /// <summary>
    /// Whether a scene is currently loaded.
    /// </summary>
    public bool HasActiveScene => _currentScene != null;

    /// <summary>
    /// Whether a transition is in progress.
    /// </summary>
    public bool IsTransitioning => _transitionState != null;

    /// <summary>
    /// Number of scenes in the stack.
    /// </summary>
    public int SceneStackDepth => _sceneStack.Count;

    /// <summary>
    /// Creates a new scene manager.
    /// </summary>
    public SceneManager(
        World world,
        AssetManager assetManager,
        AudioManager audioManager,
        EventBus eventBus,
        SystemManager systemManager,
        IRenderer renderer,
        IInputManager inputManager)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _assetManager = assetManager ?? throw new ArgumentNullException(nameof(assetManager));
        _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _systemManager = systemManager ?? throw new ArgumentNullException(nameof(systemManager));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
    }

    /// <summary>
    /// Register a scene type for loading by name.
    /// </summary>
    /// <param name="scene">The scene to register.</param>
    public void RegisterScene(IScene scene)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        if (_registeredScenes.ContainsKey(scene.SceneName))
            throw new InvalidOperationException($"Scene '{scene.SceneName}' is already registered");

        _registeredScenes[scene.SceneName] = scene;
    }

    /// <summary>
    /// Register multiple scenes at once.
    /// </summary>
    /// <param name="scenes">The scenes to register.</param>
    public void RegisterScenes(params IScene[] scenes)
    {
        foreach (var scene in scenes)
            RegisterScene(scene);
    }

    /// <summary>
    /// Load a scene, replacing the current scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    /// <param name="parameters">Parameters to pass to the scene.</param>
    /// <param name="transition">The transition type to use.</param>
    public void LoadScene(string sceneName, Dictionary<string, object> parameters = null, SceneTransition transition = SceneTransition.Immediate)
    {
        if (!_registeredScenes.TryGetValue(sceneName, out var scene))
            throw new InvalidOperationException($"Scene '{sceneName}' is not registered. Call RegisterScene first.");

        LoadSceneInternal(scene, parameters, transition, replaceAll: true);
    }

    /// <summary>
    /// Load a scene instance directly.
    /// </summary>
    /// <param name="scene">The scene to load.</param>
    /// <param name="parameters">Parameters to pass to the scene.</param>
    /// <param name="transition">The transition type to use.</param>
    public void LoadScene(IScene scene, Dictionary<string, object> parameters = null, SceneTransition transition = SceneTransition.Immediate)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        LoadSceneInternal(scene, parameters, transition, replaceAll: true);
    }

    /// <summary>
    /// Push a scene onto the stack (for overlays like pause menus).
    /// </summary>
    /// <param name="sceneName">The name of the scene to push.</param>
    /// <param name="parameters">Parameters to pass to the scene.</param>
    public void PushScene(string sceneName, Dictionary<string, object> parameters = null)
    {
        if (!_registeredScenes.TryGetValue(sceneName, out var scene))
            throw new InvalidOperationException($"Scene '{sceneName}' is not registered");

        if (!scene.IsOverlay)
            throw new InvalidOperationException($"Scene '{sceneName}' is not marked as an overlay scene");

        if (_currentScene != null && !_currentScene.Scene.AllowsOverlay)
            throw new InvalidOperationException($"Current scene '{_currentScene.Scene.SceneName}' does not allow overlays");

        PushSceneInternal(scene, parameters);
    }

    /// <summary>
    /// Push a scene instance onto the stack.
    /// </summary>
    /// <param name="scene">The scene to push.</param>
    /// <param name="parameters">Parameters to pass to the scene.</param>
    public void PushScene(IScene scene, Dictionary<string, object> parameters = null)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        if (!scene.IsOverlay)
            throw new InvalidOperationException($"Scene '{scene.SceneName}' is not marked as an overlay scene");

        if (_currentScene != null && !_currentScene.Scene.AllowsOverlay)
            throw new InvalidOperationException($"Current scene '{_currentScene.Scene.SceneName}' does not allow overlays");

        PushSceneInternal(scene, parameters);
    }

    /// <summary>
    /// Pop the current scene from the stack, returning to the previous scene.
    /// </summary>
    public void PopScene()
    {
        if (_sceneStack.Count == 0)
            throw new InvalidOperationException("Cannot pop scene - scene stack is empty");

        // Current scene is always at top of stack
        var poppingScene = _sceneStack.Pop();

        // Publish unloading event
        _eventBus.Publish(new SceneUnloadingEvent(poppingScene.Scene.SceneName, poppingScene.Scene));

        // Unload the scene
        poppingScene.Scene.Unload(poppingScene.Context);

        // Clean up scene resources
        poppingScene.Context.Resources.UnloadAll();

        // If scene is not an overlay, clear entities (overlays don't own entities in world)
        if (!poppingScene.Scene.IsOverlay)
        {
            _world.Clear();
        }

        // Publish unloaded event
        _eventBus.Publish(new SceneUnloadedEvent(poppingScene.Scene.SceneName));

        // Resume previous scene if exists
        if (_sceneStack.Count > 0)
        {
            _currentScene = _sceneStack.Peek();
            _currentScene.Scene.OnResume();
        }
        else
        {
            _currentScene = null;
        }
    }

    /// <summary>
    /// Update the current scene. Call this from GameEngineBase.Update().
    /// </summary>
    /// <param name="deltaTime">Time since last frame in seconds.</param>
    public void Update(float deltaTime)
    {
        // Update transition if active
        if (_transitionState != null)
        {
            UpdateTransition(deltaTime);
            return;
        }

        // Update current scene
        if (_currentScene != null)
        {
            _currentScene.Scene.Update(deltaTime);
        }
    }

    /// <summary>
    /// Get the active system group for the current scene.
    /// </summary>
    /// <returns>The active system group.</returns>
    public SystemGroup GetActiveSystemGroup()
    {
        if (_currentScene != null)
            return _currentScene.Scene.ActiveSystemGroup;

        return SystemGroup.Always; // Fallback
    }

    /// <summary>
    /// Unload the current scene without loading a new one.
    /// </summary>
    public void UnloadCurrentScene()
    {
        if (_currentScene == null)
            return;

        // Unload all scenes in stack
        while (_sceneStack.Count > 0)
        {
            PopScene();
        }
    }

    /// <summary>
    /// Check if a scene is registered.
    /// </summary>
    /// <param name="sceneName">The name of the scene to check.</param>
    /// <returns>True if the scene is registered, false otherwise.</returns>
    public bool IsSceneRegistered(string sceneName)
    {
        return _registeredScenes.ContainsKey(sceneName);
    }

    /// <summary>
    /// Get all registered scene names.
    /// </summary>
    /// <returns>An enumerable of registered scene names.</returns>
    public IEnumerable<string> GetRegisteredSceneNames()
    {
        return _registeredScenes.Keys;
    }

    // ===== PRIVATE IMPLEMENTATION =====

    private void LoadSceneInternal(IScene scene, Dictionary<string, object> parameters, SceneTransition transition, bool replaceAll)
    {
        // Start transition if not immediate
        if (transition != SceneTransition.Immediate)
        {
            _transitionState = new SceneTransitionState
            {
                TargetScene = scene,
                Parameters = parameters,
                TransitionType = transition,
                Phase = TransitionPhase.FadeOut,
                Progress = 0f,
                Duration = GetTransitionDuration(transition)
            };

            var fromScene = _currentScene?.Scene?.SceneName ?? "None";
            _eventBus.Publish(new SceneTransitionStartedEvent(fromScene, scene.SceneName, transition));
            return;
        }

        // Immediate load
        LoadSceneImmediate(scene, parameters, replaceAll);
    }

    private void LoadSceneImmediate(IScene scene, Dictionary<string, object> parameters, bool replaceAll)
    {
        // Unload current scene(s)
        if (replaceAll)
        {
            UnloadCurrentScene();
        }

        // Publish loading event
        _eventBus.Publish(new SceneLoadingEvent(scene.SceneName, parameters));

        // Create scene context
        var context = new SceneContext(
            _world,
            _assetManager,
            _audioManager,
            _eventBus,
            _systemManager,
            _renderer,
            _inputManager,
            parameters
        );

        // Load the scene
        scene.Load(context);

        // Create stack frame and push
        var frame = new SceneStackFrame(scene, context);
        _sceneStack.Push(frame);
        _currentScene = frame;

        // Publish loaded event
        _eventBus.Publish(new SceneLoadedEvent(scene.SceneName, scene));
    }

    private void PushSceneInternal(IScene scene, Dictionary<string, object> parameters)
    {
        // Pause current scene
        if (_currentScene != null)
        {
            _currentScene.Scene.OnPause();
        }

        // Publish loading event
        _eventBus.Publish(new SceneLoadingEvent(scene.SceneName, parameters));

        // Create scene context
        var context = new SceneContext(
            _world,
            _assetManager,
            _audioManager,
            _eventBus,
            _systemManager,
            _renderer,
            _inputManager,
            parameters
        );

        // Load the overlay scene
        scene.Load(context);

        // Create stack frame and push
        var frame = new SceneStackFrame(scene, context);
        _sceneStack.Push(frame);
        _currentScene = frame;

        // Publish loaded event
        _eventBus.Publish(new SceneLoadedEvent(scene.SceneName, scene));
    }

    private void UpdateTransition(float deltaTime)
    {
        if (_transitionState == null)
            return;

        _transitionState.Progress += deltaTime / _transitionState.Duration;

        if (_transitionState.Phase == TransitionPhase.FadeOut && _transitionState.Progress >= 1.0f)
        {
            // Fade out complete, load new scene
            LoadSceneImmediate(_transitionState.TargetScene, _transitionState.Parameters, replaceAll: true);

            // Switch to fade in
            _transitionState.Phase = TransitionPhase.FadeIn;
            _transitionState.Progress = 0f;
        }
        else if (_transitionState.Phase == TransitionPhase.FadeIn && _transitionState.Progress >= 1.0f)
        {
            // Transition complete
            _eventBus.Publish(new SceneTransitionCompletedEvent(_transitionState.TargetScene.SceneName));
            _transitionState = null;
        }
    }

    private float GetTransitionDuration(SceneTransition transition)
    {
        return transition switch
        {
            SceneTransition.Fade => 0.5f,      // 0.5 seconds
            SceneTransition.Crossfade => 0.3f, // 0.3 seconds
            SceneTransition.Slide => 0.4f,     // 0.4 seconds
            _ => 0f
        };
    }
}
