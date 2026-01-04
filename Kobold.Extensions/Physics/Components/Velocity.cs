using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Extensions.Physics.Components
{
    /// <summary>
    /// Component representing an entity's velocity (speed and direction) in 2D space.
    /// This is the rate of change of position - how fast and in what direction 
    /// the entity is moving per second.
    /// 
    /// Used by physics systems to update an entity's Transform.Position each frame.
    /// The basic movement equation is: newPosition = oldPosition + (velocity * deltaTime)
    /// 
    /// Velocity is measured in pixels per second in the same coordinate system as Transform.Position.
    /// Positive X = moving right, Positive Y = moving down (standard screen coordinates).
    /// 
    /// </summary>
    public struct Velocity
    {
        /// <summary>
        /// The velocity vector in pixels per second.
        /// 
        /// X component: Horizontal velocity (positive = moving right, negative = moving left)
        /// Y component: Vertical velocity (positive = moving down, negative = moving up)
        /// 
        /// Magnitude of the vector represents speed: Vector2.Length() gives pixels/second
        /// Direction of the vector represents movement direction: Vector2.Normalize() gives unit direction
        /// 
        /// Examples:
        /// - (100, 0) = moving right at 100 pixels/second
        /// - (0, -50) = moving up at 50 pixels/second  
        /// - (70.7, 70.7) = moving down-right at ~100 pixels/second (diagonal)
        /// - (0, 0) = not moving (stationary)
        /// </summary>
        public Vector2 Value;

        /// <summary>
        /// Creates a new Velocity component with the specified velocity vector.
        /// </summary>
        /// <param name="value">Velocity in pixels per second (X=horizontal, Y=vertical)</param>
        public Velocity(Vector2 value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new Velocity component with the specified horizontal and vertical components.
        /// Convenience constructor for when you have separate X and Y velocities.
        /// </summary>
        /// <param name="x">Horizontal velocity in pixels per second (positive = right)</param>
        /// <param name="y">Vertical velocity in pixels per second (positive = down)</param>
        public Velocity(float x, float y)
        {
            Value = new Vector2(x, y);
        }

        /// <summary>
        /// Gets the speed (magnitude) of this velocity in pixels per second.
        /// This is always a positive value representing how fast the entity is moving,
        /// regardless of direction.
        /// </summary>
        public readonly float Speed => Value.Length();

        /// <summary>
        /// Gets the direction of movement as a normalized vector (length = 1).
        /// Returns Vector2.Zero if the entity is not moving (velocity is zero).
        /// Useful for determining which way an entity is facing or for applying forces
        /// in the direction of movement.
        /// </summary>
        public readonly Vector2 Direction
        {
            get
            {
                if (Value.LengthSquared() > 0.001f) // Avoid division by zero
                    return Vector2.Normalize(Value);
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Gets whether this entity is currently moving (has non-zero velocity).
        /// Uses a small threshold to account for floating-point precision issues.
        /// </summary>
        public readonly bool IsMoving => Value.LengthSquared() > 0.001f;

        /// <summary>
        /// Sets the velocity to move in the specified direction at the specified speed.
        /// </summary>
        /// <param name="direction">Direction to move (will be normalized automatically)</param>
        /// <param name="speed">Speed in pixels per second</param>
        public void SetDirectionAndSpeed(Vector2 direction, float speed)
        {
            if (direction.LengthSquared() > 0.001f)
                Value = Vector2.Normalize(direction) * speed;
            else
                Value = Vector2.Zero;
        }

        /// <summary>
        /// Adds the specified velocity to the current velocity.
        /// Useful for applying forces, impulses, or acceleration effects.
        /// </summary>
        /// <param name="deltaVelocity">Velocity change to apply</param>
        public void Add(Vector2 deltaVelocity)
        {
            Value += deltaVelocity;
        }

        /// <summary>
        /// Limits the velocity to the specified maximum speed.
        /// Maintains direction but caps the magnitude. Useful for implementing
        /// maximum movement speeds or terminal velocity.
        /// </summary>
        /// <param name="maxSpeed">Maximum speed in pixels per second</param>
        public void ClampToMaxSpeed(float maxSpeed)
        {
            if (Speed > maxSpeed && maxSpeed > 0)
            {
                Value = Direction * maxSpeed;
            }
        }

        /// <summary>
        /// Creates a velocity moving in the specified direction at the specified speed.
        /// </summary>
        /// <param name="direction">Direction vector (will be normalized)</param>
        /// <param name="speed">Speed in pixels per second</param>
        /// <returns>Velocity component with the specified direction and speed</returns>
        public static Velocity FromDirectionAndSpeed(Vector2 direction, float speed)
        {
            if (direction.LengthSquared() > 0.001f)
                return new Velocity(Vector2.Normalize(direction) * speed);
            return new Velocity(Vector2.Zero);
        }

        /// <summary>
        /// Creates a velocity from an angle and speed.
        /// </summary>
        /// <param name="angleRadians">Angle in radians (0 = right, π/2 = down)</param>
        /// <param name="speed">Speed in pixels per second</param>
        /// <returns>Velocity component with the specified angle and speed</returns>
        public static Velocity FromAngleAndSpeed(float angleRadians, float speed)
        {
            return new Velocity(
                MathF.Cos(angleRadians) * speed,
                MathF.Sin(angleRadians) * speed
            );
        }

        /// <summary>
        /// Creates a stationary velocity (no movement).
        /// </summary>
        public static Velocity Zero => new Velocity(Vector2.Zero);

        /// <summary>
        /// Returns a human-readable string representation of this velocity.
        /// Useful for debugging and logging.
        /// </summary>
        public override readonly string ToString()
        {
            return $"Velocity({Value.X:F1}, {Value.Y:F1}) [Speed: {Speed:F1} px/s]";
        }
    }
}
