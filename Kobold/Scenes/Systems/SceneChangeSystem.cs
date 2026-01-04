using System.Collections.Generic;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Events;
using Kobold.Core.Scenes.Events;

namespace Kobold.Core.Scenes.Systems;

/// <summary>
/// System that listens for scene change requests and delegates to SceneManager.
/// Should be registered with SystemGroup.Always so it works in all scenes.
/// </summary>
public class SceneChangeSystem : ISystem
{
    private readonly SceneManager _sceneManager;
    private readonly EventBus _eventBus;
    private readonly Queue<RequestSceneChangeEvent> _pendingChanges = new();

    /// <summary>
    /// Creates a new scene change system.
    /// </summary>
    /// <param name="sceneManager">The scene manager to use for scene changes.</param>
    /// <param name="eventBus">The event bus to subscribe to.</param>
    public SceneChangeSystem(SceneManager sceneManager, EventBus eventBus)
    {
        _sceneManager = sceneManager;
        _eventBus = eventBus;

        // Subscribe to scene change requests
        _eventBus.Subscribe<RequestSceneChangeEvent>(evt => _pendingChanges.Enqueue(evt));
    }

    /// <summary>
    /// Updates the system, processing one pending scene change per frame.
    /// </summary>
    /// <param name="deltaTime">Time since last frame in seconds.</param>
    public void Update(float deltaTime)
    {
        // Process one scene change per frame to avoid conflicts
        if (_pendingChanges.Count > 0)
        {
            var request = _pendingChanges.Dequeue();
            _sceneManager.LoadScene(request.TargetSceneName, request.Parameters, request.Transition);
        }
    }
}
