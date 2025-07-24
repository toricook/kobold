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

    /// <summary>
    /// Event fired when score changes
    /// </summary>
    public class ScoreChangedEvent : BaseEvent
    {
        public int NewScore { get; }
        public int PointsAdded { get; }
        public Vector2 Position { get; } // Where the points were earned

        public ScoreChangedEvent(int newScore, int pointsAdded, Vector2 position)
        {
            NewScore = newScore;
            PointsAdded = pointsAdded;
            Position = position;
        }
    }

    /// <summary>
    /// Event fired when a new high score is achieved
    /// </summary>
    public class HighScoreEvent : BaseEvent
    {
        public int NewHighScore { get; }
        public int PointsOver { get; }

        public HighScoreEvent(int newHighScore, int pointsOver)
        {
            NewHighScore = newHighScore;
            PointsOver = pointsOver;
        }
    }

    /// <summary>
    /// Event fired for bonus score events
    /// </summary>
    public class BonusScoreEvent : BaseEvent
    {
        public int Points { get; }
        public string Reason { get; }

        public BonusScoreEvent(int points, string reason)
        {
            Points = points;
            Reason = reason;
        }
    }

    /// <summary>
    /// Event fired when lives count changes
    /// </summary>
    public class LivesChangedEvent : BaseEvent
    {
        public int NewLives { get; }
        public int PreviousLives { get; }

        public LivesChangedEvent(int newLives, int previousLives)
        {
            NewLives = newLives;
            PreviousLives = previousLives;
        }
    }

    /// <summary>
    /// Event fired when respawn process starts
    /// </summary>
    public class RespawnStartedEvent : BaseEvent
    {
        public float RespawnDelay { get; }

        public RespawnStartedEvent(float respawnDelay)
        {
            RespawnDelay = respawnDelay;
        }
    }

    /// <summary>
    /// Event fired when ship respawns
    /// </summary>
    public class ShipRespawnedEvent : BaseEvent
    {
        public Entity Ship { get; }
        public Vector2 Position { get; }
        public int LivesRemaining { get; }

        public ShipRespawnedEvent(Entity ship, Vector2 position, int livesRemaining)
        {
            Ship = ship;
            Position = position;
            LivesRemaining = livesRemaining;
        }
    }

    /// <summary>
    /// Event fired when game is over (no lives left)
    /// </summary>
    public class GameOverEvent : BaseEvent
    {
        public int FinalLives { get; }
        public int LivesRemaining { get; }

        public GameOverEvent(int finalLives, int livesRemaining)
        {
            FinalLives = finalLives;
            LivesRemaining = livesRemaining;
        }
    }
}
