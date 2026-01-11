using Arch.Core;

namespace Kobold.Extensions.CameraFollow.Components;

/// <summary>
/// Component that marks which entity the camera should follow
/// </summary>
public struct CameraFollowTarget
{
    public Entity Target;

    public CameraFollowTarget(Entity target)
    {
        Target = target;
    }
}
