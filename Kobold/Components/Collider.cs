using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    public struct BoxCollider
    {
        public Vector2 Size;
        public Vector2 Offset;

        public BoxCollider(Vector2 size, Vector2 offset = default)
        {
            Size = size;
            Offset = offset;
        }
    }

    public struct CircleCollider
    {
        public float Radius;
        public Vector2 Offset;

        public CircleCollider(float radius, Vector2 offset = default)
        {
            Radius = radius;
            Offset = offset;
        }
    }
}
