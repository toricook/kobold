using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids.Components
{
    public struct Ship
    {
        public float ThrustPower;
        public float RotationSpeed; // radians per second
        public float MaxSpeed;

        public Ship(float thrustPower, float rotationSpeed, float maxSpeed)
        {
            ThrustPower = thrustPower;
            RotationSpeed = rotationSpeed;
            MaxSpeed = maxSpeed;
        }

        /// <summary>
        /// Create a ship with rotation speed in degrees per second (more intuitive)
        /// </summary>
        public static Ship Create(float thrustPower, float rotationDegreesPerSecond, float maxSpeed)
        {
            float rotationRadiansPerSecond = rotationDegreesPerSecond * MathF.PI / 180f;
            return new Ship(thrustPower, rotationRadiansPerSecond, maxSpeed);
        }
    }

    public struct Asteroid
    {
        public AsteroidSize Size;
        public int ScoreValue;
        public float RotationSpeed; // How fast the asteroid spins

        public Asteroid(AsteroidSize size, int scoreValue, float rotationSpeed = 0f)
        {
            Size = size;
            ScoreValue = scoreValue;
            RotationSpeed = rotationSpeed;
        }
    }

    public enum AsteroidSize
    {
        Large = 0,
        Medium = 1,
        Small = 2
    }

    public struct Weapon
    {
        public float FireRate; // bullets per second
        public float LastFired; // time since last shot
        public float BulletSpeed;
        public float BulletLifetime;

        public Weapon(float fireRate, float bulletSpeed, float bulletLifetime)
        {
            FireRate = fireRate;
            LastFired = 0f;
            BulletSpeed = bulletSpeed;
            BulletLifetime = bulletLifetime;
        }
    }

    public struct Lives
    {
        public int Count;

        public Lives(int count)
        {
            Count = count;
        }
    }

    public struct Score
    {
        public int Value;

        public Score(int value)
        {
            Value = value;
        }
    }

    public struct Invulnerable
    {
        public float RemainingTime;

        public Invulnerable(float duration)
        {
            RemainingTime = duration;
        }
    }

    // Visual effects components
    public struct Particle
    {
        public Vector2 StartColor; // Using Vector2 to represent color (R, G) for now - you might want a proper Color struct
        public Vector2 EndColor;
        public float StartSize;
        public float EndSize;
        public float Age;
        public float MaxAge;

        public Particle(float maxAge, float startSize, float endSize)
        {
            StartColor = Vector2.One; // White
            EndColor = Vector2.Zero;  // Black
            StartSize = startSize;
            EndSize = endSize;
            Age = 0f;
            MaxAge = maxAge;
        }
    }

    public struct ThrusterEffect
    {
        public bool IsVisible;
        public Vector2 Color; // Simple color representation
        public float Intensity; // 0-1 for animation/pulsing

        public ThrusterEffect(bool isVisible = false, float intensity = 1f)
        {
            IsVisible = isVisible;
            Color = Vector2.One; // White by default
            Intensity = intensity;
        }
    }
}