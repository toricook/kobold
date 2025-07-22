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

        /// <summary>
        /// Moves a value towards a target by a maximum distance
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="maxDistanceDelta">Maximum distance to move</param>
        /// <returns>New value moved towards target</returns>
        public static float MoveTowards(float current, float target, float maxDistanceDelta)
        {
            float difference = target - current;

            if (Math.Abs(difference) <= maxDistanceDelta)
            {
                return target;
            }

            return current + Math.Sign(difference) * maxDistanceDelta;
        }

        /// <summary>
        /// Moves a Vector2 towards a target by a maximum distance
        /// </summary>
        /// <param name="current">Current position</param>
        /// <param name="target">Target position</param>
        /// <param name="maxDistanceDelta">Maximum distance to move</param>
        /// <returns>New position moved towards target</returns>
        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
        {
            Vector2 difference = target - current;
            float magnitude = difference.Length();

            if (magnitude <= maxDistanceDelta || magnitude == 0f)
            {
                return target;
            }

            return current + difference / magnitude * maxDistanceDelta;
        }

        /// <summary>
        /// Linearly interpolates between two values
        /// </summary>
        /// <param name="a">Start value</param>
        /// <param name="b">End value</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>Interpolated value</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Clamp(t, 0f, 1f);
        }

        /// <summary>
        /// Linearly interpolates between two Vector2 values
        /// </summary>
        /// <param name="a">Start value</param>
        /// <param name="b">End value</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>Interpolated value</returns>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t
            );
        }

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        /// <param name="degrees">Angle in degrees</param>
        /// <returns>Angle in radians</returns>
        public static float DegreesToRadians(float degrees)
        {
            return degrees * MathF.PI / 180f;
        }

        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        /// <param name="radians">Angle in radians</param>
        /// <returns>Angle in degrees</returns>
        public static float RadiansToDegrees(float radians)
        {
            return radians * 180f / MathF.PI;
        }

        /// <summary>
        /// Wraps an angle to be within -PI to PI range
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Wrapped angle</returns>
        public static float WrapAngle(float angle)
        {
            while (angle > MathF.PI)
                angle -= 2 * MathF.PI;
            while (angle < -MathF.PI)
                angle += 2 * MathF.PI;
            return angle;
        }

        /// <summary>
        /// Gets the shortest angle difference between two angles
        /// </summary>
        /// <param name="from">From angle in radians</param>
        /// <param name="to">To angle in radians</param>
        /// <returns>Shortest angle difference</returns>
        public static float AngleDifference(float from, float to)
        {
            float difference = to - from;
            return WrapAngle(difference);
        }
    }
}

