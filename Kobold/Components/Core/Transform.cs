using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Core component representing an entity's position, rotation, and scale in 2D world space.
    /// This is the fundamental spatial component that most other systems depend on - rendering,
    /// physics, collision detection, etc. all use Transform to determine where things are.
    /// 
    /// Position is in world coordinates (pixels), rotation is in radians, and scale is a multiplier
    /// where Vector2.One (1,1) represents normal size.
    /// </summary>
    public struct Transform
    {
        /// <summary>
        /// World position in 2D space, measured in pixels from the origin (0,0).
        /// Typically (0,0) represents the top-left corner of the screen, with X increasing
        /// to the right and Y increasing downward (standard screen coordinates).
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Rotation angle in radians around the entity's center point.
        /// 0 radians = pointing right (+X direction)
        /// π/2 radians (90°) = pointing down (+Y direction)  
        /// π radians (180°) = pointing left (-X direction)
        /// 3π/2 radians (270°) = pointing up (-Y direction)
        /// 
        /// Used by rendering systems for sprite rotation and physics systems for directional movement.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// Scale multiplier for rendering and collision detection.
        /// Vector2.One (1,1) = normal size
        /// (2,2) = double size in both dimensions
        /// (0.5,0.5) = half size in both dimensions
        /// (1,2) = normal width, double height
        /// 
        /// Affects both visual rendering and collision bounds. A scale of (0,0) would make
        /// the entity invisible and have no collision.
        /// </summary>
        public Vector2 Scale;

        /// <summary>
        /// Creates a new Transform with the specified position, rotation, and scale.
        /// </summary>
        /// <param name="position">World position in pixels</param>
        /// <param name="rotation">Rotation angle in radians (default: 0 = facing right)</param>
        /// <param name="scale">Scale multiplier (default: Vector2.One = normal size)</param>
        public Transform(Vector2 position, float rotation = 0f, Vector2 scale = default)
        {
            Position = position;
            Rotation = rotation;
            // If no scale provided, default to normal size (1,1)
            Scale = scale == default ? Vector2.One : scale;
        }

        /// <summary>
        /// Creates a Transform at the specified position with default rotation and scale.
        /// Convenience constructor for the most common use case.
        /// </summary>
        /// <param name="x">X coordinate in world space</param>
        /// <param name="y">Y coordinate in world space</param>
        public Transform(float x, float y) : this(new Vector2(x, y))
        {
        }

        /// <summary>
        /// Gets the forward direction vector based on the current rotation.
        /// This is useful for movement systems that need to move "forward" relative to rotation.
        /// </summary>
        /// <returns>Normalized direction vector pointing in the rotation direction</returns>
        public readonly Vector2 Forward
        {
            get
            {
                return new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
            }
        }

        /// <summary>
        /// Gets the right direction vector based on the current rotation.
        /// Perpendicular to the forward direction, useful for strafing movement.
        /// </summary>
        /// <returns>Normalized direction vector pointing 90° clockwise from forward</returns>
        public readonly Vector2 Right
        {
            get
            {
                return new Vector2(-MathF.Sin(Rotation), MathF.Cos(Rotation));
            }
        }

        /// <summary>
        /// Moves the transform by the specified offset vector.
        /// </summary>
        /// <param name="offset">Distance to move in world coordinates</param>
        public void Translate(Vector2 offset)
        {
            Position += offset;
        }

        /// <summary>
        /// Moves the transform by the specified distances.
        /// </summary>
        /// <param name="deltaX">Distance to move along X axis</param>
        /// <param name="deltaY">Distance to move along Y axis</param>
        public void Translate(float deltaX, float deltaY)
        {
            Position += new Vector2(deltaX, deltaY);
        }

        /// <summary>
        /// Rotates the transform by the specified angle.
        /// </summary>
        /// <param name="deltaRotation">Angle to rotate by, in radians</param>
        public void Rotate(float deltaRotation)
        {
            Rotation += deltaRotation;
        }

        /// <summary>
        /// Sets the rotation to point toward the specified target position.
        /// Useful for aiming mechanics or making entities face their movement direction.
        /// </summary>
        /// <param name="targetPosition">World position to face toward</param>
        public void LookAt(Vector2 targetPosition)
        {
            Vector2 direction = targetPosition - Position;
            Rotation = MathF.Atan2(direction.Y, direction.X);
        }

        /// <summary>
        /// Creates a transformation matrix representing this transform's position, rotation, and scale.
        /// Used by advanced rendering systems for GPU-accelerated transformations.
        /// </summary>
        /// <returns>4x4 transformation matrix for 2D graphics</returns>
        public readonly Matrix4x4 ToMatrix()
        {
            return Matrix4x4.CreateScale(Scale.X, Scale.Y, 1.0f) *
                   Matrix4x4.CreateRotationZ(Rotation) *
                   Matrix4x4.CreateTranslation(Position.X, Position.Y, 0.0f);
        }

        /// <summary>
        /// Converts rotation from radians to degrees for debugging or display purposes.
        /// </summary>
        /// <returns>Rotation angle in degrees</returns>
        public readonly float GetRotationDegrees()
        {
            return Rotation * 180f / MathF.PI;
        }

        /// <summary>
        /// Sets the rotation from degrees instead of radians.
        /// Convenience method for easier setup during development.
        /// </summary>
        /// <param name="degrees">Rotation angle in degrees</param>
        public void SetRotationDegrees(float degrees)
        {
            Rotation = degrees * MathF.PI / 180f;
        }

        /// <summary>
        /// Returns a human-readable string representation of this transform.
        /// Useful for debugging and logging.
        /// </summary>
        public override readonly string ToString()
        {
            return $"Transform(Pos: {Position}, Rot: {GetRotationDegrees():F1}°, Scale: {Scale})";
        }

        /// <summary>
        /// Creates a Transform positioned at the world origin with default rotation and scale.
        /// </summary>
        public static Transform Identity => new Transform(Vector2.Zero, 0f, Vector2.One);

        /// <summary>
        /// Creates a Transform at the specified position with default rotation and scale.
        /// </summary>
        /// <param name="position">World position</param>
        /// <returns>Transform at the specified position</returns>
        public static Transform AtPosition(Vector2 position) => new Transform(position);

        /// <summary>
        /// Creates a Transform at the specified position with the specified rotation.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="rotationDegrees">Rotation in degrees</param>
        /// <returns>Transform with position and rotation set</returns>
        public static Transform AtPositionAndRotation(Vector2 position, float rotationDegrees)
        {
            return new Transform(position, rotationDegrees * MathF.PI / 180f, Vector2.One);
        }
    }
}