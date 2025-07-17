using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    public struct Transform
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Scale;

        public Transform(Vector2 position, float rotation = 0f, Vector2 scale = default)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale == default ? Vector2.One : scale;
        }
    }
}
