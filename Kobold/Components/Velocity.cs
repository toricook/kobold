using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    public struct Velocity
    {
        public Vector2 Value;

        public Velocity(Vector2 value)
        {
            Value = value;
        }

        public Velocity(float x, float y)
        {
            Value = new Vector2(x, y);
        }
    }
}
