using Arch.Core;
using Asteroids.Components;
using Kobold.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Kobold.Extensions.Physics.Components;
namespace Asteroids.Systems
{
    /// <summary>
    /// Handles ship movement, rotation, and thrust input for Asteroids
    /// </summary>
    public class ShipControlSystem : ISystem
    {
        private readonly World _world;
        private readonly IInputManager _inputManager;
        private readonly ShipControlConfig _config;

        public ShipControlSystem(World world, IInputManager inputManager, ShipControlConfig config = null)
        {
            _world = world;
            _inputManager = inputManager;
            _config = config ?? new ShipControlConfig();
        }

        public void Update(float deltaTime)
        {
            var shipQuery = new QueryDescription().WithAll<Ship, Transform, Velocity, AngularVelocity, Thruster>();

            _world.Query(in shipQuery, (Entity entity, ref Ship ship, ref Transform transform, ref Velocity velocity, ref AngularVelocity angularVelocity, ref Thruster thruster) =>
            {
                // Handle rotation input
                HandleRotationInput(ref angularVelocity, ship.RotationSpeed, deltaTime);

                // Handle thrust input
                HandleThrustInput(ref thruster, ship.ThrustPower);

                // Apply speed limiting if ship has MaxSpeed component
                if (_world.Has<MaxSpeed>(entity))
                {
                    ref var maxSpeed = ref _world.Get<MaxSpeed>(entity);
                    LimitSpeed(ref velocity, maxSpeed.Value);
                }

                // Update thruster effect visibility if ship has one
                if (_world.Has<ThrusterEffect>(entity))
                {
                    ref var thrusterEffect = ref _world.Get<ThrusterEffect>(entity);
                    thrusterEffect.IsVisible = thruster.IsActive;
                }
            });
        }

        private void HandleRotationInput(ref AngularVelocity angularVelocity, float rotationSpeed, float deltaTime)
        {
            float rotationInput = 0f;

            // Check rotation input keys
            if (_inputManager.IsKeyDown(_config.RotateLeftKey) || _inputManager.IsKeyDown(_config.AlternateRotateLeftKey))
            {
                rotationInput -= 1f;
            }
            if (_inputManager.IsKeyDown(_config.RotateRightKey) || _inputManager.IsKeyDown(_config.AlternateRotateRightKey))
            {
                rotationInput += 1f;
            }

            // Apply rotation based on input
            if (_config.UseInstantRotation)
            {
                // Direct angular velocity control (classic Asteroids feel)
                angularVelocity.Value = rotationInput * rotationSpeed;
            }
            else
            {
                // Acceleration-based rotation (more realistic)
                float targetAngularVelocity = rotationInput * rotationSpeed;
                float rotationAcceleration = _config.RotationAcceleration;

                angularVelocity.Value = MathUtils.MoveTowards(
                    angularVelocity.Value,
                    targetAngularVelocity,
                    rotationAcceleration * deltaTime
                );
            }
        }

        private void HandleThrustInput(ref Thruster thruster, float thrustPower)
        {
            // Check thrust input
            bool thrustPressed = _inputManager.IsKeyDown(_config.ThrustKey) || _inputManager.IsKeyDown(_config.AlternateThrustKey);

            thruster.IsActive = thrustPressed;
            thruster.Power = thrustPressed ? thrustPower : 0f;
        }

        private void LimitSpeed(ref Velocity velocity, float maxSpeed)
        {
            float currentSpeed = velocity.Value.Length();
            if (currentSpeed > maxSpeed)
            {
                velocity.Value = Vector2.Normalize(velocity.Value) * maxSpeed;
            }
        }
    }

    /// <summary>
    /// Configuration for ship controls and behavior
    /// </summary>
    public class ShipControlConfig
    {
        // Input key mappings
        public KeyCode RotateLeftKey { get; set; } = KeyCode.Left;
        public KeyCode RotateRightKey { get; set; } = KeyCode.Right;
        public KeyCode ThrustKey { get; set; } = KeyCode.Up;
        public KeyCode AlternateRotateLeftKey { get; set; } = KeyCode.A;
        public KeyCode AlternateRotateRightKey { get; set; } = KeyCode.D;
        public KeyCode AlternateThrustKey { get; set; } = KeyCode.W;

        // Control behavior
        public bool UseInstantRotation { get; set; } = true; // Classic Asteroids style
        public float RotationAcceleration { get; set; } = 720f; // degrees/second² (only used if !UseInstantRotation)

        // Debug options
        public bool ShowDebugInfo { get; set; } = false;
    }

}