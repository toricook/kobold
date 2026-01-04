using System.Collections.Generic;

namespace Kobold.Core.Scenes;

/// <summary>
/// Types of scene transitions.
/// </summary>
public enum SceneTransition
{
    /// <summary>Immediate switch, no effect.</summary>
    Immediate,

    /// <summary>Fade to black then fade in.</summary>
    Fade,

    /// <summary>Crossfade between scenes (requires rendering both).</summary>
    Crossfade,

    /// <summary>Slide scenes horizontally.</summary>
    Slide,

    /// <summary>Custom transition (implement via event handlers).</summary>
    Custom
}

/// <summary>
/// Internal helper class to track a scene in the scene stack.
/// </summary>
internal class SceneStackFrame
{
    public IScene Scene { get; }
    public SceneContext Context { get; }

    public SceneStackFrame(IScene scene, SceneContext context)
    {
        Scene = scene;
        Context = context;
    }
}

/// <summary>
/// Internal helper class to track the state of an ongoing scene transition.
/// </summary>
internal class SceneTransitionState
{
    public IScene TargetScene { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public SceneTransition TransitionType { get; set; }
    public TransitionPhase Phase { get; set; }
    public float Progress { get; set; }
    public float Duration { get; set; }
}

/// <summary>
/// Internal enum representing the phase of a transition.
/// </summary>
internal enum TransitionPhase
{
    FadeOut,
    FadeIn
}
