using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Extensions.CameraFollow.Components;
using System.Numerics;

namespace Kobold.Extensions.CameraFollow.Systems;

/// <summary>
/// System that makes the camera follow its target entity
/// </summary>
public class CameraFollowSystem : ISystem
{
    private readonly World _world;

    public CameraFollowSystem(World world)
    {
        _world = world;
    }

    public void Update(float deltaTime)
    {
        var query = new QueryDescription().WithAll<Core.Components.Camera, CameraFollowTarget>();

        _world.Query(in query, (ref Core.Components.Camera camera, ref CameraFollowTarget followTarget) =>
        {
            // Check if target entity exists and has a Transform
            if (_world.IsAlive(followTarget.Target) && _world.Has<Transform>(followTarget.Target))
            {
                ref var targetTransform = ref _world.Get<Transform>(followTarget.Target);

                if (camera.FollowTarget && camera.SmoothSpeed > 0)
                {
                    // Smooth follow
                    camera.Position = Vector2.Lerp(
                        camera.Position,
                        targetTransform.Position,
                        1.0f - MathF.Pow(0.5f, deltaTime * camera.SmoothSpeed)
                    );
                }
                else if (camera.FollowTarget)
                {
                    // Instant follow
                    camera.Position = targetTransform.Position;
                }

                // Clamp camera to bounds
                camera.Position = Vector2.Clamp(camera.Position, camera.MinBounds, camera.MaxBounds);
            }
        });
    }
}
