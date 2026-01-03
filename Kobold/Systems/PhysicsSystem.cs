using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    /// <summary>
    /// Handles basic physics simulation including position, velocity, and acceleration
    /// </summary>
    public class PhysicsSystem : ISystem
    {
        private readonly World _world;
        private readonly PhysicsConfig _config;

        public PhysicsSystem(World world, PhysicsConfig config = null)
        {
            _world = world;
            _config = config ?? new PhysicsConfig();
        }

        public void Update(float deltaTime)
        {
            UpdateKinematics(deltaTime);

            if (_config.EnableGravity)
            {
                ApplyGravity(deltaTime);
            }

            if (_config.EnableDamping)
            {
                ApplyDamping(deltaTime);
            }
        }

        /// <summary>
        /// Updates position based on velocity (basic kinematic physics)
        /// </summary>
        private void UpdateKinematics(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Transform, Velocity>();

            _world.Query(in query, (ref Transform transform, ref Velocity velocity) =>
            {
                transform.Position += velocity.Value * deltaTime;
            });
        }

        /// <summary>
        /// Updates velocity based on acceleration and applies gravity
        /// </summary>
        private void ApplyGravity(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Velocity>().WithAny<Physics, Physics>();

            _world.Query(in query, (Entity entity, ref Velocity velocity) =>
            {
                // Check if entity has custom gravity
                if (_world.Has<Gravity>(entity))
                {
                    ref var gravity = ref _world.Get<Gravity>(entity);
                    velocity.Value += gravity.Force * deltaTime;
                }
                // Otherwise use global gravity if entity has Physics component
                else if (_world.Has<Physics>(entity))
                {
                    velocity.Value += _config.GlobalGravity * deltaTime;
                }
            });
        }

        /// <summary>
        /// Applies damping/friction to slow down moving objects
        /// </summary>
        private void ApplyDamping(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Velocity>();

            _world.Query(in query, (Entity entity, ref Velocity velocity) =>
            {
                float damping = _config.DefaultDamping;

                // Check for custom damping
                if (_world.Has<Physics>(entity))
                {
                    ref var physics = ref _world.Get<Physics>(entity);
                    damping = physics.Damping;
                }

                // Apply damping
                velocity.Value *= (1.0f - damping * deltaTime);

                // Stop very small velocities to prevent jittering
                if (velocity.Value.LengthSquared() < _config.MinVelocityThreshold)
                {
                    velocity.Value = Vector2.Zero;
                }
            });
        }
    }

    /// <summary>
    /// Configuration for physics simulation
    /// </summary>
    public class PhysicsConfig
    {
        public bool EnableGravity { get; set; } = false;
        public bool EnableDamping { get; set; } = false;
        public bool EnableRotationalPhysics { get; set; } = false;
        public bool EnableThrust { get; set; } = false;
        public bool EnableSpeedLimits { get; set; } = false;
        public Vector2 GlobalGravity { get; set; } = new Vector2(0, 981f); // 9.81m/s² * 100 for pixels
        public float DefaultDamping { get; set; } = 0.1f;
        public float DefaultLinearDamping { get; set; } = 0.1f;
        public float DefaultAngularDamping { get; set; } = 0.1f;
        public float MinVelocityThreshold { get; set; } = 0.01f;
        public bool UseMultiplicativeDamping { get; set; } = false;
    }
}

