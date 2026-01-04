using System.Collections.Generic;
using Kobold.Core.Events;

namespace Kobold.Core.Scenes.Events;

/// <summary>
/// Published when a scene is about to be loaded.
/// </summary>
public class SceneLoadingEvent : BaseEvent
{
    /// <summary>
    /// The name of the scene being loaded.
    /// </summary>
    public string SceneName { get; }

    /// <summary>
    /// Parameters being passed to the scene.
    /// </summary>
    public Dictionary<string, object> Parameters { get; }

    public SceneLoadingEvent(string sceneName, Dictionary<string, object> parameters = null)
    {
        SceneName = sceneName;
        Parameters = parameters ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Published when a scene has finished loading.
/// </summary>
public class SceneLoadedEvent : BaseEvent
{
    /// <summary>
    /// The name of the scene that was loaded.
    /// </summary>
    public string SceneName { get; }

    /// <summary>
    /// The scene instance that was loaded.
    /// </summary>
    public IScene Scene { get; }

    public SceneLoadedEvent(string sceneName, IScene scene)
    {
        SceneName = sceneName;
        Scene = scene;
    }
}

/// <summary>
/// Published when a scene is about to be unloaded.
/// </summary>
public class SceneUnloadingEvent : BaseEvent
{
    /// <summary>
    /// The name of the scene being unloaded.
    /// </summary>
    public string SceneName { get; }

    /// <summary>
    /// The scene instance being unloaded.
    /// </summary>
    public IScene Scene { get; }

    public SceneUnloadingEvent(string sceneName, IScene scene)
    {
        SceneName = sceneName;
        Scene = scene;
    }
}

/// <summary>
/// Published when a scene has been unloaded.
/// </summary>
public class SceneUnloadedEvent : BaseEvent
{
    /// <summary>
    /// The name of the scene that was unloaded.
    /// </summary>
    public string SceneName { get; }

    public SceneUnloadedEvent(string sceneName)
    {
        SceneName = sceneName;
    }
}

/// <summary>
/// Published when a scene transition starts (with optional transition effect).
/// </summary>
public class SceneTransitionStartedEvent : BaseEvent
{
    /// <summary>
    /// The name of the scene being transitioned from.
    /// </summary>
    public string FromScene { get; }

    /// <summary>
    /// The name of the scene being transitioned to.
    /// </summary>
    public string ToScene { get; }

    /// <summary>
    /// The type of transition being performed.
    /// </summary>
    public SceneTransition TransitionType { get; }

    public SceneTransitionStartedEvent(string fromScene, string toScene, SceneTransition transitionType)
    {
        FromScene = fromScene;
        ToScene = toScene;
        TransitionType = transitionType;
    }
}

/// <summary>
/// Published when a scene transition completes.
/// </summary>
public class SceneTransitionCompletedEvent : BaseEvent
{
    /// <summary>
    /// The name of the scene that is now active after the transition.
    /// </summary>
    public string SceneName { get; }

    public SceneTransitionCompletedEvent(string sceneName)
    {
        SceneName = sceneName;
    }
}

/// <summary>
/// Request a scene change (systems can publish this, then a scene change system handles it).
/// </summary>
public class RequestSceneChangeEvent : BaseEvent
{
    /// <summary>
    /// The name of the target scene to load.
    /// </summary>
    public string TargetSceneName { get; }

    /// <summary>
    /// Parameters to pass to the target scene.
    /// </summary>
    public Dictionary<string, object> Parameters { get; }

    /// <summary>
    /// The type of transition to use.
    /// </summary>
    public SceneTransition Transition { get; }

    public RequestSceneChangeEvent(
        string targetSceneName,
        Dictionary<string, object> parameters = null,
        SceneTransition transition = SceneTransition.Fade)
    {
        TargetSceneName = targetSceneName;
        Parameters = parameters ?? new Dictionary<string, object>();
        Transition = transition;
    }
}
