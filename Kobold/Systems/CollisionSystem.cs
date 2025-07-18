using Arch.Core;
using Kobold.Core.Components;
using Kobold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    /// <summary>
    /// A system that does basic AABB collision detection on all BoxColliders
    /// </summary>
    public class CollisionSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        public CollisionSystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;
        }

        public void Update(float deltaTime)
        {
            // Generic AABB collision detection
            var colliders = new List<(Entity entity, Vector2 position, Vector2 size)>();

            var colliderQuery = new QueryDescription().WithAll<Transform, BoxCollider>();
            _world.Query(in colliderQuery, (Entity entity, ref Transform transform, ref BoxCollider collider) =>
            {
                colliders.Add((entity, transform.Position, collider.Size));
            });

            // Check all pairs for collisions
            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = i + 1; j < colliders.Count; j++)
                {
                    var (entity1, pos1, size1) = colliders[i];
                    var (entity2, pos2, size2) = colliders[j];

                    if (IsColliding(pos1, size1, pos2, size2))
                    {
                        _eventBus.Publish(new CollisionEvent(entity1, entity2));
                    }
                }
            }
        }

        private bool IsColliding(Vector2 pos1, Vector2 size1, Vector2 pos2, Vector2 size2)
        {
            return pos1.X < pos2.X + size2.X &&
                   pos1.X + size1.X > pos2.X &&
                   pos1.Y < pos2.Y + size2.Y &&
                   pos1.Y + size1.Y > pos2.Y;
        }
    }
}
