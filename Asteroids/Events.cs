using Arch.Core;
using Asteroids.Components;
using Kobold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids.Events
{
    /// <summary>
    /// Event fired when a weapon is fired
    /// </summary>
    public class WeaponFiredEvent : BaseEvent
    {
        public Entity Shooter { get; }
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public Entity Bullet { get; }

        public WeaponFiredEvent(Entity shooter, Vector2 position, Vector2 direction, Entity bullet)
        {
            Shooter = shooter;
            Position = position;
            Direction = direction;
            Bullet = bullet;
        }
    }

    /// <summary>
    /// Event fired when a bullet hits something
    /// </summary>
    public class BulletHitEvent : BaseEvent
    {
        public Entity Bullet { get; }
        public Entity Target { get; }
        public Vector2 HitPosition { get; }

        public BulletHitEvent(Entity bullet, Entity target, Vector2 hitPosition)
        {
            Bullet = bullet;
            Target = target;
            HitPosition = hitPosition;
        }
    }

    /// <summary>
    /// Event fired when a bullet expires naturally (lifetime)
    /// </summary>
    public class BulletExpiredEvent : BaseEvent
    {
        public Entity Bullet { get; }
        public Vector2 Position { get; }

        public BulletExpiredEvent(Entity bullet, Vector2 position)
        {
            Bullet = bullet;
            Position = position;
        }
    }

    /// <summary>
    /// Event fired when an asteroid is destroyed
    /// </summary>
    public class AsteroidDestroyedEvent : BaseEvent
    {
        public Entity Asteroid { get; }
        public AsteroidSize Size { get; }
        public Vector2 Position { get; }
        public int ScoreValue { get; }

        public AsteroidDestroyedEvent(Entity asteroid, AsteroidSize size, Vector2 position, int scoreValue)
        {
            Asteroid = asteroid;
            Size = size;
            Position = position;
            ScoreValue = scoreValue;
        }
    }

    /// <summary>
    /// Event fired when a new asteroid is spawned
    /// </summary>
    public class AsteroidSpawnedEvent : BaseEvent
    {
        public Entity Asteroid { get; }
        public AsteroidSize Size { get; }
        public Vector2 Position { get; }

        public AsteroidSpawnedEvent(Entity asteroid, AsteroidSize size, Vector2 position)
        {
            Asteroid = asteroid;
            Size = size;
            Position = position;
        }
    }

    /// <summary>
    /// Event fired when a new wave starts
    /// </summary>
    public class WaveStartedEvent : BaseEvent
    {
        public int WaveNumber { get; }
        public int AsteroidCount { get; }

        public WaveStartedEvent(int waveNumber, int asteroidCount)
        {
            WaveNumber = waveNumber;
            AsteroidCount = asteroidCount;
        }
    }

    /// <summary>
    /// Event fired when all asteroids in a wave are destroyed
    /// </summary>
    public class WaveCompletedEvent : BaseEvent
    {
        public int WaveNumber { get; }

        public WaveCompletedEvent(int waveNumber)
        {
            WaveNumber = waveNumber;
        }
    }

    /// <summary>
    /// Event fired when the player ship is hit by an asteroid
    /// </summary>
    public class ShipHitEvent : BaseEvent
    {
        public Entity Ship { get; }
        public Entity Asteroid { get; }
        public Vector2 HitPosition { get; }

        public ShipHitEvent(Entity ship, Entity asteroid, Vector2 hitPosition)
        {
            Ship = ship;
            Asteroid = asteroid;
            HitPosition = hitPosition;
        }
    }
}
