using Arch.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Events
{
    public class CollisionEvent : BaseEvent
    {
        public Entity Entity1 { get; }
        public Entity Entity2 { get; }

        public CollisionEvent(Entity entity1, Entity entity2)
        {
            Entity1 = entity1;
            Entity2 = entity2;
        }
    }

    public class BoundaryCollisionEvent : BaseEvent
    {
        public Entity Entity { get; }
        public BoundaryType Boundary { get; }

        public BoundaryCollisionEvent(Entity entity, BoundaryType boundary)
        {
            Entity = entity;
            Boundary = boundary;
        }
    }

    public enum BoundaryType
    {
        Top, Bottom, Left, Right
    }

}
