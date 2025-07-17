using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Pong.Components;
using System.Numerics;

namespace Pong.Systems
{
    public class InputSystem
    {
        private readonly IInputManager _inputManager;
        private readonly World _world;

        public InputSystem(IInputManager inputManager, World world)
        {
            _inputManager = inputManager;
            _world = world;
        }

        public void Update(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Paddle, Velocity, Transform>();

            _world.Query(in query, (Entity entity, ref Paddle paddle, ref Velocity velocity, ref Transform transform) =>
            {
                if (!paddle.IsPlayer) return;

                velocity.Value = Vector2.Zero;

                if (_inputManager.IsKeyDown(KeyCode.Up) || _inputManager.IsKeyDown(KeyCode.W))
                {
                    velocity.Value = new Vector2(0, -paddle.Speed);
                }
                else if (_inputManager.IsKeyDown(KeyCode.Down) || _inputManager.IsKeyDown(KeyCode.S))
                {
                    velocity.Value = new Vector2(0, paddle.Speed);
                }
            });
        }
    }
}
