using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using System.Numerics;

namespace Pong.Systems
{
    public class MovementSystem : ISystem
    {
        private readonly World _world;

        public MovementSystem(World world)
        {
            _world = world;
        }

        public void Update(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Transform, Velocity>();

            _world.Query(in query, (ref Transform transform, ref Velocity velocity) =>
            {
                transform.Position += velocity.Value * deltaTime;
            });
        }
    }
}