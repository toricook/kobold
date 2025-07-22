using Kobold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    public class AsteroidDestroyedEvent : BaseEvent
    {
        public int AsteroidSize { get; }
        public Vector2 Position { get; }
        public int ScoreValue { get; }
    }

    public class ShipDestroyedEvent : BaseEvent
    {
        public Vector2 Position { get; }
        public int LivesRemaining { get; }
    }

    public class WaveCompletedEvent : BaseEvent
    {
        public int WaveNumber { get; }
        public int NextWaveAsteroidCount { get; }
    }

    public class WeaponFiredEvent : BaseEvent
    {
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
    }
}
