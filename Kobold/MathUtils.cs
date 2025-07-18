using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core
{
    public static class MathUtils
    {
        public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
        {
            return new Vector2(
                Math.Clamp(value.X, min.X, max.X),
                Math.Clamp(value.Y, min.Y, max.Y)
            );
        }

        public static float RandomRange(float min, float max)
        {
            return min + (Random.Shared.NextSingle() * (max - min));
        }

        public static Vector2 RandomDirection()
        {
            float angle = Random.Shared.NextSingle() * MathF.PI * 2;
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }
    }
}
