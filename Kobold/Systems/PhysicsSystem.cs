using Arch.Core;
using Kobold.Core.Abstractions;
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
    /// Enhanced physics system with rotational physics, thrust, and drag
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
            // Apply thrust forces first
            if (_config.EnableThrust)
            {
                ApplyThrust(deltaTime);
            }

            // Update kinematics (position from velocity, rotation from angular velocity)
            UpdateKinematics(deltaTime);

            // Apply rotational physics
            if (_config.EnableRotationalPhysics)
            {
                UpdateRotationalKinematics(deltaTime);
            }

            // Apply gravity if enabled
            if (_config.EnableGravity)
            {
                ApplyGravity(deltaTime);
            }

            // Apply drag/friction
            if (_config.EnableDamping)
            {
                ApplyDamping(deltaTime);
            }

            // Enforce speed limits
            if (_config.EnableSpeedLimits)
            {
                EnforceSpeedLimits();
            }
        }

        /// <summary>
        /// Updates position based on velocity and rotation based on angular velocity
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
        /// Updates rotation based on angular velocity
        /// </summary>
        private void UpdateRotationalKinematics(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Transform, AngularVelocity>();

            _world.Query(in query, (ref Transform transform, ref AngularVelocity angularVelocity) =>
            {
                transform.Rotation += angularVelocity.Value * deltaTime;
            });
        }

        /// <summary>
        /// Apply thrust forces based on entity rotation and thrust input
        /// </summary>
        private void ApplyThrust(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Transform, Velocity, Thruster>();

            _world.Query(in query, (ref Transform transform, ref Velocity velocity, ref Thruster thruster) =>
            {
                if (thruster.IsActive && thruster.Power > 0)
                {
                    // Calculate thrust direction based on entity rotation
                    // Rotation of 0 = pointing right, rotation increases counter-clockwise
                    var thrustDirection = new Vector2(
                        MathF.Cos(transform.Rotation),
                        MathF.Sin(transform.Rotation)
                    );

                    // Apply thrust force
                    var thrustForce = thrustDirection * thruster.Power * deltaTime;
                    velocity.Value += thrustForce;
                }
            });
        }

        /// <summary>
        /// Updates velocity based on acceleration and applies gravity
        /// </summary>
        private void ApplyGravity(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Velocity>().WithAny<Physics, Gravity>();

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
            // Linear damping
            var velocityQuery = new QueryDescription().WithAll<Velocity>();

            _world.Query(in velocityQuery, (Entity entity, ref Velocity velocity) =>
            {
                float damping = _config.DefaultLinearDamping;

                // Check for custom damping
                if (_world.Has<Drag>(entity))
                {
                    ref var drag = ref _world.Get<Drag>(entity);
                    damping = drag.Linear;
                }
                else if (_world.Has<Physics>(entity))
                {
                    ref var physics = ref _world.Get<Physics>(entity);
                    damping = physics.Damping;
                }

                // Apply damping - can be multiplicative or subtractive
                if (_config.UseMultiplicativeDamping)
                {
                    velocity.Value *= MathF.Pow(1.0f - damping, deltaTime);
                }
                else
                {
                    velocity.Value *= (1.0f - damping * deltaTime);
                }

                // Stop very small velocities to prevent jittering
                if (velocity.Value.LengthSquared() < _config.MinVelocityThreshold)
                {
                    velocity.Value = Vector2.Zero;
                }
            });

            // Angular damping
            var angularQuery = new QueryDescription().WithAll<AngularVelocity>();

            _world.Query(in angularQuery, (Entity entity, ref AngularVelocity angularVelocity) =>
            {
                float angularDamping = _config.DefaultAngularDamping;

                // Check for custom angular damping
                if (_world.Has<Drag>(entity))
                {
                    ref var drag = ref _world.Get<Drag>(entity);
                    angularDamping = drag.Angular;
                }

                // Apply angular damping
                if (_config.UseMultiplicativeDamping)
                {
                    angularVelocity.Value *= MathF.Pow(1.0f - angularDamping, deltaTime);
                }
                else
                {
                    angularVelocity.Value *= (1.0f - angularDamping * deltaTime);
                }

                // Stop very small angular velocities
                if (MathF.Abs(angularVelocity.Value) < _config.MinAngularVelocityThreshold)
                {
                    angularVelocity.Value = 0f;
                }
            });
        }

        /// <summary>
        /// Enforce maximum speed limits on entities
        /// </summary>
        private void EnforceSpeedLimits()
        {
            var query = new QueryDescription().WithAll<Velocity, MaxSpeed>();

            _world.Query(in query, (ref Velocity velocity, ref MaxSpeed maxSpeed) =>
            {
                var currentSpeed = velocity.Value.Length();
                if (currentSpeed > maxSpeed.Value)
                {
                    velocity.Value = Vector2.Normalize(velocity.Value) * maxSpeed.Value;
                }
            });
        }

        /// <summary>
        /// Add an impulse (instant velocity change) to an entity
        /// </summary>
        public void AddImpulse(Entity entity, Vector2 impulse)
        {
            if (_world.Has<Velocity>(entity))
            {
                ref var velocity = ref _world.Get<Velocity>(entity);

                // Consider mass if entity has physics component
                if (_world.Has<Physics>(entity))
                {
                    ref var physics = ref _world.Get<Physics>(entity);
                    if (!physics.IsStatic && physics.Mass > 0)
                    {
                        velocity.Value += impulse / physics.Mass;
                    }
                }
                else
                {
                    velocity.Value += impulse;
                }
            }
        }

        /// <summary>
        /// Add angular impulse to an entity
        /// </summary>
        public void AddAngularImpulse(Entity entity, float angularImpulse)
        {
            if (_world.Has<AngularVelocity>(entity))
            {
                ref var angularVelocity = ref _world.Get<AngularVelocity>(entity);
                angularVelocity.Value += angularImpulse;
            }
        }
    }

    /// <summary>
    /// Enhanced configuration for physics simulation
    /// </summary>
    public class PhysicsConfig
    {
        // Feature toggles
        public bool EnableGravity { get; set; } = false;
        public bool EnableDamping { get; set; } = true;
        public bool EnableRotationalPhysics { get; set; } = true;
        public bool EnableThrust { get; set; } = true;
        public bool EnableSpeedLimits { get; set; } = true;

        // Gravity settings
        public Vector2 GlobalGravity { get; set; } = new Vector2(0, 981f); // 9.81m/s² * 100 for pixels

        // Damping settings
        public float DefaultLinearDamping { get; set; } = 0.02f; // Very light damping for space
        public float DefaultAngularDamping { get; set; } = 0.05f;
        public bool UseMultiplicativeDamping { get; set; } = true; // More realistic than linear

        // Thresholds
        public float MinVelocityThreshold { get; set; } = 0.1f;
        public float MinAngularVelocityThreshold { get; set; } = 0.01f; // radians/second
    }
}