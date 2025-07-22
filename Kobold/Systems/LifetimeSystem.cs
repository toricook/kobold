using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    public class LifetimeSystem : ISystem
    {
        private readonly World _world;

        public LifetimeSystem(World world)
        {
            _world = world;
        }

        public void Update(float deltaTime)
        {
            var query = new QueryDescription().WithAll<Lifetime>();
            var entitiesToDestroy = new List<Entity>();

            _world.Query(in query, (Entity entity, ref Lifetime lifetime) =>
            {
                lifetime.RemainingTime -= deltaTime;
                if (lifetime.RemainingTime <= 0)
                {
                    entitiesToDestroy.Add(entity);
                }
            });

            foreach (var entity in entitiesToDestroy)
            {
                _world.Destroy(entity);
            }
        }
    }
}
