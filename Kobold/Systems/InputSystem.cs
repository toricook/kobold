using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Abstractions.Input;
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
    /// Generic input system that handles PlayerControlled entities
    /// Can be used directly or extended for custom input handling
    /// </summary>
    public class InputSystem : ISystem
    {
        protected readonly IInputManager InputManager;
        protected readonly World World;

        public InputSystem(IInputManager inputManager, World world)
        {
            InputManager = inputManager;
            World = world;
        }

        public virtual void Update(float deltaTime)
        {
            // Handle all entities with PlayerControlled component
            HandlePlayerControlledMovement();
        }

        /// <summary>
        /// Handles movement for entities with PlayerControlled component
        /// </summary>
        protected void HandlePlayerControlledMovement()
        {
            var query = new QueryDescription().WithAll<PlayerControlled, Velocity>();

            World.Query(in query, (ref PlayerControlled playerControlled, ref Velocity velocity) =>
            {
                // For platformers (HorizontalOnly), preserve vertical velocity for gravity/jumping
                // For top-down games, reset all velocity
                if (playerControlled.HorizontalOnly)
                {
                    // Keep Y velocity (gravity), reset X
                    velocity.Value = new Vector2(0, velocity.Value.Y);
                }
                else
                {
                    // Reset both components for full directional control
                    velocity.Value = Vector2.Zero;
                }

                // Handle vertical movement
                if (!playerControlled.HorizontalOnly)
                {
                    if (InputManager.IsKeyDown(playerControlled.UpKey) || InputManager.IsKeyDown(playerControlled.AlternateUpKey))
                    {
                        velocity.Value += new Vector2(0, -playerControlled.Speed);
                    }
                    else if (InputManager.IsKeyDown(playerControlled.DownKey) || InputManager.IsKeyDown(playerControlled.AlternateDownKey))
                    {
                        velocity.Value += new Vector2(0, playerControlled.Speed);
                    }
                }

                // Handle horizontal movement
                if (!playerControlled.VerticalOnly)
                {
                    if (InputManager.IsKeyDown(playerControlled.LeftKey) || InputManager.IsKeyDown(playerControlled.AlternateLeftKey))
                    {
                        velocity.Value += new Vector2(-playerControlled.Speed, 0);
                    }
                    else if (InputManager.IsKeyDown(playerControlled.RightKey) || InputManager.IsKeyDown(playerControlled.AlternateRightKey))
                    {
                        velocity.Value += new Vector2(playerControlled.Speed, 0);
                    }
                }
            });
        }

        /// <summary>
        /// Generic movement handler for any component type with custom key bindings
        /// </summary>
        protected void HandleMovement<T>(float speed, KeyCode up, KeyCode down, KeyCode left = KeyCode.Left, KeyCode right = KeyCode.Right, bool verticalOnly = false)
            where T : struct
        {
            var query = new QueryDescription().WithAll<T, Velocity>();

            World.Query(in query, (ref T component, ref Velocity velocity) =>
            {
                velocity.Value = Vector2.Zero;

                if (InputManager.IsKeyDown(up))
                    velocity.Value += new Vector2(0, -speed);
                if (InputManager.IsKeyDown(down))
                    velocity.Value += new Vector2(0, speed);

                if (!verticalOnly)
                {
                    if (InputManager.IsKeyDown(left))
                        velocity.Value += new Vector2(-speed, 0);
                    if (InputManager.IsKeyDown(right))
                        velocity.Value += new Vector2(speed, 0);
                }
            });
        }

        /// <summary>
        /// Checks if any directional key is pressed
        /// </summary>
        protected bool IsAnyDirectionalKeyPressed()
        {
            return InputManager.IsKeyDown(KeyCode.Up) || InputManager.IsKeyDown(KeyCode.Down) ||
                   InputManager.IsKeyDown(KeyCode.Left) || InputManager.IsKeyDown(KeyCode.Right) ||
                   InputManager.IsKeyDown(KeyCode.W) || InputManager.IsKeyDown(KeyCode.S) ||
                   InputManager.IsKeyDown(KeyCode.A) || InputManager.IsKeyDown(KeyCode.D);
        }
    }
}
