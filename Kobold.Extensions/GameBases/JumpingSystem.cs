using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components;
using System.Numerics;

namespace Kobold.Extensions.GameBases
{
    /// <summary>
    /// System that handles jumping for platformer games.
    /// Detects when the player is grounded and allows jumping with a configurable jump force.
    /// </summary>
    public class JumpingSystem : ISystem
    {
        private readonly World _world;
        private readonly IInputManager _inputManager;
        private readonly JumpingConfig _config;

        public JumpingSystem(World world, IInputManager inputManager, JumpingConfig? config = null)
        {
            _world = world;
            _inputManager = inputManager;
            _config = config ?? new JumpingConfig();
        }

        public void Update(float deltaTime)
        {
            // Query for entities with PlayerControlled, Velocity, and Player tag
            var query = new QueryDescription().WithAll<PlayerControlled, Velocity, Player>();

            _world.Query(in query, (ref PlayerControlled playerControlled, ref Velocity velocity) =>
            {
                // Only handle jumping for horizontal-only controls (platformers)
                if (!playerControlled.HorizontalOnly)
                    return;

                // Check if jump key is pressed (Space or W)
                bool jumpPressed = _inputManager.IsKeyPressed(_config.JumpKey) ||
                                 _inputManager.IsKeyPressed(_config.AlternateJumpKey);

                if (jumpPressed)
                {
                    // Debug: log jump attempt
                    bool isGrounded = Math.Abs(velocity.Value.Y) < _config.GroundedThreshold;
                    System.Console.WriteLine($"Jump key pressed! Velocity.Y={velocity.Value.Y:F2}, Threshold={_config.GroundedThreshold}, Grounded={isGrounded}");

                    if (isGrounded)
                    {
                        // Apply jump force (negative Y = upward)
                        velocity.Value = new Vector2(velocity.Value.X, -_config.JumpForce);
                        System.Console.WriteLine($"JUMP! Applied force={_config.JumpForce}");
                    }
                }
            });
        }
    }

    /// <summary>
    /// Configuration for jumping behavior in platformers.
    /// </summary>
    public class JumpingConfig
    {
        /// <summary>
        /// The upward force applied when jumping (in pixels/second).
        /// Typical values: 400-600 for reasonable jump heights.
        /// </summary>
        public float JumpForce { get; set; } = 500f;

        /// <summary>
        /// The velocity threshold for considering the player "grounded".
        /// If abs(velocity.Y) is less than this, the player can jump.
        /// </summary>
        public float GroundedThreshold { get; set; } = 10f;

        /// <summary>
        /// Primary jump key.
        /// </summary>
        public KeyCode JumpKey { get; set; } = KeyCode.Space;

        /// <summary>
        /// Alternate jump key (commonly W for WASD controls).
        /// </summary>
        public KeyCode AlternateJumpKey { get; set; } = KeyCode.W;
    }
}
