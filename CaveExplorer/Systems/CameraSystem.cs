using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using System;
using System.Numerics;

namespace CaveExplorer.Systems
{
    /// <summary>
    /// System that updates the camera to follow the player with smooth movement.
    /// </summary>
    public class CameraSystem : ISystem
    {
        private readonly World _world;

        public CameraSystem(World world)
        {
            _world = world;
        }

        public void Update(float deltaTime)
        {
            // Query for camera and player
            var cameraQuery = new QueryDescription().WithAll<Camera>();
            var playerQuery = new QueryDescription().WithAll<Player, Transform>();

            // Get player position
            Vector2? playerPosition = null;
            _world.Query(in playerQuery, (ref Player player, ref Transform transform) =>
            {
                playerPosition = transform.Position;
            });

            // If no player found, don't update camera
            if (!playerPosition.HasValue)
                return;

            // Update camera to follow player
            _world.Query(in cameraQuery, (Entity entity, ref Camera camera) =>
            {
                if (!camera.FollowTarget)
                    return;

                Vector2 targetPosition = playerPosition.Value;

                // Smooth follow using lerp
                if (camera.SmoothSpeed > 0)
                {
                    float lerpFactor = Math.Min(1f, deltaTime * camera.SmoothSpeed);
                    camera.Position = Vector2.Lerp(camera.Position, targetPosition, lerpFactor);
                }
                else
                {
                    // Instant follow
                    camera.Position = targetPosition;
                }

                // Clamp camera to bounds
                camera.Position = new Vector2(
                    Math.Clamp(camera.Position.X, camera.MinBounds.X, camera.MaxBounds.X),
                    Math.Clamp(camera.Position.Y, camera.MinBounds.Y, camera.MaxBounds.Y)
                );
            });
        }
    }
}
