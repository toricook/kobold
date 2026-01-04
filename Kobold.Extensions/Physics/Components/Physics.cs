using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Extensions.Physics.Components
{
    /// <summary>
    /// Angular velocity component for rotating entities
    /// </summary>
    public struct AngularVelocity
    {
        public float Value; // radians per second

        public AngularVelocity(float value)
        {
            Value = value;
        }

        /// <summary>
        /// Create angular velocity from degrees per second
        /// </summary>
        public static AngularVelocity FromDegrees(float degreesPerSecond)
        {
            return new AngularVelocity(degreesPerSecond * MathF.PI / 180f);
        }

        /// <summary>
        /// Get angular velocity in degrees per second
        /// </summary>
        public float GetDegrees()
        {
            return Value * 180f / MathF.PI;
        }
    }

    /// <summary>
    /// Thruster component for entities that can apply thrust
    /// </summary>
    public struct Thruster
    {
        public float Power;     // Thrust force magnitude
        public bool IsActive;   // Whether thrust is currently being applied

        public Thruster(float power, bool isActive = false)
        {
            Power = power;
            IsActive = isActive;
        }
    }

    /// <summary>
    /// Drag/friction component for air/space resistance
    /// </summary>
    public struct Drag
    {
        public float Linear;  // Linear drag coefficient (0 = no drag, 1 = full stop)
        public float Angular; // Angular drag coefficient

        public Drag(float linear, float angular = 0f)
        {
            Linear = linear;
            Angular = angular;
        }

        /// <summary>
        /// Space-like drag (very minimal)
        /// </summary>
        public static Drag Space => new Drag(0.001f, 0.002f);

        /// <summary>
        /// Atmosphere-like drag (more noticeable)
        /// </summary>
        public static Drag Atmosphere => new Drag(0.1f, 0.05f);

        /// <summary>
        /// Water-like drag (heavy)
        /// </summary>
        public static Drag Water => new Drag(0.3f, 0.2f);
    }

    /// <summary>
    /// Maximum speed constraint for entities
    /// </summary>
    public struct MaxSpeed
    {
        public float Value;

        public MaxSpeed(float value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Enhanced physics properties
    /// </summary>
    public struct Physics
    {
        public float Mass;
        public float Damping; // Legacy support - prefer Drag component
        public bool IsStatic; // If true, physics won't affect this entity
        public float Restitution; // Bounciness (0 = no bounce, 1 = perfect bounce)

        public Physics(float mass = 1f, float damping = 0f, bool isStatic = false, float restitution = 0f)
        {
            Mass = mass;
            Damping = damping;
            IsStatic = isStatic;
            Restitution = restitution;
        }

        /// <summary>
        /// Static object (infinite mass, doesn't move)
        /// </summary>
        public static Physics Static => new Components.Physics(float.PositiveInfinity, 0f, true);

        /// <summary>
        /// Light object (moves easily)
        /// </summary>
        public static Physics Light => new Components.Physics(0.5f);

        /// <summary>
        /// Heavy object (harder to move)
        /// </summary>
        public static Physics Heavy => new Components.Physics(5f);
    }

    /// <summary>
    /// Acceleration component for entities that need forces applied over time
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

    /// <summary>
    /// Angular acceleration component
    /// </summary>
    public struct AngularAcceleration
    {
        public float Value; // radians per second squared

        public AngularAcceleration(float value)
        {
            Value = value;
        }

        /// <summary>
        /// Create from degrees per second squared
        /// </summary>
        public static AngularAcceleration FromDegrees(float degreesPerSecondSquared)
        {
            return new AngularAcceleration(degreesPerSecondSquared * MathF.PI / 180f);
        }
    }

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

        public static Gravity Up(float strength = 981f)
        {
            return new Gravity(0, -strength);
        }

        public static Gravity Left(float strength = 981f)
        {
            return new Gravity(-strength, 0);
        }

        public static Gravity Right(float strength = 981f)
        {
            return new Gravity(strength, 0);
        }
    }
}