using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Adds gravity to an entity
    /// </summary>
    public struct Gravity
    {
        public Vector2 Force;

        public Gravity(Vector2 force)
        {
            Force = force;
        }

        public Gravity(float x, float y)
        {
            Force = new Vector2(x, y);
        }

        public static Gravity Down(float strength = 981f)
        {
            return new Gravity(0, strength);
        }
    }

    /// <summary>
    /// General physics properties for an entity
    /// </summary>
    public struct Physics
    {
        public float Mass;
        public float Damping; // 0 = no damping, 1 = full damping
        public bool IsStatic; // If true, physics won't affect this entity

        public Physics(float mass = 1f, float damping = 0f, bool isStatic = false)
        {
            Mass = mass;
            Damping = damping;
            IsStatic = isStatic;
        }
    }

    /// <summary>
    /// Acceleration component for entities that need forces applied
    /// </summary>
    public struct Acceleration
    {
        public Vector2 Value;

        public Acceleration(Vector2 value)
        {
            Value = value;
        }

        public Acceleration(float x, float y)
        {
            Value = new Vector2(x, y);
        }
    }
}
